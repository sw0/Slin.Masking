
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Slin.Masking;
using Xunit.Sdk;

namespace Slin.Masking2.Tests
{
	public class MaskerTests
	{

		IServiceProvider CreateProvider() {

			var services = new ServiceCollection();
			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddJsonFile("masking.json");

			var configuration = configBuilder.Build();

			services.AddSingleton<IConfiguration>(configuration);
			services.Configure<MaskingProfile>(configuration.GetSection("Masking"))
				.PostConfigure<MaskingProfile>((options) => options.Normalize());

			services.AddScoped<MaskingProfile>(provider =>
			{
				var profile = provider.GetRequiredService<IOptions<MaskingProfile>>()!.Value;

				return profile;
			});

			//services.AddScoped<IMaskContext, MaskingContext>();
			services.AddScoped<IMasker, Masker>();
			//services.AddScoped<IUrlMasker, Masker>();

			var provider = services.BuildServiceProvider();

			var profile = provider.GetService<IOptions<MaskingProfile>>()!.Value;

			Assert.True(profile.Rules.All(x => !string.IsNullOrEmpty(x.Key)));
			Assert.True(profile.Rules.All(x => x.Value.Formatters != null));

			return provider;
		}

		[Fact]
		public void MaskProfileTest1()
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


		//[Fact]
		//public void UrlMaskerTest1()
		//{
		//	var provider =	CreateProvider();

		//	//var context = new MaskingContext(profile);
		//	using (provider.CreateScope())
		//	{
		//		var engine = provider.GetRequiredService<IUrlMasker>();

		//		Assert.NotNull(engine);

		//		var url = "";
		//		url = "?firstname=shawn&lastname=1234&password=7234324";
		//		url = "?firstname=shawn&lastname=&password=";
		//		engine.TryMask(url);

		//		////string result;
		//		//int idx = 0;
		//		//foreach (var item in data)
		//		//{
		//		//	Assert.True(engine.TryMask(item.Key, item.Value, out var result), $"Failed: {item.Key} on sub-test: {idx}");
		//		//	Assert.True(object.Equals(item.Expected, result), $"Not matched: {item.Key} on sub-test: {idx}");
		//		//	idx++;
		//		//}
		//	}
		//}
	}
}