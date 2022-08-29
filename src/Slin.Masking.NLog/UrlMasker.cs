using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slin.Masking.NLog
{
	public interface IUrlMasker
	{
		string MaskUrl(string url);
	}

	public class UrlMasker : IUrlMasker
	{
		public string MaskUrl(string url)
		{
			throw new NotImplementedException();
		}
	}
}
