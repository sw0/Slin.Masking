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
	}
}