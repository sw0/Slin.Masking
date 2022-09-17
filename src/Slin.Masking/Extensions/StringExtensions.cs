using System;
using System.Collections.Generic;
using System.Text;

namespace Slin.Masking
{
	public static class StringExtensions
	{
		public static string Mask(this string value, string maskFormat)
		{
			if (maskFormat.StartsWith("{0"))
			{
				var masked = string.Format(MaskFormatter.Default, maskFormat, value);
				return masked;
			}
			else
			{
				var masked = string.Format(MaskFormatter.Default, $"{{0:{maskFormat}}}", value);
				return masked;
			}
		}
	}
}
