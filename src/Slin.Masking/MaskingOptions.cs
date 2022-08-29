using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Slin.Masking
{

	public class MaskingOptions
	{
		public static readonly MaskingOptions Default = new MaskingOptions
		{
			PatternCheckChars = new List<char> { '?', '+', '*', '\\', '[', ']', '(', ')', '.' },
			KeyCaseInsensitive = false,
			ThrowIfNamedProfileNotFound = false,
		};

		public List<char> PatternCheckChars { get; set; }

		/// <summary>
		/// 
		/// keyName like firstName, FirstName will be treated same.
		/// <remarks>default true, reasons:
		/// 1. by default in ConfigurationProvider (AddJsonFile), although key is sensitive in json file, but it's treated case-insensitively while building provider
		/// 2. In reality, we may got log key like firstName, and FirstName. If set it to false, we need to set like "firstname":{"KeyName":"firstname}
		/// </remarks>
		/// </summary>
		public bool KeyCaseInsensitive { get; set; } = true;

		public bool ValueCaseInsensitive { get; set; } = false;

		public bool ThrowIfNamedProfileNotFound { get; internal set; }

		internal RegexOptions GetKeyNameRegexOptions()
		{
			if (KeyCaseInsensitive == true) return RegexOptions.Compiled | RegexOptions.IgnoreCase;
			return RegexOptions.Compiled;
		}
		internal RegexOptions GetValuePatternRegexOptions()
		{
			if (ValueCaseInsensitive == true) return RegexOptions.Compiled | RegexOptions.IgnoreCase;
			return RegexOptions.Compiled;
		}
	}
}
