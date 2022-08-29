using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Slin.Masking
{
	internal interface IMaskingContext
	{
		Regex GetOrAddRegex(string pattern, RegexOptions options);

		Regex GetRequiredRegex(string pattern);

		//bool IsEligible(string actualKeyName, string fieldValue);

		bool IsLikePattern(string fieldName);

		IKeyedMasker GetKeyedMasker(string key, string value);

		IMaskFormatter MaskFormatter { get; }
	}


	internal class MaskingContext : IMaskingContext
	{
		protected Dictionary<string, ValueFormatterDefinition> NamedFormatters => _profile.NamedFormatterDefintions;

		protected Dictionary<string, MaskRuleDefinition> Items => _profile.Rules;

		//private readonly Dictionary<string, MaskRuleDefinition> _lookup = new Dictionary<string, MaskRuleDefinition>();

		private readonly ConcurrentDictionary<string, KeyedMasker> _pooled;

		private readonly List<KeyedMasker> _pooledPatternedKeyMaskers = new List<KeyedMasker>();

		//todo this might not be shared.
		public static readonly ConcurrentDictionary<string, Regex> PooledRegex = new ConcurrentDictionary<string, Regex>();

		private readonly MaskingProfile _profile;

		public IMaskFormatter MaskFormatter { get; private set; }

		private bool _initalized = false;

		public MaskingContext(MaskingProfile profile, IMaskFormatter maskFormatter = null)
		{
			_profile = profile;

			//todo print configurations

			if (_profile.Options.KeyCaseInsensitive)
			{
				_pooled = new ConcurrentDictionary<string, KeyedMasker>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				_pooled = new ConcurrentDictionary<string, KeyedMasker>();
			}

			MaskFormatter = maskFormatter ?? new MaskFormatter();

			if (_profile.Options == null)
			{
				_profile.Options = MaskingOptions.Default;
			}
			else if (_profile.Options.PatternCheckChars == null || _profile.Options.PatternCheckChars.Count == 0)
			{
				_profile.Options.PatternCheckChars = new List<char>(MaskingOptions.Default.PatternCheckChars);
			}

			Initialize();

			_pooledPatternedKeyMaskers = _pooled.Values.Where(km => km.KeyNamePatterned).ToList();
		}

		private KeyedMasker NormalizeMaskDefinition(KeyedMasker definition)
		{
			//todo 
			var namedFormatters = NamedFormatters;
			var count = definition.Formatters.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				var formatter = definition.Formatters[i];
				if (!string.IsNullOrEmpty(formatter.Name))
				{
					if (namedFormatters.ContainsKey(formatter.Name))
					{
						var found = namedFormatters[formatter.Name];
						definition.Formatters[i] = new ValueFormatter(this, found);
					}
					else
					{
						definition.Formatters.RemoveAt(i);
						if (_profile.Options.ThrowIfNamedProfileNotFound)
							throw new Exception($"{nameof(_profile.NamedFormatterDefintions)} does not found: {formatter.Name}");
					}
				}
			}

			return definition;
		}

		private void Initialize()
		{
			if (_initalized) throw new Exception("already initialized");

			RegexOptions keyRegOptions = _profile.Options.GetKeyNameRegexOptions();
			RegexOptions valRegOptions = _profile.Options.GetValuePatternRegexOptions();

			var normalized = new List<KeyedMasker>();

			foreach (var item in Items)
			{
				if (string.IsNullOrEmpty(item.Value.KeyName)) item.Value.KeyName = item.Key;

				normalized.Add(NormalizeMaskDefinition(new KeyedMasker(this, item.Value)));
			}

			#region regex cache
			foreach (var item in normalized)
			{
				if (item.Formatters.Count == 0) continue;
				try
				{
					//todo make sure not duplicated key
					_pooled.TryAdd(item.KeyName, item);

					if (IsLikePattern(item.KeyName))
						GetOrAddRegex(item.KeyName, keyRegOptions);

					int idx = 0;
					foreach (var fmter in item.Formatters)
					{
						if (IsLikePattern(fmter.ValuePattern))
							GetOrAddRegex(fmter.ValuePattern, valRegOptions);

						if (!string.IsNullOrEmpty(fmter.Format) && MaskFormatter.IsFormatMatched(fmter.Format))
						{
							//todo throw 
							throw new Exception($"Format '{fmter.Format}' defined in for key '{item.KeyName}' on position '{idx + 1}' is not valid format");
						}
						idx++;
					}
				}
				catch (Exception)
				{
					//todo ...
					throw;
				}
			}
			#endregion

			_initalized = true;
		}

		public bool IsLikePattern(string input)
		{
			return _profile.Options.PatternCheckChars.Any(c => input.Contains(c));
		}

		public Regex GetOrAddRegex(string pattern, RegexOptions options)
		{
			var regex = PooledRegex.GetOrAdd(pattern, new Regex(pattern, options));

			return regex;
		}

		public Regex GetRequiredRegex(string pattern)
		{
			if (PooledRegex.TryGetValue(pattern, out var regex)) return regex;

			throw new Exception($"Regex for pattern '{pattern}' was expected");
		}


		public IKeyedMasker GetKeyedMasker(string key, string value)
		{
			if (_pooled.TryGetValue(key, out var masker))
			{
				if (masker == null) return null;

				if (masker.IsEligibleToMask(value)) return masker;

				if (masker.Formatters.Any(f => f.ValueMatchesPattern(value)))
					return masker;
				return null;
			}
			else
			{
				foreach (var item in _pooledPatternedKeyMaskers)
				{
					if (GetRequiredRegex(item.KeyName).IsMatch(key))
					{
						_pooled.TryAdd(key, item);

						return item;
					}
				}
			}
			return null;
		}
	}

	//public static class MaskingExtensions
	//{
	//	public static bool UseDefaultMasker(this IServiceCollection definition, string name)
	//	{
	//		if (definition.FieldNamePatterned)
	//		{

	//		}
	//		else
	//		{
	//			if (name != definition.KeyName) return false;
	//		}
	//	}
	//}
}
