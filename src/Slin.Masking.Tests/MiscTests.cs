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

		[Fact]
		public void CopyXElementTest()
		{
			var json = @"<data><ssn>123</ssn><dob>123456</dob><list><a>123</a><b>123</b></list></data>";

			var ele1 = XElement.Parse(json);

			var ele2 = new XElement(ele1);

			foreach (var e in ele2.Elements())
			{
				if (e.HasElements)
				{
					foreach (var item in e.Elements())
					{
						item.SetValue("good");
					}
				}
				else
				{
					e.SetValue("asd");
				}
			}

			var x = ele2.ToString() != ele1.ToString();

			Assert.True(x);
		}

		[Theory]
		[InlineData("xml")]
		[InlineData("masked")]
		public void LearnXElement(string type)
		{
			var (element, elementMasked) = DummyData.GetXElement();

			if (type == "masked") element = elementMasked;

			var xml = element.ToString(SaveOptions.None);
			var xml2 = element.ToString(SaveOptions.DisableFormatting);

			Assert.NotNull(xml);
			Assert.NotNull(xml2);
			Assert.Contains("<![CDATA[", xml);
			Assert.Contains("<![CDATA[", xml2);
		}

		[Fact]
		public void GetJsonElementsTest()
		{
			var (a, b) = DummyData.GetJsonString(true, ModeIfArray.HandleAll);



		}
	}
}
