using System.Collections.Generic;
using System.Text.RegularExpressions;

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
}
