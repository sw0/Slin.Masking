namespace Slin.Masking
{
	internal interface IValueFormatter
	{
		/// <summary>
		/// indicates the which NamedFormatter would be used.
		/// </summary>
		string Name { get; set; }

		string Format { get; set; }
		/// <summary>
		/// NOTE: here valuepattern would get append with ignore case comment (?#True/False)
		/// </summary>
		string ValuePattern { get; set; }

		/// <summary>
		/// indicates if ignore case when compare <see cref="ValuePattern"/> with the value.
		/// </summary>
		bool IgnoreCase { get; set; }


		bool TryFormat(string value, out string result);

		bool ValueMatchesPattern(string value);

		bool HasValuePatterned { get; }
	}
}
