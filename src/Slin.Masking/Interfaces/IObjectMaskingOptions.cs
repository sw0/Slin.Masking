using System.Collections.Generic;

namespace Slin.Masking
{
	public interface IObjectMaskingOptions
	{
		/// <summary>
		/// sometimes, we need to mask number in json, for example, balance of an banking account is sensitive.
		/// </summary>
		bool MaskJsonNumberEnabled { get; }
		/// <summary>
		/// if enabled, it will try to parse value as JSON for keys found in <see cref="SerializedKeys"/>. If parsed successfully, it will do the masking recursively for the parsed object. Working with <see cref="SerializedKeys"/>, <see cref="JsonMinLength"/>
		/// </summary>
		bool MaskJsonSerializedEnabled { get; }
		/// <summary>
		/// default: true. Indicates to make deserialized string as JSON and keep it as JSON or unchanged as string.
		/// if true, the serialized string will be parsed as Json node if it's a valid JSON, which will be more friendly in reading.
		/// </summary>
		bool MaskJsonSerializedParsedAsNode { get; }

        /// <summary>
        /// default false. It works with <see cref="UrlKeys"/> and rules defined in <see cref="UrlMaskingPattern"/>.
        /// </summary>
        bool MaskUrlEnabled { get; }
        /// <summary>
        /// default: false. It works with <see cref="SerializedKeys"/>, <see cref="XmlMinLength"/>.
        /// </summary>
        bool MaskXmlSerializedEnabled { get; }
		bool MaskXmlSerializedOnXmlAttributeEnabled { get; }
		bool MaskJsonSerializedOnXmlAttributeEnabled { get; }
		bool MaskNestedKvpEnabled { get; }
		List<KeyKeyValueKey> KeyKeyValueKeys { get; }
		List<string> SerializedKeys { get; }
		bool SerializedKeysCaseSensitive { get; }
        /// <summary>
        /// specify which keys will treated as URL or Query(kvp). Will be used when <see cref="MaskUrlEnabled"/> is true.
        /// </summary>
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