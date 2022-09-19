using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Slin.Masking
{
	public class MaskingProfile : IMaskingProfile
	{

		#region -- object masking settings --
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
		/// default false
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

		/// <inheritdoc/>
		public bool MaskNestedKvpEnabled { get; set; } = false;
		public List<KeyKeyValueKey> KeyKeyValueKeys { get; set; }
		/// <summary>
		/// Default: 3. 
		/// If value.Length < N, it mask engine will bypass it.
		/// For name it might be short. So set it to 3
		/// </summary>
		public int ValueMinLength { get; set; } = 3;
		/// <summary>
		/// default: 30. If string length is less than N, it will bypass parsing Xml document.
		/// </summary>
		public int XmlMinLength { get; set; } = 30;
		/// <summary>
		/// default: 15. If string length is less than N, it will bypass parsing Json document.
		/// </summary>
		public int JsonMinLength { get; set; } = 15;
		#endregion

		#region -- masker settings --

		///// <summary>
		///// Masking Options for <see cref="IMasker"/> or <see cref="IUrlMasker"/>
		///// </summary>
		//public MaskingOptions MaskingOptions { get; set; }

		public string RegexCheckChars { get; set; } = MaskingProfile.DefaultRegexCheckChars;

		internal const string DefaultRegexCheckChars = "+*?[]|().\\^$";

		/// <summary>
		/// Important: suggest ignore case. so default: true.
		/// </summary>
		public bool KeyedMaskerPoolIgnoreCase { get; set; } = true;

		public int KeyNameLenLimitToCache { get; set; }

		#endregion

		#region -- masking settings: rules related properties --

		private Dictionary<string, ValueFormatterDefinition> _namedFormatterDefintions;
		/// <summary>
		/// optional NamedFormatterDefinitions
		/// </summary>
		public Dictionary<string, ValueFormatterDefinition> NamedFormatterDefintions
		{
			get { return _namedFormatterDefintions; }
			set
			{
				if (value != null)
					_namedFormatterDefintions = new Dictionary<string, ValueFormatterDefinition>(value, StringComparer.OrdinalIgnoreCase);
				else
					_namedFormatterDefintions = value;
			}
		}
		//= new Dictionary<string, ValueFormatterDefinition>(StringComparer.OrdinalIgnoreCase);

		//todo use MaskRuleDefinitionCollection?
		public Dictionary<string, MaskRuleDefinition> Rules { get; set; } = new Dictionary<string, MaskRuleDefinition>();

		/// <summary>
		/// Url Masking patterns. Each pattern will be tried and group name will be used the key name while masking.
		/// Example:
		/// <![CDATA[
		///    https://abc.com/cards/pan/(?<pan>\d{16})/ssn/(?<ssn>\d{16}) 
		///    it's just for example, usually we should not pass sensitive information in URL
		/// ]]>
		/// </summary>
		public List<UrlMaskingPattern> UrlMaskingPatterns { get; set; } = new List<UrlMaskingPattern>();

		#endregion

		/// <summary>
		/// validation inside post configuration. The important thing here is set NamedFormatterDefinitions formatters's name by item.key.
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void Normalize()
		{
			foreach (var item in NamedFormatterDefintions)
			{
				//if (string.IsNullOrEmpty(item.Value.Name)) 
				item.Value.Name = item.Key.ToLower();
			}

			if (string.IsNullOrWhiteSpace(RegexCheckChars))
			{
				RegexCheckChars = DefaultRegexCheckChars;
			}

#if DEBUG
			foreach (var item in Rules)
			{
				if (item.Value.Formatters == null || item.Value.Formatters.Count == 0)
				{
					//todo wanning
					throw new Exception($"Formatters should be be null or none set. Rule key: {item.Key}");
				}

				foreach (var fmt in item.Value.Formatters)
				{
					//todo warnning if both Name and Format got set.
					//if(fmt.Name)
				}
			}
#endif
		}
	}
}
