using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Slin.Masking
{
	public static class StringExtensions
	{
		public static string Mask(this string value, string maskFormat = "*")
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

		/// <summary>
		/// skip space or new line \r\n
		/// </summary>
		/// <param name="value"></param>
		/// <param name="character"></param>
		/// <returns></returns>
		public static bool StartsWithExt(this string value, char character)
		{
			if (value == null) return false;

			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == ' ') continue;
				if (value[i] == '\r') continue;
				if (value[i] == '\n') continue;
				return character == value[i];
			}
			return false;
		}

		/// <summary>
		/// skip space or new line \r\n
		/// </summary>
		/// <param name="value"></param>
		/// <param name="character"></param>
		/// <returns></returns>
		public static bool EndsWithExt(this string value, char character)
		{
			if (value == null) return false;

			for (var i = value.Length - 1; i >= 0; i--)
			{
				if (value[i] == ' ') continue;
				if (value[i] == '\r') continue;
				if (value[i] == '\n') continue;
				return character == value[i];
			}

			return false;
		}
	}
}
