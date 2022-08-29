using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Slin.Masking
{
	/// <summary>
	/// 
	/// </summary>
	public interface IMasker
	{
		bool TryMask(string key, string value, out string masked);
	}

	/// <summary>
	/// default Masker
	/// </summary>
	public class Masker : IMasker//, IUrlMasker
	{
		private readonly IMaskingContext _context;

		public Masker(MaskingProfile profile, IMaskFormatter maskFormatter = null)
		{
			_context = new MaskingContext(profile, maskFormatter);
		}

		public bool TryMask(string key, string value, out string masked)
		{
			var masker = _context.GetKeyedMasker(key, value);

			if (masker == null)
			{
				masked = value;
				return false;
			}

			masked = masker.Mask(value);
			return true;
		}

		//public string TryMask(string url)
		//{
		//	if (string.IsNullOrEmpty(url)) return url;

		//	//var uri = new Uri(url, UriKind.RelativeOrAbsolute);

		//	var builder = new StringBuilder(url.Length);

		//	var path = "";
		//	var idx = url.IndexOf('?');
		//	if (idx >= 0)
		//	{
		//		if (idx > 0)
		//		{
		//			path = url.Substring(0, idx);

		//			MaskPath(path, builder);
		//		}
		//		var query = url.Substring(idx);

		//		MaskQuery(query, builder);

		//		return builder.ToString();
		//	}
		//	else 

		//	return url;
		//}

		//private string GetPath(string url) {
		//	if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
		//			url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
		//			|| url.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
		//	{
		//		string path = url.Substring(url.IndexOf('/', url.IndexOf("://") + 3));

		//		return path;
		//	}
		//}
		//private string MaskPath(string path, StringBuilder builder)
		//{

		//}
		//private string MaskQuery(string query, StringBuilder builder)
		//{

		//	var items = HttpUtility.ParseQueryString(query);

		//	if (items.HasKeys())
		//	{
		//		bool changed = false;

		//		for (var i = 0; i < items.Count; i++)
		//		{
		//			var key = items.Keys[i];
		//			var value = items[i];
		//			if (value != "")
		//			{
		//				if (TryMask(key, value, out var masked))
		//				{
		//					if (masked != value && changed == false)
		//						changed = true;
		//				}
		//			}
		//		}
		//	}
		//}
	}

	//public interface IUrlMasker
	//{
	//	string TryMask(string url);
	//}
}
