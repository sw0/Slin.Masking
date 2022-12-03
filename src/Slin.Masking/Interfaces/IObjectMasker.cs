namespace Slin.Masking
{
	public interface IObjectMasker : IJsonMasker, IXmlMasker, IUrlMasker
	{
		string MaskObject(object value);

		//void MaskObject(object value, StringBuilder builder);

		///// <summary>
		///// Indicates enabled or not. Just used as global setting. Not affecting ObjectMasker actually.
		///// </summary>
		//bool Enabled { get; }
	}
}
