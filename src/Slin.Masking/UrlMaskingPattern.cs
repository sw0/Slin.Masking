namespace Slin.Masking
{
	public class UrlMaskingPattern
	{
		/// <summary>
		/// optional: name for the definition
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// default:true
		/// </summary>
		public bool Enabled { get; set; } = true;
		private string _pattern;
		/// <summary>
		/// Pattern
		/// </summary>
		public string Pattern
		{
			get { return _pattern; }
			set
			{
				if (value != _pattern)
				{
					_pattern = value;
					_cacheKey = $"{Pattern}(?#{IgnoreCase})";
				}
			}
		}

		//public string Format { get; set; } = "REDACTED";

		private bool _ignoreCase = true;
		/// <summary>
		/// default: true
		/// </summary>
		public bool IgnoreCase
		{
			get { return _ignoreCase; }
			set
			{
				if (value != _ignoreCase)
				{
					_ignoreCase = value;
					_cacheKey = $"{Pattern}(?#{value})";
				}
			}
		}

		private string _cacheKey;
		internal string CacheKey => _cacheKey;
	}
}
