using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Slin.Masking
{
	internal interface IValueFormatter
	{
		string Name { get; set; }

		string Format { get; set; }
		/// <summary>
		/// NOTE: here valuepattern would get append with ignore case comment (?#True/False)
		/// </summary>
		string ValuePattern { get; set; }

		bool IgnoreCase { get; set; }

		//string CacheKey { get; }

		bool TryFormat(string value, out string result);

		bool ValueMatchesPattern(string value);

		bool HasValuePatterned { get; }
	}

	public class ValueFormatterDefinition //: IValueFormatter, IValueFormatter2
	{
		/// <summary>
		/// this will be global unique name.
		/// If name is not set, it will be set by Key, it set, it will ignore other property and use NamedFormatter.
		/// </summary>
		public string Name { get; set; }

		public string Format { get; set; }

		public string ValuePattern { get; set; } = "";
		/// <summary>
		/// indicates whether valuepattern ignores case or not, so does if it's regular expression. 
		/// default: false
		/// </summary>
		public bool IgnoreCase { get; set; } = false;
		/// <summary>
		/// default true
		/// </summary>
		public bool Enabled { get; set; } = true;

		public ValueFormatterDefinition() { }

		public ValueFormatterDefinition(string format, bool ignoreCase = false)
		{
			Format = format;
			IgnoreCase = ignoreCase;
			Enabled = true;
		}
		public ValueFormatterDefinition(string format, string valuePattern, bool ignoreCase = false)
		{
			Format = format;
			ValuePattern = valuePattern;
			IgnoreCase = ignoreCase;
			Enabled = true;
		}

		/// <summary>
		/// however it does not in appsettings.json?
		/// </summary>
		/// <param name="format"></param>
		public static implicit operator ValueFormatterDefinition(string format)
		{
			return new ValueFormatterDefinition { Format = format, Enabled = true };
		}
	}


	internal class ValueFormatter : IValueFormatter
	{
		private readonly IMaskingContext _context;

		public string Name { get; set; }
		public string Format { get; set; }
		/// <summary>
		/// NOTE: here ValuePattern will got suffix of (?#True or False) if ValuePattern was provided and is like a Regular Expression.
		/// </summary>
		public string ValuePattern { get; set; } = "";

		//public string CacheKey { get; }

		public bool HasValuePatterned => !string.IsNullOrEmpty(ValuePattern) && _context.IsLikePattern(ValuePattern);

		public bool IgnoreCase { get; set; }

		public ValueFormatter(IMaskingContext context, ValueFormatterDefinition valueFormatterDefinition)
		{
			_context = context;
			Name = valueFormatterDefinition.Name;
			if (!string.IsNullOrEmpty(valueFormatterDefinition.Format))
			{
				Format = $"{{0:{valueFormatterDefinition.Format}}}";
			}
			else
			{
				Format = valueFormatterDefinition.Format;
			}
			ValuePattern = valueFormatterDefinition.ValuePattern;
			IgnoreCase = valueFormatterDefinition.IgnoreCase;

			if (HasValuePatterned)
			{
				ValuePattern = $"{ValuePattern}(?#{IgnoreCase})";
			}
		}

		public bool ValueMatchesPattern(string value)//, bool defaultWhenNotPatterned =true)
		{
			if (string.IsNullOrEmpty(value))
				return false;
			if (string.IsNullOrEmpty(ValuePattern))
				return true;

			if (HasValuePatterned)
			{
				return _context.GetRequiredRegex(ValuePattern).IsMatch(value);
			}

			if (IgnoreCase)
				return value.Equals(ValuePattern, System.StringComparison.OrdinalIgnoreCase);
			else
				return value == ValuePattern;
		}


		public bool TryFormat(string value, out string result)
		{
			//if (Format == "{0:EMPTY}") { result = ""; return true; }
			if (string.IsNullOrEmpty(value)) { result = value; return true; }

			if (HasValuePatterned)
			{
				var regex = _context.GetRequiredRegex(ValuePattern);

				if (!regex.IsMatch(value))
				{
					result = value;
					return false;
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(ValuePattern) && !value.Equals(ValuePattern, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
				{
					result = value;
					return false;
				}
			}

			if (Format == "{0:null}")
			{
				//NOTE IMPORTANT HERE that Formatter does not support null value result.
				//So use this way here.
				result = null; return true;
			}

#if DEBUG //better for debugging
			var formatter = new MaskFormatter();
			result = string.Format(formatter, Format, value);
#else
			result = string.Format(_context.MaskFormatter, Format, value);
#endif
			return true;
		}
	}
}
