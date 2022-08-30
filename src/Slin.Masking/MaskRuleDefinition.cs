using System.Collections.Generic;

namespace Slin.Masking
{
	public class MaskRuleDefinition
	{
		public string Description { get; set; }

		//public List<string> FieldNames { get; set; } = new List<string>(1);
		public string KeyName { get; set; } = "";

		public List<ValueFormatterDefinition> Formatters { get; set; }
	}
}
