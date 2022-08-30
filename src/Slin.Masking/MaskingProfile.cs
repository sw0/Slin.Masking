using System;
using System.Collections.Generic;

namespace Slin.Masking
{
	public class MaskingProfile
	{
		/// <summary>
		/// Masking Options
		/// </summary>
		public MaskingOptions Options { get; set; }

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
