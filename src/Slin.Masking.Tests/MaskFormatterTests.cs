
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Slin.Masking;
using Xunit;

namespace Slin.Masking.Tests
{
	public class MaskFormatterTests
	{
		[Theory]
		[InlineData("{0:*}")]
		[InlineData("{0:*2}")]
		[InlineData("{0:L2}")]
		[InlineData("{0:R2}")]
		[InlineData("{0:L2R2}")]
		public void NullStringTest(string format)
		{
			var input = default(string);

			Assert.Null(input);

			try
			{
				var value = string.Format(new MaskFormatter(), format, input);

				throw new Exception("Should not reach here");
			}
			catch (Exception ex)
			{
				Assert.Equal("null object is not allowed for masking", ex.Message);
			}
		}

		[Theory]
		[InlineData("{0:*}", "")]
		[InlineData("{0:*2}", "")]
		[InlineData("{0:L2}", "")]
		[InlineData("{0:R2}", "")]
		[InlineData("{0:L2R2}", "")]
		public void EmptyStringTest(string format, string expected)
		{
			var input = "";

			var value = string.Format(new MaskFormatter(), format, input);
			Assert.Equal(expected, value);
		}


		[Theory]
		[InlineData(default(string), "")]
		[InlineData("", "")]
		[InlineData("1234134123412", "1234134123412")]
		[InlineData("helloworld", "helloworld")]
		public void NullTest(string input, string expected)
		{
			//NOTE:
			//  for null format, it's sepecial that it's actually not be handled by MaskFormatter!!!
			//  all null string will be turned to original value like the test cases here.
			var value = string.Format(new MaskFormatter(), "{0:null}", input);
			Assert.Equal(expected, value);
		}

		[Theory]
		[InlineData("", "")]
		[InlineData("1234134123412", "")]
		[InlineData("helloworld", "")]
		public void EmptyTest(string input, string expected)
		{
			var value = string.Format(new MaskFormatter(), "{0:EMPTY}", input);
			Assert.Equal(expected, value);
		}

		[Theory]
		[InlineData("", "")]
		[InlineData("REDACTED", "REDACTED")]
		[InlineData("helloworld", "REDACTED")]
		[InlineData("123456789", "REDACTED")]
		public void RedactedTest(string input, string expected)
		{
			var value = string.Format(new MaskFormatter(), "{0:REDACTED}", input);
			Assert.Equal(expected, value);
		}


		[Theory]
		[InlineData("{0:*}", "*********")]
		[InlineData("{0:*2}", "**")]
		[InlineData("{0:L2}", "12*******")]
		[InlineData("{0:R2}", "*******89")]
		[InlineData("{0:L2R2}", "12*****89")]
		[InlineData("{0:L3R3}", "123***789")]
		[InlineData("{0:L9R9}", "123456789")]
		[InlineData("{0:L3R6}", "123456789")]
		[InlineData("{0:L5R5}", "123456789")]
		public void SSNTest(string format, string expected)
		{
			var ssn = "123456789";

			var value = string.Format(new MaskFormatter(), format, ssn);
			Assert.Equal(expected, value);
		}

		[Theory]
		[InlineData("{0:*}", "*********")]
		[InlineData("{0:L2*}", "12*******")]
		[InlineData("{0:L2*3}", "12***")]
		[InlineData("{0:R2*}", "*******89")]
		[InlineData("{0:L2*3R2}", "12***89")]
		[InlineData("{0:L3*3R3}", "123***789")]
		[InlineData("{0:L9*3R9}", "123456789")]
		[InlineData("{0:L3*3R6}", "123456789")]
		[InlineData("{0:L5*3R5}", "123456789")]
		public void SSNTest2(string format, string expected)
		{
			var ssn = "123456789";

			var value = string.Format(new MaskFormatter(), format, ssn);
			Assert.Equal(expected, value);
		}

		[Theory]
		[InlineData("{0:*}", "****************")]
		[InlineData("{0:L4*3R4}", "1234***3456")]
		[InlineData("{0:L3*R3}", "123**********456")]
		public void PanTest(string format, string expected)
		{
			var pan = "1234567890123456";

			var value = string.Format(new MaskFormatter(), format, pan);
			Assert.Equal(expected, value);
		}

		[Theory]
		[InlineData("Shawn", "{0:*}", "*****")]
		[InlineData("Shawn", "{0:L2*}", "Sh***")]
		[InlineData("Shawn", "{0:L3*R3}", "Shawn")]
		public void NameTest(string input, string format, string expected)
		{
			var value = string.Format(new MaskFormatter(), format, input);
			Assert.Equal(expected, value);
		}
	}
}