using System;

namespace Slin.Masking
{
	public interface IMaskFormatter : IFormatProvider, ICustomFormatter
	{
		bool IsFormatMatched(string format);
	}
}
