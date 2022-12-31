namespace Slin.Masking
{
	public interface IObjectMasker : IJsonMasker, IXmlMasker, IUrlMasker
	{
		string MaskObject(object value);
	}
}
