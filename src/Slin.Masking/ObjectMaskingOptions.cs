using System;
using System.Collections.Generic;

namespace Slin.Masking
{
	/// <summary>
	/// ObjectMaskingOptions is options for <see cref="ObjectMasker"/>.
	/// </summary>
	public class ObjectMaskingOptions : IObjectMaskingOptions
	{
		/// <summary>
		/// enabled by default
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// default false. It works with <see cref="UrlKeys"/> and rules defined in <see cref="UrlMaskingPattern"/>.
		/// </summary>
		public bool MaskUrlEnabled { get; set; }

		/// <summary>
		/// specify which keys will treated as URL or Query(kvp). Will be used when <see cref="MaskUrlEnabled"/> is true.
		/// </summary>
		public List<string> UrlKeys { get; set; } = new List<string>();

		/// <summary>
		/// SerializedKeys with string value value will be tried deserialized and get masked if <see cref="MaskJsonSerializedEnabled"/> or <see cref="MaskXmlSerializedEnabled"/> is true.
		/// </summary>
		public List<string> SerializedKeys { get; set; } = new List<string>();

		/// <summary>
		/// default: true. It works with <see cref="SerializedKeys"/>, <see cref="JsonMinLength"/>.
		/// </summary>
		public bool MaskJsonSerializedEnabled { get; set; } = true;
		/// <summary>
		/// default: true. Indicates to make deserialized string as JSON and keep it as JSON or unchanged as string.
		/// </summary>
		public bool MaskJsonSerializedParsedAsNode { get; set; } = true;

		/// <summary>
		/// default: false. It works with <see cref="SerializedKeys"/>, <see cref="XmlMinLength"/>.
		/// </summary>
		public bool MaskXmlSerializedEnabled { get; set; } = false;
		/// <summary>
		/// default false.
		/// </summary>
		public bool MaskXmlSerializedOnXmlAttributeEnabled { get; set; } = false;
		/// <summary>
		/// default false
		/// </summary>
		public bool MaskJsonSerializedOnXmlAttributeEnabled { get; set; } = false;
		/// <summary>
		/// Will be used to indicates comparison for Serialized Keys or Url Keys against property name
		/// </summary>
		public bool SerializedKeysCaseSensitive { get; set; } = true;

		/// <summary>
		/// default:false, if you need to enable number mask, set it to true. For example, account's balance.
		/// </summary>
		public bool MaskJsonNumberEnabled { get; set; }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public bool MaskNestedKvpEnabled { get; set; }

		/// <summary>
		/// if <see cref="MaskNestedKvpEnabled"/> is enabled, and <see cref="KeyKeyValueKeys"/> is null, it will use Key,Value and key,value.
		/// </summary>
		public List<KeyKeyValueKey> KeyKeyValueKeys { get; set; } = new List<KeyKeyValueKey>(KeyKeyValueKey.DefaultKeyKeyValueKeys);

		/// <summary>
		/// Default: 3. 
		/// If value.Length is less then N, masker will bypass it, because it usually does not contain secnsitive data.
		/// For name it might be short. So set it to 3. Like firstname, lastname.
		/// </summary>
		public int ValueMinLength { get; set; } = 3;
		/// <summary>
		/// default: 30. If string length is less than N, it will bypass parsing Xml document.
		/// Because, such document is too small and probably does not contains data eligible for masking.
		/// This is used when <see cref="MaskXmlSerializedEnabled"/> is enabled.
		/// </summary>
		public int XmlMinLength { get; set; } = 30;
		/// <summary>
		/// default: 15. If string length is less than N, it will bypass parsing Json document.
		/// null or empty string is actually a simple document, probably does not contains data eligible for masking
		/// This is used when <see cref="MaskJsonSerializedEnabled"/> is enabled.
		/// </summary>
		public int JsonMinLength { get; set; } = 15;

		/// <summary>
		/// default: 64
		/// </summary>
		public int JsonMaxDepth { get; set; } = 64;//default

		/// <summary>
		/// allowed characters
		/// </summary>
		public string JsonAllowedCharacters { get; set; }
		/// <summary>
		/// allowed unicoderanges, i.e. CjkUnifiedIdeographs
		/// </summary>
		public string[] JsonAllowedUnicodeRanges { get; set; }
		/// <summary>
		/// default: Default
		/// </summary>
		public ModeIfArray GlobalModeForArray { get; set; }

		public List<UrlMaskingPattern> UrlMaskingPatterns { get; set; } = new List<UrlMaskingPattern>();

	}
}
