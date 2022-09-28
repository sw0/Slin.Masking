using System.Collections.Generic;

namespace Slin.Masking
{
	public interface IObjectMaskingOptions
	{
		bool Enabled { get; }
		bool MaskJsonNumberEnabled { get; }
		bool MaskJsonSerializedEnabled { get; }
		/// <summary>
		/// default: true. Indicates to make deserialized string as JSON and keep it as JSON or unchanged as string.
		/// if true, the serialized string will be parsed as Json node if it's a valid JSON, which will be more friendly in reading.
		/// </summary>
		bool MaskJsonSerializedParsedAsNode { get; }

		bool MaskUrlEnabled { get; }
		bool MaskXmlSerializedEnabled { get; }
		bool MaskXmlSerializedOnXmlAttributeEnabled { get; }
		bool MaskJsonSerializedOnXmlAttributeEnabled { get; }
		bool MaskNestedKvpEnabled { get; }
		List<KeyKeyValueKey> KeyKeyValueKeys { get; }
		List<string> SerializedKeys { get; }
		bool SerializedKeysCaseSensitive { get; }
		List<string> UrlKeys { get; }

		List<UrlMaskingPattern> UrlMaskingPatterns { get; }
		/// <summary>
		/// default: 3
		/// </summary>
		int ValueMinLength { get; }
		int XmlMinLength { get; }
		int JsonMinLength { get; }
		int JsonMaxDepth { get; }

		/// <summary>
		/// default: <![CDATA[<>&+]]>
		/// </summary>
		string JsonAllowedCharacters { get; }
		/// <summary>
		/// specify additional allowed unicode ranges, which is defined in <see cref="System.Text.Unicode.UnicodeRanges"/>. UnicodeRanges.BasicLatin would always be allowed.
		/// </summary>
		string[] JsonAllowedUnicodeRanges { get; }

		/// <summary>
		/// current only support Json array.
		/// </summary>
		ModeIfArray GlobalModeForArray { get; }
	}
}