using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Slin.Masking;
using Xunit.Sdk;

namespace Slin.Masking.Tests
{
	public class MaskerTests: TestBase
	{
		[Fact]
		public void MaskProfileTest()
		{
			var profile = CreateProvider().GetService<IOptions<MaskingProfile>>()!.Value;

			Assert.True(profile.Rules.All(x => !string.IsNullOrEmpty(x.Key)));
			Assert.True(profile.Rules.All(x => x.Value.Formatters != null));
		}


		[Fact]
		public void MaskerTest()
		{
			var data = new[] {
				new{Key="FirstName", Value ="Shawn",  Expected ="Sh***" },
				new{Key="SSN", Value ="123456789",  Expected ="*********" },
				new{Key="SsN", Value ="123456789",  Expected ="*********" },
				//16 PAN
				new{Key="Pan", Value ="1234567890123456", Expected ="1234********3456" },
				new{Key="PAN", Value ="1234567890123456", Expected ="1234********3456" },
				new{Key="PaN", Value ="1234567890123456", Expected ="1234********3456" },
				new{Key="Primaryaccountnumber", Value ="1234567890123456", Expected ="1234********3456" },
				//15 PAN
				new{Key="Pan", Value ="123456789012345", Expected ="123456*****2345" },
				new{Key="PAN", Value ="123456789012345", Expected ="123456*****2345" },
				new{Key="PaN", Value ="123456789012345", Expected ="123456*****2345" },
				new{Key="Primaryaccountnumber", Value ="123456789012345", Expected ="123456*****2345" },
				new{Key="personalaccountnumber", Value ="123456789012345", Expected ="123456*****2345" },
				new{Key="DOB", Value ="2022-08-29T23:56:32.1895861", Expected ="REDACTED" },
				new{Key="temperatureF", Value ="4", Expected = default(string) }
			};

			var provider = CreateProvider();

			//var context = new MaskingContext(profile);
			using (provider.CreateScope())
			{
				var engine = provider.GetRequiredService<IMasker>();

				Assert.NotNull(engine);

				//string result;
				int idx = 0;
				foreach (var item in data)
				{
					Assert.True(engine.TryMask(item.Key, item.Value, out var result), $"Failed: {item.Key} on sub-test: {idx}");
					Assert.True(object.Equals(item.Expected, result), $"Not matched: {item.Key} on sub-test: {idx}");
					idx++;
				}
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

				//var url = "";
				//url = "?firstname=shawn&lastname=1234&password=7234324";
				//url = "?firstname=shawn&lastname=&password=";
				var result = engine.MaskUrl(url);
				Assert.Equal(expected, result);
			}
		}
	}
}