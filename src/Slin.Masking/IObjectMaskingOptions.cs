using System.Collections.Generic;

namespace Slin.Masking
{
	public interface IObjectMaskingOptions
	{
		bool Enabled { get; set; }
		bool MaskJsonNumberEnabled { get; set; }
		bool MaskJsonSerializedEnabled { get; set; }
		bool MaskUrlEnabled { get; set; }
		bool MaskXmlSerializedEnabled { get; set; }
		List<string> SerializedKeys { get; set; }
		bool SerializedKeysCaseSensitive { get; set; }
		List<string> UrlKeys { get; set; }
		int ValueMinLength { get; set; }
	}
}