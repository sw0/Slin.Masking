using System;
using System.Collections.Generic;

namespace Slin.Masking
{
	public class ObjectMaskingOptions : IObjectMaskingOptions
	{
		/// <summary>
		/// enabled by default
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// default false. works with <see cref="UrlKeys"/>
		/// </summary>
		public bool MaskUrlEnabled { get; set; }

		/// <summary>
		/// specify which keys will treated as URL or Query(kvp). Will be used when <see cref="MaskUrlEnabled"/>
		/// </summary>
		public List<string> UrlKeys { get; set; } = new List<string>();

		/// <summary>
		/// SerializedKeys with string value value will be tried deserialized and get masked if <see cref="MaskJsonSerializedEnabled"/> or <see cref="MaskXmlSerializedEnabled"/>(Not implemented yet) is true.
		/// </summary>
		public List<string> SerializedKeys { get; set; }

		/// <summary>
		/// default: true. When true, SerializedKeys
		/// </summary>
		public bool MaskJsonSerializedEnabled { get; set; } = true;

		/// <summary>
		/// NOT SUPPORTED YET!!!
		/// </summary>
		public bool MaskXmlSerializedEnabled { get; set; } = true;

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
		public List<KeyKeyValueKey> KeyKeyValueKeys { get; set; }

		/// <summary>
		/// Default: 3. 
		/// If value.Length < N, it mask engine will bypass it.
		/// For name it might be short. So set it to 3
		/// </summary>
		public int ValueMinLength { get; set; } = 3;
	}
}
