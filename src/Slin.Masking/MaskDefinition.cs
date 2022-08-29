using System.Collections.Generic;
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
		string ValuePattern { get; set; }
		//bool SetEmpty { get; set; } //todo maybe no need this
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
	}


	internal class ValueFormatter : IValueFormatter
	{
		private readonly IMaskingContext _context;

		public string Name { get; set; }
		public string Format { get; set; }
		public string ValuePattern { get; set; } = "";

		public bool HasValuePatterned => !string.IsNullOrEmpty(ValuePattern) && _context.IsLikePattern(ValuePattern);

		//public bool SetEmpty { get; set; } //todo maybe no need this
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
			//SetEmpty = valueFormatterDefinition.SetEmpty;
		}

		public bool ValueMatchesPattern(string value)//, bool defaultWhenNotPatterned =true)
		{
			if (string.IsNullOrEmpty(value))
				return false;
			if (string.IsNullOrEmpty(ValuePattern))
				return true;

			if (_context.IsLikePattern(ValuePattern))
			{
				return _context.GetRequiredRegex(ValuePattern).IsMatch(value);
			}

			return value == ValuePattern; //todo case-insensitive
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
				if (!string.IsNullOrEmpty(ValuePattern) && ValuePattern != value)
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


	public class MaskRuleDefinition
	{
		public string Description { get; set; }

		//public List<string> FieldNames { get; set; } = new List<string>(1);
		public string KeyName { get; set; } = "";

		public List<ValueFormatterDefinition> Formatters { get; set; }
	}
}
