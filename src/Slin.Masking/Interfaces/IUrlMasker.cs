namespace Slin.Masking
{
	/// <summary>
	/// IUrlMasker interface
	/// </summary>
	public interface IUrlMasker
	{
		/// <summary>
		/// Mask Url base on patterns using named group name, for query, by default it will use query parameter name.
		/// Profile.UrlMaskingPatterns is used by default as mask rules.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="maskParamters">default:true, to extract key value from query and process masking</param>
		/// <param name="overwrittenPatterns">if provided, it will overwrite those provied in profile</param>
		/// <returns></returns>
		string MaskUrl(string url, bool maskParamters = true, params UrlMaskingPattern[] overwrittenPatterns);
	}

}
