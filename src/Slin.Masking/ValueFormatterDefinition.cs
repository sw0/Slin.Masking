namespace Slin.Masking
{
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
}
