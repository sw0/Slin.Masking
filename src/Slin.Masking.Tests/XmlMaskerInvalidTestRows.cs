using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slin.Masking.Tests
{
	internal class XmlMaskerInvalidTestRows : TheoryData<string, string, bool, string>
	{
		public XmlMaskerInvalidTestRows()
		{
			AddRow("c-null", null, false, null);
			AddRow("c-empty", "", false, null);
			AddRow("c-simple", "".WrapXml(), false, null);
		}
	}
}
