using System.Collections.Generic;

namespace Slin.Masking
{
	public interface IObjectMaskingOptions
	{
		bool Enabled { get; }
		bool MaskJsonNumberEnabled { get; }
		bool MaskJsonSerializedEnabled { get; }
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




		string JsonAllowedCharacters { get; }

		string[] JsonAllowedUnicodeRanges { get; }

		ArrayItemHandleMode ArrayItemHandleMode { get; }
	}

	/// <summary>
	/// this only will handel string and number (if MaskJsonNumberEnabled is true)
	/// </summary>
	public enum ArrayItemHandleMode
	{
		/// <summary>
		/// default way, ignore property name. It will not mask {"ssn":["123456789"]}
		/// </summary>
		Default = 0,
		/// <summary>
		/// {"ssn":["123456789"]}, will be masked only when array item count is 1
		/// </summary>
		SingleItemAsValue = 1,
		/// <summary>
		/// {"ssn":["123456789","101123456"]}, all items will be masked
		/// </summary>
		AnyItemsAsValues = 2,
	}
}