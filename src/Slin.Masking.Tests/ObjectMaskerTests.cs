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
using static Slin.Masking.Tests.DummyData;
using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Slin.Masking.Tests
{
	public partial class ObjectMaskerTests : TestBase
	{
		public ObjectMaskerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{ }

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
			profile.ArrayItemHandleMode = ArrayItemHandleMode.SingleItemAsValue;

			profile.UrlKeys = new List<string> { "requestUrl", "query", "kvpFIEld", "kvpfield", "formdata" };
			profile.KeyKeyValueKeys.Add(new KeyKeyValueKey("key", "val"));

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


		//[Theory]
		////[MemberData(nameof(DummyDataRows))]
		//[ClassData(typeof(DummyDataTestRows))]
		//public void MaskDummyDataUserTest(string[] keys, string expected)
		//{
		//	var profile = GetMaskingProfile();

		//	ModifyProfile(profile);

		//	var masker = new Masker(profile);
		//	var objectMasker = new ObjectMasker(masker, profile);

		//	var dummyData = CreateLogEntry();

		//	var data = dummyData.Picks(keys);

		//	var actual = objectMasker.MaskObject(data);

		//	Assert.True(actual != null);

		//	File.AppendAllText("c:\\work\\a.log", String.Join(',', keys) + ":\r\n");
		//	File.AppendAllText("c:\\work\\a.log", actual);

		//	Assert.Equal(expected, actual);

		//	WriteLine($"test on data with keys '{string.Join(',', keys)}' good");
		//}

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

				var data = CreateLogEntry();

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

				var data = CreateLogEntry();

				var result = output2 = objectMasker.MaskObject(data);

				Assert.True(result != null);
			}

			Assert.NotEqual(output1, output2);
		}

		#region -- try using StringBuilder. TODO --

		private IMaskingProfile _profile;
		private Masker _masker;

		[Theory]
		[ClassData(typeof(JsonMaskerTestRows))]
		public void JsonMaskerTest(string keys, string expected, string expectedOnlyValue)
		{
			var profile = GetMaskingProfile();
			ModifyProfile(profile);

			var masker = new Masker(profile);
			var jsonMasker = new JsonMasker(masker, profile);
			var xmlMasker = new XmlMasker(masker, profile);
			jsonMasker.SetXmlMasker(xmlMasker);
			xmlMasker.SetJsonMasker(jsonMasker);

			var data = CreateLogEntry().Picks(keys.Split(','));

			StringBuilder sb = new StringBuilder();
			StringBuilder sb2 = new StringBuilder();
			jsonMasker.MaskObject(data, sb);

			if (data.Count == 1)
			{
				var value = data.First().Value;
				jsonMasker.MaskObject(value, sb2);
			}

			var actual = sb.ToString();
			var actual2 = sb2.ToString();

#if DEBUG
			var file = @$"c:\work\slin.masking.tests\{nameof(JsonMaskerTest)}_{DateTime.Now:HHmm}.txt";

			if (!Directory.Exists(Path.GetDirectoryName(file)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(file)!);
			}
			File.WriteAllText(file, actual);

			if (actual2.Length > 0)
			{
				File.AppendAllText(file, "\r\n\r\nexpectedOnlyValue:\r\n" + expectedOnlyValue);
				File.AppendAllText(file, "\r\n\r\nactual2:\r\n" + actual2);
			}
#endif
			Assert.Equal(expected, actual);
			Assert.Equal(expectedOnlyValue, actual2);
		}

		[Theory]
		[ClassData(typeof(XmlMaskerInvalidTestRows))]
		public void XmlMaskerInvalidTest(string xml, bool valid, string expected)
		{
			XmlMaskerTestInternal(nameof(XmlMaskerInvalidTest), xml, valid, expected);
		}

		[Theory]
		[ClassData(typeof(XmlMaskerTestRows))]
		public void XmlMaskerTest(string xml, bool valid, string expected)
		{
			XmlMaskerTestInternal(nameof(XmlMaskerTest), xml, valid, expected);
		}


		[Theory]
		[ClassData(typeof(XmlJsonMaskerTestRows))]
		public void XmlWithJsonMaskerTest(string xml, bool valid, string expected)
		{
			XmlMaskerTestInternal(nameof(XmlWithJsonMaskerTest), xml, valid, expected);
		}

		private void XmlMaskerTestInternal(string testName, string xml, bool valid, string expected)
		{
			var profile = GetMaskingProfile();
			ModifyProfile(profile);

			var masker = new Masker(profile);
			var xmlMasker = new XmlMasker(masker, profile);
			var jsonMasker = new JsonMasker(masker, profile);
			xmlMasker.SetJsonMasker(jsonMasker);
			jsonMasker.SetXmlMasker(xmlMasker);

			var objMasker = new ObjectMasker(masker, profile);

			var parsed = xmlMasker.TryParse(xml, out var ele);

			Assert.True(valid == parsed, "failed to parse the XML");

			if (parsed)
			{
				var actual = xmlMasker.MaskXmlElementString(ele);

				var parsed2 = xmlMasker.TryParse(xml, out var ele2);
				Assert.True(parsed2, "should be true, as it was parsed successfully");
				var actual2 = objMasker.MaskObject(ele2);

#if DEBUG
				var file = @$"c:\work\slin.masking.tests\{testName}_{DateTime.Now:HHmm}.txt";

				if (!Directory.Exists(Path.GetDirectoryName(file)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(file)!);
				}

				if (!File.Exists(file))
				{
					File.WriteAllText(file, $"{testName} test cases:" + Environment.NewLine);
				}

				File.AppendAllText(file, "input:" + Environment.NewLine);
				File.AppendAllText(file, xml + Environment.NewLine + Environment.NewLine);

				File.AppendAllText(file, "actual:" + Environment.NewLine);
				File.AppendAllText(file, actual + Environment.NewLine + Environment.NewLine);

				File.AppendAllText(file, "expected:" + Environment.NewLine + expected
					+ Environment.NewLine + Environment.NewLine
					+ $"[expected==actual]: {expected == actual}" + Environment.NewLine);
				File.AppendAllText(file, new string('-', 100)
					+ Environment.NewLine + Environment.NewLine);
#endif
				Assert.Equal(expected, actual);

				//Compare with two ways: MaskObject va MaskXmlElementString
				//NOTE here there is a known issue that JsonNode has a bug:
				var expected2 = expected.Replace("&amp;amp;", "&amp;").Replace("&amp;", "&");
				var actual3 = actual2.Replace("\\u0026","&").Replace("&amp;amp;", "&amp;").Replace("&amp;", "&")
					.Replace("\\u4E2D\\u56FD", "中国").Replace("\\u4E16\\u754C", "世界");
				Assert.Equal(expected2, actual3);
			}
		}
		#endregion
	}
}