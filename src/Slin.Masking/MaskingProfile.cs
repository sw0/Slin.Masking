using System;
using System.Collections.Generic;

namespace Slin.Masking
{
	public class MaskingProfile
	{
		///// <summary>
		///// Object Maskig options for global masking setting when using <see cref="IObjectMasker"/>
		///// </summary>
		//public ObjectMaskingOptions ObjectMaskingOptions { get; set; }

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
		/// Will be used to indicates comparison for Serialized Keys or Url Keys against property name
		/// </summary>
		public bool SerializedKeysCaseSensitive { get; set; } = true;

		/// <summary>
		/// default:false, if you need to enable number mask, set it to true. For example, account's balance.
		/// </summary>
		public bool MaskJsonNumberEnabled { get; set; }

		/// <summary>
		/// Default: 3. 
		/// If value.Length < N, it mask engine will bypass it.
		/// For name it might be short. So set it to 3
		/// </summary>
		public int ValueMinLength { get; set; } = 3;
		#endregion

		/// <summary>
		/// Masking Options for <see cref="IMasker"/> or <see cref="IUrlMasker"/>
		/// </summary>
		public MaskingOptions MaskingOptions { get; set; }

		/// <summary>
		/// optional NamedFormatterDefinitions
		/// </summary>
		public Dictionary<string, ValueFormatterDefinition> NamedFormatterDefintions { get; set; }
			= new Dictionary<string, ValueFormatterDefinition>(StringComparer.OrdinalIgnoreCase);

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

		//private Action OnProfileValidation { get; private set; }

		/// <summary>
		/// TODO validation inside post configuration
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void Normalize()
		{
			foreach (var item in NamedFormatterDefintions)
			{
				//if (string.IsNullOrEmpty(item.Value.Name)) 
				item.Value.Name = item.Key;
			}

			foreach (var item in Rules)
			{
				if (item.Value.Formatters == null || item.Value.Formatters.Count == 0)
				{
					//todo wanning
#if DEBUG
					throw new Exception("Formatters should be be null or none set");
#endif
				}

				foreach (var fmt in item.Value.Formatters)
				{
					//todo warnning if both Name and Format got set.
					//if(fmt.Name)
				}
			}
		}
	}
}
