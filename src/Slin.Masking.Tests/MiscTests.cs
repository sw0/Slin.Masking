using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Slin.Masking.Tests
{
	public class MiscTests : TestBase
	{
		public MiscTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{ }

		[Theory]
		[InlineData("xml")]
		[InlineData("masked")]
		public void LearnXElement(string type)
		{
			var element = type == "xml" ? DummyData.GetXElement()
				: DummyData.GetXElementMasked();

			var xml = element.ToString(SaveOptions.None);
			var xml2 = element.ToString(SaveOptions.DisableFormatting);

			Assert.NotNull(xml);
			Assert.NotNull(xml2);
			Assert.Contains("<![CDATA[", xml);
			Assert.Contains("<![CDATA[", xml2);
		}

		//[Theory]
		//[InlineData(1)]
		//[InlineData(1000)]
		//[InlineData(5000)]
		//public void MaskObjectPerfTestWithSibling(int count)
		//{
		//}
	}
}
