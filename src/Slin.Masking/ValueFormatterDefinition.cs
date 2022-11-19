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
		/// <summary>
		/// value pattern is a string, can be regular expression. Default is empty string.
		/// This is adavnce feature, and be used to check the value matched or not, masking would be skipped if value is not matched. It has a bit performance influence.
		/// </summary>
		/// <example>
		/// \d{16} can be used to check it's an 16 digits or not, for card number validation.
		/// </example>
		public string ValuePattern { get; set; } = "";
		/// <summary>
		/// indicates whether <see cref="ValuePattern"/> ignores case or not, so does if it's regular expression. 
		/// default: false
		/// </summary>
		public bool IgnoreCase { get; set; } = false;
		/// <summary>
		/// default true
		/// </summary>
		public bool Enabled { get; set; } = true;

		public ValueFormatterDefinition() { }

		public ValueFormatterDefinition(string format)
		{
			Format = format;
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
