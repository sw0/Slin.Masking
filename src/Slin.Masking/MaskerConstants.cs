using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Slin.Masking
{
	internal class MaskerConstants
	{
		//public static readonly JavaScriptEncoder JsonDefaultEncoder = JavaScriptEncoder.Create(
		//	UnicodeRanges.BasicLatin,
		//	UnicodeRanges.CjkCompatibility,
		//	UnicodeRanges.CjkCompatibilityForms,
		//	UnicodeRanges.CjkCompatibilityIdeographs,
		//	UnicodeRanges.CjkRadicalsSupplement,
		//	UnicodeRanges.CjkStrokes,
		//	UnicodeRanges.CjkUnifiedIdeographs,
		//	UnicodeRanges.CjkUnifiedIdeographsExtensionA,
		//	UnicodeRanges.CjkSymbolsandPunctuation,
		//	UnicodeRanges.HalfwidthandFullwidthForms);

		public static readonly TextEncoderSettings DefaultTextEncoderSettings = new TextEncoderSettings();

		static MaskerConstants()
		{
			DefaultTextEncoderSettings.AllowCharacters('\u0436', '\u0430');
			DefaultTextEncoderSettings.AllowRange(UnicodeRanges.BasicLatin);
		}
	}
}
