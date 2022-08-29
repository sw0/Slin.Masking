using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Slin.Masking
{
	public interface IKeyedMasker
	{
		string KeyName { get; }

		string Mask(string value);

		bool IsEligibleToMask(string value);
	}

	internal class KeyedMasker : IKeyedMasker
	{
		private readonly IMaskingContext _context;

		public string KeyName { get; set; } = "";

		public List<IValueFormatter> Formatters { get; set; }

		public KeyedMasker(IMaskingContext context, MaskRuleDefinition source)
		{
			_context = context;

			var formatters = source.Formatters.ConvertAll(o => new ValueFormatter(context, o));

			KeyName = source.KeyName;
			Formatters = new List<IValueFormatter>(formatters);
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
