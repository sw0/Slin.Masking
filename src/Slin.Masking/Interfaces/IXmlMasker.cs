using System.Xml.Linq;

namespace Slin.Masking
{
	public interface IXmlMasker
	{
		string MaskXmlElementString(XElement node);

		bool TryParse(string value, out XElement node, bool basicValidation = true);
	}
}
