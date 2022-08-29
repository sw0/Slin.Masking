using System;
using System.Collections.Generic;

namespace Slin.Masking.NLog
{
	public class LogMaskingOptions
	{
		public List<string> SerializedKeys { get; set; }

		public bool EnabledUnfoldSerialized { get; set; } = true;

		public bool SerializedKeysCaseSensitive { get; set; } = true;

		/// <summary>
		/// Default: 3. 
		/// If value.Length < N, it mask engine will bypass it.
		/// For name it might be short. So set it to 3
		/// </summary>
		public int ValueMinLength { get; set; } = 3;
		/// <summary>
		/// default:false, if you need to enable number mask, set it to true.
		/// </summary>
		public bool EnableJsonNumberMasking { get; set; }
	}
}
