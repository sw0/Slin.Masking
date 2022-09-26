using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Slin.Masking.Tests
{
	public class MaskingProfileTests : TestBase
	{
		public MaskingProfileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{ }

		[Fact]
		public void MaskProfileTest()
		{
			var profile = CreateProvider().GetService<IOptions<MaskingProfile>>()!.Value;

			Assert.True(profile.Rules.Count > 0);
			Assert.True(profile.NamedFormatters.Count > 0);
			Assert.True(profile.Rules.All(x => !string.IsNullOrEmpty(x.Key)));
			Assert.True(profile.Rules.All(x => x.Value.Formatters != null));
		}

		[Fact]
		public void MaskProfileWithExceptionTest()
		{
			var profile = CreateProvider().GetService<IOptions<MaskingProfile>>()!.Value;

			profile.NamedFormatters.Clear();
			profile.Normalize();

			try { var masker = new Masker(profile); }
			catch (Exception ex)
			{
				Assert.StartsWith("NamedFormatters does not found:", ex.Message);
			}
		}

		[Fact]
		public void MaskProfileWithException2Test()
		{
			var profile = CreateProvider().GetService<IOptions<MaskingProfile>>()!.Value;

			var nameNotExists = "not-exists-sfsdfds";
			profile.Rules.Add("sssss", new MaskRuleDefinition
			{
				Formatters = new List<ValueFormatterDefinition>
			 {
				 new ValueFormatterDefinition{Name=nameNotExists}
			 }
			});
			profile.Normalize();

			try { var masker = new Masker(profile); }
			catch (Exception ex)
			{
				Assert.StartsWith("NamedFormatters does not found:", ex.Message);
				Assert.EndsWith(nameNotExists, ex.Message);
			}
		}
	}
}
