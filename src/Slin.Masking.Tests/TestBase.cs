
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using System.Text.Json;

namespace Slin.Masking.Tests
{
	public abstract class TestBase
	{
		ITestOutputHelper _output;
		public TestBase(ITestOutputHelper testOutputHelper)
		{ _output = testOutputHelper; }

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

			services.AddSingleton<IMaskingProfile>(provider =>
			{
				var profile = provider.GetRequiredService<IOptions<MaskingProfile>>()!.Value;
				return profile;
			});
			services.AddSingleton<IObjectMaskingOptions>(provider =>
			{
				var profile = provider.GetRequiredService<IMaskingProfile>();
				return profile;
			});
			services.AddSingleton<IMaskingOptions>(provider =>
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

		protected void WriteLine(string msg) => _output.WriteLine(msg);


		protected MaskingProfile GetMaskingProfile()
		{
			//the json here are the default configuration.
			var json = @"{
    ""Enabled"": true,
    ""MaskUrlEnabled"": true, 
    ""MaskJsonSerializedEnabled"": true,
    ""MaskXmlSerializedEnabled"": true,
    ""MaskXmlSerializedOnXmlAttributeEnabled"": false,
    ""MaskJsonSerializedOnXmlAttributeEnabled"": false,
    ""MaskJsonNumberEnabled"": true,
    ""MaskNestedKvpEnabled"": true, 
    ""KeyKeyValueKeys"": [{""KeyKeyName"": ""Key"",""ValueKeyName"": ""Value""},
      {""KeyKeyName"": ""key"",""ValueKeyName"": ""value""}],
    ""ValueMinLength"": 3, 
    ""XmlMinLength"": 15,
    ""JsonMinLength"": 10, 
    ""SerializedKeysCaseSensitive"": false,
    ""UrlKeys"": [ ""requestUrl"", ""query"", ""kvpFIEld"", ""kvpfield"" ],
    ""SerializedKeys"": [ ""Body"", ""ResponseBody"", ""reserialize"" ],
    ""UrlMaskingPatterns"": [
      {
        ""Pattern"": ""firstname/(?<firstName>[^/]+)|lastName/(?<lastname>[^/\\?]+)"",
        ""IgnoreCase"": true
      },
      {
        ""Pattern"": ""pan/(?<pan>\\d{15,16})""
      }
    ],
    ""NamedFormatters"": {
      ""null"": { ""Format"": ""null"" },
      ""empty"": { ""Format"": ""EMPTY"" },
      ""redacted"": { ""Format"": ""REDACTED"" },
      ""credential_long"": {
        ""Format"": ""L9*4R6"",
        ""ValuePattern"": "".{24,}""
      },
      ""credential_short"": {
        ""Format"": ""L3*4R3"",
        ""ValuePattern"": "".{10,24}""
      },
      ""password"": { ""Format"": ""*6"" },
      ""dob"": { ""Format"": ""REDACTED"" },
      ""name"": { ""Format"": ""L2*"" },
      ""ssn"": { ""Format"": ""*"" },
      ""phone"": { ""Format"": ""L3R4"" },
      ""email"": {
        ""Format"": ""L3*@"",
        ""Description"": ""email is special, shawn@a.cn will be masked as 'sha**@a.cn'""
      },
      ""pan"": {
        ""ValuePattern"": ""^\\d{15,16}$"",
        ""Format"": ""L4*R4"",
        ""IgnoreCase"": false,
        ""Enabled"": true
      },
      ""cvv"": { ""Format"": ""*"" },
      ""Remove"": { ""RemoveNode"": true }
    },
    ""Rules"": {
      ""authorization"": {
        ""KeyName"": ""^authorization|access_token|accesstoken|code$"",
        ""IgnoreKeyCase"": true,
        ""Formatters"": [
          { ""Name"": ""credential_long"" },
          { ""Name"": ""credential_short"" }
        ]
      },
      ""SSN"": { ""Formatters"": [ { ""Name"": ""SSN"" } ] },
      ""DOB"": { ""Formatters"": [ { ""Name"": ""dob"" } ] },
      ""Pan"": {
        ""KeyName"": ""^pan|PersonalAccountNumber|PrimaryAccountNumber$"",
        ""IgnoreKeyCase"": true,
        ""Formatters"": [ { ""Name"": ""pan"" } ]
      },
      ""cvv"": { ""Formatters"": [ { ""Name"": ""cvv"" } ] },
      ""Balance"": { ""Formatters"": [ { ""Format"": ""null"" } ] },
      ""FirstName"": { ""Formatters"": [ { ""Name"": ""Name"" } ] },
      ""LastName"": { ""Formatters"": [ { ""Name"": ""Name"" } ] },
      ""Email"": { ""Formatters"": [ { ""Name"": ""email"" } ] },
      ""Password"": { ""Formatters"": [ { ""Format"": ""*"" } ] },
      ""PhoneNumber"": { ""Formatters"": [ { ""Name"": ""phone"" } ] }
    }
}";

			var profile = JsonSerializer.Deserialize<MaskingProfile>(json);
			profile!.Normalize();

			return profile;
		}


		protected void ModifyProfile(MaskingProfile profile)
		{
			profile!.MaskXmlSerializedEnabled = true;
			profile!.MaskXmlSerializedOnXmlAttributeEnabled = true;
			profile!.MaskJsonSerializedOnXmlAttributeEnabled = true;
			profile.ArrayItemHandleMode = ArrayItemHandleMode.SingleItemAsValue;

			profile.UrlKeys = new List<string> { "requestUrl", "query", "kvpFIEld", "kvpfield", "formdata" };
			profile.KeyKeyValueKeys.Add(new KeyKeyValueKey("key", "val"));

			profile.NamedFormatters.Add("pAN14", new ValueFormatterDefinition
			{
				ValuePattern = "^\\d{14}$",
				Format = "L6*R4",
				IgnoreCase = false
			});

			profile.Rules.Add("amount", new MaskRuleDefinition
			{
				KeyName = "^(transaction)?amount$(?#ignorecase)",
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Name="null" }
				}
			});
			profile.Rules["pan"] = new MaskRuleDefinition
			{
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Name = "pan" },
					new ValueFormatterDefinition{ Name = "pan14" }
				}
			};
			profile.Rules["dataInBytes"] = new MaskRuleDefinition
			{
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Name = "REDACTED" }
				}
			};
		}
	}
}