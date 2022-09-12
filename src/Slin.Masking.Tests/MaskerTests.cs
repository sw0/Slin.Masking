using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Slin.Masking;
using Xunit.Sdk;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text;

namespace Slin.Masking.Tests
{
	public class MaskerTests : TestBase
	{
		[Fact]
		public void MaskProfileTest()
		{
			var profile = CreateProvider().GetService<IOptions<MaskingProfile>>()!.Value;

			Assert.True(profile.Rules.Count > 0);
			Assert.True(profile.NamedFormatterDefintions.Count > 0);
			Assert.True(profile.Rules.All(x => !string.IsNullOrEmpty(x.Key)));
			Assert.True(profile.Rules.All(x => x.Value.Formatters != null));
		}


		[Theory]
		[InlineData("FirstName", "Shawn", "Sh***")]
		[InlineData("SSN", "123456789", "*********")]
		[InlineData("SsN", "123456789", "*********")]
		[InlineData("Pan", "1234567890123456", "1234********3456")]
		[InlineData("PAN", "1234567890123456", "1234********3456")]
		[InlineData("PaN", "1234567890123456", "1234********3456")]
		[InlineData("Primaryaccountnumber", "1234567890123456", "1234********3456")]
		[InlineData("Pan", "123456789012345", "123456*****2345")]
		[InlineData("PAN", "123456789012345", "123456*****2345")]
		[InlineData("PaN", "123456789012345", "123456*****2345")]
		[InlineData("Primaryaccountnumber", "123456789012345", "123456*****2345")]
		[InlineData("personalaccountnumber", "123456789012345", "123456*****2345")]
		[InlineData("DOB", "2022-08-29T23:56:32.1895861", "REDACTED")]
		[InlineData("temperatureF", "4", null)]
		public void MaskerTest(string key, string value, string expected)
		{
			//var data = new[] {
			//	new{Key="FirstName", Value ="Shawn",  Expected ="Sh***" },
			//	new{Key="SSN", Value ="123456789",  Expected ="*********" },
			//	new{Key="SsN", Value ="123456789",  Expected ="*********" },
			//	//16 PAN
			//	new{Key="Pan", Value ="1234567890123456", Expected ="1234********3456" },
			//	new{Key="PAN", Value ="1234567890123456", Expected ="1234********3456" },
			//	new{Key="PaN", Value ="1234567890123456", Expected ="1234********3456" },
			//	new{Key="Primaryaccountnumber", Value ="1234567890123456", Expected ="1234********3456" },
			//	//15 PAN
			//	new{Key="Pan", Value ="123456789012345", Expected ="123456*****2345" },
			//	new{Key="PAN", Value ="123456789012345", Expected ="123456*****2345" },
			//	new{Key="PaN", Value ="123456789012345", Expected ="123456*****2345" },
			//	new{Key="Primaryaccountnumber", Value ="123456789012345", Expected ="123456*****2345" },
			//	new{Key="personalaccountnumber", Value ="123456789012345", Expected ="123456*****2345" },
			//	new{Key="DOB", Value ="2022-08-29T23:56:32.1895861", Expected ="REDACTED" },
			//	new{Key="temperatureF", Value ="4", Expected = default(string) }
			//};

			var provider = CreateProvider();

			//var context = new MaskingContext(profile);
			using (provider.CreateScope())
			{
				var engine = provider.GetRequiredService<IMasker>();

				Assert.NotNull(engine);

				var success = engine.TryMask(key, value, out var result);

				Assert.True(success, $"Failed: {key} ");
				Assert.True(object.Equals(expected, result), $"Not matched: {key}");
			}
		}


		[Theory]
		[InlineData("?firstname=shawn&lastname=1234", "?firstname=sh***&lastname=12**")]
		[InlineData("firstname=shawn&lastname=1234", "firstname=sh***&lastname=12**")]
		[InlineData("https://jd.com/pan/1234567890123456/firstname/hello", "https://jd.com/pan/1234********3456/firstname/he***")]
		[InlineData("https://jd.com/firstname/shawn/lastname/lin", "https://jd.com/firstname/sh***/lastname/li*")]
		[InlineData("https://jd.com/firstname/shawn/lastname/lin?ssn=123456789", "https://jd.com/firstname/sh***/lastname/li*?ssn=*********")]
		public void UrlMaskerTest(string url, string expected)
		{
			var provider = CreateProvider();

			//var context = new MaskingContext(profile);
			using (provider.CreateScope())
			{
				var engine = provider.GetRequiredService<IUrlMasker>();

				Assert.NotNull(engine);

				var result = engine.MaskUrl(url);
				Assert.Equal(expected, result);
			}
		}


		[Fact]
		public void XmlMaskTest()
		{
			var xml = DummyData.Xml1;

			var provider = CreateProvider();

			//var context = new MaskingContext(profile);
			using (provider.CreateScope())
			{
				var engine = provider.GetRequiredService<IObjectMasker>();

				Assert.NotNull(engine);

				var data = DummyData.CreateLogEntry();

				var result = engine.MaskObject(data);
				Assert.True(result != null);
			}
		}
	}
}