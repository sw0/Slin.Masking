
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Slin.Masking.Tests
{
	public abstract class TestBase
	{
		protected IServiceCollection CreateServiceCollection(Action<IServiceCollection> setup = null)
		{
			var services = new ServiceCollection();
			var configBuilder = new ConfigurationBuilder();
			configBuilder
				.AddJsonFile("masking.json")
				.AddJsonFile("masking.custom.json", true);

			var configuration = configBuilder.Build();

			services.AddSingleton<IConfiguration>(configuration);
			services.Configure<MaskingProfile>(configuration.GetSection("Masking"))
				.PostConfigure<MaskingProfile>((options) => options.Normalize());

			services.AddScoped<IMaskingProfile>(provider =>
			{
				var profile = provider.GetRequiredService<IOptions<MaskingProfile>>()!.Value;
				return profile;
			});
			services.AddScoped<IObjectMaskingOptions>(provider =>
			{
				var profile = provider.GetRequiredService<IMaskingProfile>();
				return profile;
			});
			services.AddScoped<IMaskingOptions>(provider =>
			{
				var profile = provider.GetRequiredService<IMaskingProfile>();
				return profile;
			});

			services.AddScoped<IMasker, Masker>();
			services.AddScoped<IUrlMasker, Masker>();
			services.AddScoped<IObjectMasker, ObjectMasker>();

			if (setup != null) setup(services);

			return services;
		}

		protected IServiceProvider CreateProvider()
		{
			var provider = CreateServiceCollection().BuildServiceProvider();


			return provider;
		}
	}
}