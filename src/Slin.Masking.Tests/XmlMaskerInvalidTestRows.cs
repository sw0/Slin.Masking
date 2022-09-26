using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slin.Masking.Tests
{
	internal class XmlMaskerInvalidTestRows : TheoryData<string, bool, string>
	{
		public XmlMaskerInvalidTestRows()
		{
			AddRow(null, false, null);
			AddRow("", false, null);
			AddRow("".WrapXml(), false, null);
		}
	}
}
