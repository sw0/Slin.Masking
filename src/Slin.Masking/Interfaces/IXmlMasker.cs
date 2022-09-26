using System.Xml.Linq;

namespace Slin.Masking
{
	public interface IXmlMasker
	{
		/// <summary>
		/// element will be masked 
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		string MaskXmlElementString(XElement node);

		bool TryParse(string value, out XElement node, bool basicValidation = true);
	}
}
