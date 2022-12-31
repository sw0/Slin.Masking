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
		/// <param name="maskParameters">default:true, to extract key value from query and process masking</param>
		/// <param name="overwrittenPatterns">if provided, it will overwrite those provided in profile</param>
		/// <returns></returns>
		string MaskUrl(string url, bool maskParameters = true, params UrlMaskingPattern[] overwrittenPatterns);
	}

}
