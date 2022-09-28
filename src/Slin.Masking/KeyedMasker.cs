using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Slin.Masking
{
	public sealed class MaskingConstants
	{
		public const string PatternCaseInsensitiveSuffix = "(?#caseinsensitive)";
		public const string PatternCaseSensitiveSuffix = "(?#casesensitive)";
	}

	//public interface IKeyedRule
	//{
	//	string KeyName { get; }

	//	ModeIfArray ModeIfArray { get; }

	//}

	internal interface IKeyedMasker //: IKeyedRule
	{
		string KeyName { get; }

		string Mask(string value);

		bool IsEligibleToMask(string value);
	}

	internal class KeyedMasker : IKeyedMasker
	{
		private readonly IMaskingContext _context;

		/// <summary>
		/// NOTE: KeyName by default is case-sensitive. 
		/// If want it be case-sensitive, please use lower-cased '(?#casesensitive)' as suffix to indicate ignore case or not when using it's a regex. For example: '[first|last]name(?#casesensitive)'.
		/// </summary>
		public string KeyName { get; set; } = "";

		public List<IValueFormatter> Formatters { get; set; }

		/// <summary>
		/// if currently only supports JSON array
		/// </summary>
		public ModeIfArray ModeIfArray { get; set; }

		/// <summary>
		/// default: 30
		/// </summary>
		public int KeyNameLenLimitToCache { get; set; } = 30;

		public KeyedMasker(IMaskingContext context, MaskRuleDefinition source)
		{
			_context = context;

			var formatters = source.Formatters.Where(f => f.Enabled)
				.Select(o => new ValueFormatter(context, o));

			KeyName = source.KeyName;
			//ModeIfArray = source.ModeIfArray;

			if (source.IngoreKeyCase && !KeyName.EndsWith(MaskingConstants.PatternCaseInsensitiveSuffix))
			{
				KeyName += MaskingConstants.PatternCaseInsensitiveSuffix;
			}

			Formatters = new List<IValueFormatter>(formatters);

			KeyNameLenLimitToCache = Math.Min(_context.Options.KeyNameLenLimitToCache, source.KeyNameLenLimitToCache);
		}

		//todo this should not be
		public bool KeyNamePatterned => !string.IsNullOrEmpty(KeyName) && _context.IsLikePattern(KeyName);

		public string Mask(string value)
		{
			foreach (var formatter in Formatters)
			{
				if (formatter.TryFormat(value, out string result))
					return result;
			}

			return value;
		}

		public bool IsEligibleToMask(string value)
		{
			if (string.IsNullOrEmpty(value)) return false;
			return Formatters.Any(f => f.ValueMatchesPattern(value));
		}
	}
}
