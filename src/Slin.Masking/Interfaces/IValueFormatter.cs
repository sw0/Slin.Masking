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
}
