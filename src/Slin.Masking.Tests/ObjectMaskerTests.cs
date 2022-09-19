using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Slin.Masking;
using Xunit.Sdk;
using System.Xml.Linq;
using System.Xml.Serialization;
using System;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text;
using Xunit.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace Slin.Masking.Tests
{
	public class ObjectMaskerTests : TestBase
	{
		public ObjectMaskerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{ }


		private string SquareBrackes(string str)
		{
			return string.Concat("[", str, "]");
		}

		public static IEnumerable<object[]> DummyDataRows =>
			new List<object[]>
			{
			new object[] { 1, 2, 3 },
			new object[] { -4, -6, -10 },
			new object[] { -2, 2, 0 },
			new object[] { int.MinValue, -1, int.MaxValue },
			};

		private MaskingProfile GetMaskingProfile()
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
    ""NamedFormatterDefintions"": {
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


		private void ModifyProfile(MaskingProfile profile)
		{
			profile!.MaskXmlSerializedEnabled = true;
			profile!.MaskXmlSerializedOnXmlAttributeEnabled = true;

			profile.NamedFormatterDefintions.Add("pAN14", new ValueFormatterDefinition
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


		[Theory]
		//[MemberData(nameof(DummyDataRows))]
		[ClassData(typeof(DummyDataTestRows))]
		public void MaskDummyDataUserTest(string[] keys, string expected)
		{
			var profile = GetMaskingProfile();

			ModifyProfile(profile);

			var masker = new Masker(profile);
			var objectMasker = new ObjectMasker(masker, profile);

			var dummyData = DummyData.CreateLogEntry();

			var data = dummyData.Picks(keys);

			var actual = objectMasker.MaskObject(data);

			Assert.True(actual != null);

			Assert.Equal(expected, actual);

			WriteLine($"test on data with keys '{string.Join(',', keys)}' good");
		}

		[Fact]
		public void MaskDummyDataTest()
		{
			var provider = CreateProvider();

			var output1 = "";
			var output2 = "";
			using (provider.CreateScope())
			{
				var profile = provider.GetRequiredService<IMaskingProfile>() as MaskingProfile;

				profile!.MaskXmlSerializedEnabled = true;
				profile!.MaskXmlSerializedOnXmlAttributeEnabled = true;

				var masker = new Masker(profile);
				var objectMasker = new ObjectMasker(masker, profile);

				var data = DummyData.CreateLogEntry();

				var result = output1 = objectMasker.MaskObject(data);

				Assert.True(result != null);
			}

			using (provider.CreateScope())
			{
				var profile = provider.GetRequiredService<IMaskingProfile>() as MaskingProfile;

				profile!.MaskXmlSerializedEnabled = false;
				profile!.MaskXmlSerializedOnXmlAttributeEnabled = false;

				var masker = new Masker(profile);
				var objectMasker = new ObjectMasker(masker, profile);

				var data = DummyData.CreateLogEntry();

				var result = output2 = objectMasker.MaskObject(data);

				Assert.True(result != null);
			}

			Assert.NotEqual(output1, output2);
		}

		#region -- try using StringBuilder. TODO --

		[Fact]
		public void TryMaskWithStringBuilderTest()
		{
			var provider = CreateProvider();

			using (provider.CreateScope())
			{
				var profile = provider.GetRequiredService<IMaskingProfile>() as MaskingProfile;

				var masker = new Masker(profile);
				var objectMasker = new ObjectMasker(masker, profile);

				var data = DummyData.CreateLogEntry();

				var sb = new StringBuilder();

				var dicData = data.ToDictionary(x => x.Key, x => x.Value);
				var element = JsonSerializer.SerializeToElement(data);
				var element2 = JsonSerializer.SerializeToElement(dicData);

				//var doc = JsonSerializer.SerializeToDocument(data);
				MaskJsonElement(null, element, sb);

				WriteLine(sb.ToString());
			}
		}

		private void MaskJsonElement(string? propertyName, JsonElement element, StringBuilder builder)
		{
			//todo depth check...

			bool previousAppeared = false;
			switch (element.ValueKind)
			{
				case JsonValueKind.Undefined:
					builder.Append("null");
					break;
				case JsonValueKind.Object:
					builder.Append('{');
					var isKvp = IsKvpObject(element, out string keyKey, out var key, out string valKey, out var value);
					if (isKvp)
					{
						var keyName = key.GetString();
						builder.Append('"').Append(keyName).Append('"').Append(':');
						//builder.Append('{');
						MaskJsonElement(keyName, value, builder);
						//builder.Append('}');
					}
					foreach (var child in element.EnumerateObject())
					{
						//todo skip properties of key and value
						if (isKvp && (child.Name == keyKey || child.Name == valKey))
							continue;

						if (isKvp)
							builder.Append(',');

						MaskProperty(child, builder);
						builder.Append(',');

						if (!previousAppeared)
							previousAppeared = true;
					}

					if (previousAppeared)
					{
						builder.Remove(builder.Length - 1, 1);
					}
					builder.Append('}');
					break;
				case JsonValueKind.Array:
					builder.Append('[');

					foreach (var child in element.EnumerateArray())
					{
						MaskJsonElement(null, child, builder);

						builder.Append(',');

						if (!previousAppeared)
							previousAppeared = true;
					}

					if (previousAppeared)
					{
						builder.Remove(builder.Length - 1, 1);
					}
					builder.Append(']');
					break;
				case JsonValueKind.String:
					//todo 
					builder.Append(string.Concat("\"", element.GetString(), "\"")); //todo quote
					break;
				case JsonValueKind.Number:
					builder.Append(element.GetRawText());
					break;
				case JsonValueKind.True:
					builder.Append(element.GetRawText());
					break;
				case JsonValueKind.False:
					builder.Append(element.GetRawText());
					break;
				case JsonValueKind.Null:
					builder.Append("null");
					break;
				default:
					throw new NotImplementedException("unkonwn valuekind");
			}
		}

		private void MaskProperty(JsonProperty property, StringBuilder builder)
		{
			builder.Append('"').Append(property.Name).Append('"').Append(':');
			MaskJsonElement(property.Name, property.Value, builder);
		}

		private bool IsKvpList(JsonElement ele, out string keyKey, out JsonElement key, out string valKey, out JsonElement value)
		{
			keyKey = valKey = "";
			key = value = default(JsonElement);
			if (ele.ValueKind != JsonValueKind.Array) return false;

			int flag = 0;
			int count = 0;
			keyKey = "Key";
			valKey = "Value";
			foreach (var child in ele.EnumerateArray())
			{
				count++;
				if (child.TryGetProperty(keyKey, out key))
				{
					if (key.ValueKind == JsonValueKind.String)
					{
						flag |= 1;
					}
				}
				if (child.TryGetProperty(valKey, out value))
				{
					flag |= 2;
				}
			}

			return flag == 3 && count == 2;
		}

		public bool IsKvpObject(JsonElement ele, out string keyKey, out JsonElement key, out string valKey, out JsonElement value)
		{
			keyKey = valKey = "";
			key = value = default(JsonElement);
			if (ele.ValueKind != JsonValueKind.Object) return false;

			int flag = 0;
			int count = ele.EnumerateObject().Count();
			keyKey = "Key";
			valKey = "Value";
			if (ele.TryGetProperty(keyKey, out key))
			{
				if (key.ValueKind == JsonValueKind.String)
				{
					flag |= 1;
				}
			}
			if (ele.TryGetProperty(valKey, out value))
			{
				flag |= 2;
			}
			return flag == 3 && count == 2;
			//todo not checking count?
		}
		#endregion
	}
}