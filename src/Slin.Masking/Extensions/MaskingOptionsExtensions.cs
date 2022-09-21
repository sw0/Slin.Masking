using System;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Slin.Masking
{
	public static class MaskingOptionsExtensions
	{
		public static TextEncoderSettings GetTextEncoderSettings(this IObjectMaskingOptions options)
		{
			TextEncoderSettings settings = new TextEncoderSettings();

			settings.AllowCharacters("<>&+".ToArray());

			if (options.JsonAllowedCharacters != null && options.JsonAllowedCharacters.Length > 0)
			{
				settings.AllowCharacters(options.JsonAllowedCharacters.ToCharArray().Distinct().ToArray());

			}
			settings.AllowRange(UnicodeRanges.BasicLatin);

			if (options.JsonAllowedUnicodeRanges != null && options.JsonAllowedUnicodeRanges.Length > 0)
			{
				var properties = typeof(UnicodeRanges).GetProperties(BindingFlags.Public | BindingFlags.Static);

				var additional = options.JsonAllowedUnicodeRanges.Distinct().Where(x => "BasicLatin".Equals(x, StringComparison.OrdinalIgnoreCase))
					.Select(p => properties.FirstOrDefault(x => x.Name.Equals(p, StringComparison.OrdinalIgnoreCase)))
					.Where(x => x != null)
					.Select(p => p.GetValue(null) as UnicodeRange)
					.ToArray();

				if (additional.Length > 0)
					settings.AllowRanges(additional);
			}

			return settings;
		}
	}
}
