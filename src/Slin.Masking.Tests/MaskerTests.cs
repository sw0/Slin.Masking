using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Slin.Masking.Tests
{
	public class MaskerTests : TestBase
	{
		public MaskerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{ }
		[Theory]
		[InlineData("FirstName", "Shawn", "Sh***")]
		[InlineData("SSN", "123456789", "*********")]
		[InlineData("SsN", "123456789", "*********")]
		[InlineData("Pan", "1234567890123456", "1234********3456")]
		[InlineData("PAN", "1234567890123456", "1234********3456")]
		[InlineData("PaN", "1234567890123456", "1234********3456")]
		[InlineData("Primaryaccountnumber", "1234567890123456", "1234********3456")]
		[InlineData("Pan", "12345678912345", "123456****2345")]
		[InlineData("PAN", "12345678912345", "123456****2345")]
		[InlineData("PaN", "12345678912345", "123456****2345")]
		[InlineData("Primaryaccountnumber", "12345678912345", "123456****2345")]
		[InlineData("personalaccountnumber", "12345678912345", "123456****2345")]
		[InlineData("DOB", "2022-08-29T23:56:32.1895861", "REDACTED")]
		public void MaskerTest(string key, string value, string expected)
		{
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
	}
}