
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
            services.Configure<MaskingProfile>(configuration.GetSection("Masking"));

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
    //JSON Related
    ""MaskJsonSerializedEnabled"": true,
    ""MaskJsonNumberEnabled"": true,//default false
    ""JsonAllowedCharacters"": ""<>&+"", //by default: <>&+ will be allowed. This would make these characters not been escaped.
	""JsonAllowedUnicodeRanges"": [],
    ""MaskJsonSerializedParsedAsNode"": true, //default: true
    ""JsonMinLength"": 10, 
	""GlobalModeForArray"": 1, //0:default,1:single,2:all
    //XML related
    ""MaskXmlSerializedEnabled"": true,
    ""MaskXmlSerializedOnXmlAttributeEnabled"": false,
    ""MaskJsonSerializedOnXmlAttributeEnabled"": false,
    ""XmlMinLength"": 15,
	//URL related
    ""MaskUrlEnabled"": true, 
    ""UrlKeys"": [ ""requestUrl"", ""query"", ""kvpFIEld"", ""kvpfield"" ],
    ""UrlMaskingPatterns"": [
      {
        ""Pattern"": ""firstname/(?<firstName>[^/]+)|lastName/(?<lastname>[^/\\?]+)"",
        ""IgnoreCase"": true
      },
      {
        ""Pattern"": ""pan/(?<pan>\\d{15,16})""
      }
    ],
    //Common setting for both Json and XML masking
    ""SerializedKeys"": [ ""Body"", ""ResponseBody"", ""reserialize"", ""badJsonBody"" ],
    ""SerializedKeysCaseSensitive"": false,
    ""KeyedMaskerPoolIgnoreCase"": true, //default true
    ""EnableUnmatchedKeysCache"": true, //default true
	""KeyNameLenLimitToCache"": 36,//default 36
    ""ValueMinLength"": 3, 
    ""MaskNestedKvpEnabled"": true, 
    ""KeyKeyValueKeys"": [{""KeyKeyName"": ""Key"",""ValueKeyName"": ""Value""},
      {""KeyKeyName"": ""key"",""ValueKeyName"": ""value""}],

	//MASKING SETTINGS
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
	  //NOTE: rule key by default is case-insensitive, so does name of formatter. 
	  //It's controlled by property:IgnoreKeyCase, default is true
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
		//IgnoreKeyCase is working with KeyName
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

            var profile = JsonSerializer.Deserialize<MaskingProfile>(json, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            });
            profile!.Normalize();

            return profile;
        }


        protected void ModifyProfile(MaskingProfile profile)
        {
            profile!.MaskXmlSerializedEnabled = true;
            profile!.MaskXmlSerializedOnXmlAttributeEnabled = true;
            profile!.MaskJsonSerializedOnXmlAttributeEnabled = true;
            profile.GlobalModeForArray = ModeIfArray.HandleSingle;

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
            profile.Rules["ssnList"] = new MaskRuleDefinition
            {
                Formatters = new List<ValueFormatterDefinition> {
                    new ValueFormatterDefinition{ Format = "*" }
                }
            };
        }
    }
}