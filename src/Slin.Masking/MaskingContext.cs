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
		IMaskingOptions Options { get; }

		Regex GetOrAddRegex(string pattern, RegexOptions options);

		Regex GetRequiredRegex(string pattern);

		bool IsLikePattern(string fieldName);

		IKeyedMasker GetKeyedMasker(string key, string value);

		IMaskFormatter MaskFormatter { get; }

		/// <summary>
		/// [optional] Only used for URL masking.
		/// </summary>
		List<UrlMaskingPattern> UrlMaskingPatterns { get; }
	}

	internal class MaskingContext : IMaskingContext
	{
		protected Dictionary<string, ValueFormatterDefinition> NamedFormatters => Options.NamedFormatterDefintions;

		protected Dictionary<string, MaskRuleDefinition> Items => Options.Rules;

		//private readonly Dictionary<string, MaskRuleDefinition> _lookup = new Dictionary<string, MaskRuleDefinition>();

		private readonly ConcurrentDictionary<string, KeyedMasker> _pooled;

		private readonly List<KeyedMasker> _pooledPatternedKeyMaskers = new List<KeyedMasker>();

		//todo this might not be shared.
		public static readonly ConcurrentDictionary<string, Regex> PooledRegex = new ConcurrentDictionary<string, Regex>();

		/// <summary>
		/// maybe it's not good to use public here, but it's intenal use. so just keep it here
		/// </summary>
		public List<UrlMaskingPattern> UrlMaskingPatterns => Options.UrlMaskingPatterns;

		public IMaskingOptions Options { get; }

		public IMaskFormatter MaskFormatter { get; private set; }

		private bool _initalized = false;

		public MaskingContext(IMaskingOptions options, IMaskFormatter maskFormatter = null)
		{
			if(options == null)	throw new ArgumentNullException("options");

			Options = options;

			//todo print configurations

			if (Options.KeyedMaskerPoolIgnoreCase)
			{
				_pooled = new ConcurrentDictionary<string, KeyedMasker>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				_pooled = new ConcurrentDictionary<string, KeyedMasker>();
			}

			MaskFormatter = maskFormatter ?? new MaskFormatter();

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
						
						throw new Exception($"{nameof(Options.NamedFormatterDefintions)} does not found: {formatter.Name}");
					}
				}
			}

			return definition;
		}

		private void Initialize()
		{
			if (_initalized) throw new Exception("already initialized");

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
					{
						if (item.KeyName.EndsWith("(?#casesensitive)"))
							GetOrAddRegex(item.KeyName, RegexOptions.Compiled);
						else
							GetOrAddRegex(item.KeyName, RegexOptions.Compiled | RegexOptions.IgnoreCase);
					}


					int idx = 0;
					foreach (var fmter in item.Formatters)
					{
						if (fmter.HasValuePatterned)
							GetOrAddRegex(fmter.ValuePattern, fmter.IgnoreCase ? RegexOptions.Compiled | RegexOptions.IgnoreCase : RegexOptions.Compiled);

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

			if (UrlMaskingPatterns != null)
			{
				UrlMaskingPatterns.ForEach(item =>
				{
					if (!item.Enabled) return;
					if (string.IsNullOrWhiteSpace(item.Pattern)) return;

					try
					{
						var reg = new Regex(item.Pattern, RegexOptions.Compiled |
							(item.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None));

						PooledRegex.TryAdd(item.CacheKey, reg);
					}
					catch (Exception)
					{
						//todo logging						
					}
				});
			}
			#endregion

			_initalized = true;
		}

		public bool IsLikePattern(string input)
		{
			return input != null && Options.RegexCheckChars.Any(c => input.Contains(c));
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
						//TODO maybe we'd better to add limits here
						if (item.KeyNameLenLimitToCache <= 0 
							|| key.Length < item.KeyNameLenLimitToCache)
							_pooled.TryAdd(key, item);

						return item;
					}
				}
			}
			return null;
		}
	}
}
