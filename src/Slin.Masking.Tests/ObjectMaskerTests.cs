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

namespace Slin.Masking.Tests
{
	public class ObjectMaskerTests : TestBase
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

		public class TestRows : TheoryData<string, string, string>
		{
			public TestRows()
			{
				//simple types
				Add(Keys.boolOfTrue, Masked.boolOfTrue, "true");
				Add(Keys.ssn, Masked.ssn, Quotes(SSN));
				Add(Keys.dob, Masked.dob, Quotes(DobStr));
				Add("ts", "{\"ts\":\"5.99ms\"}", Quotes("5.99ms"));
				Add(Keys.PrimaryAccountnumBER, Masked.PrimaryAccountnumBER, Quotes(PAN));
				Add(Keys.transactionAmount, Masked.transactionAmount, Amount.ToString());

				//object
				Add(Keys.data, Masked.data, Unpack(Masked.data));
				Add(Keys.user, Masked.user, Unpack(Masked.user));
				Add(Keys.kvp, Masked.kvp, Unpack(Masked.kvp));
				Add(Keys.kvpObj, Masked.kvpObj, Unpack(Masked.kvpObj));
				Add(Keys.kvpCls, Masked.kvpCls, Unpack(Masked.kvpCls));
				Add(Keys.Key, Masked.Key, Unpack(Masked.Key));
				//headers array of key-values
				Add(Keys.flatHeaders, Masked.flatHeaders, Unpack(Masked.flatHeaders));
				Add(Keys.headers, Masked.headers, Unpack(Masked.headers));

				//url, query/form-data? support decode?
				Add(Keys.query, Masked.query, Quotes(UrlQueryWithQuestionMark));
				Add(Keys.formdata, Masked.formdata, Quotes(UrlQuery));
				Add(Keys.requestUrl, Masked.requestUrl, Quotes(UrlFull));

				//arrays
				Add(Keys.dataInBytes, Masked.dataInBytes, Quotes(Convert.ToBase64String(DataInBytes)));
				Add(Keys.arrayOfInt, Masked.arrayOfInt, Unpack(Masked.arrayOfInt));
				Add(Keys.arrayOfStr, Masked.arrayOfStr, Unpack(Masked.arrayOfStr));
				Add(Keys.arrayOfObj, Masked.arrayOfObj, Unpack(Masked.arrayOfObj));
				Add(Keys.arrayOfKvpCls, Masked.arrayOfKvpCls, Unpack(Masked.arrayOfKvpCls));

				//arrays complex
				Add(Keys.arrayOfKvp, Masked.arrayOfKvp, Unpack(Masked.arrayOfKvp));
				Add(Keys.arrayOfKvpNested, Masked.arrayOfKvpNested, Unpack(Masked.arrayOfKvpNested));
				Add(Keys.dictionary, Masked.dictionary, Unpack(Masked.dictionary));
				Add(Keys.dictionaryNested, Masked.dictionaryNested, Unpack(Masked.dictionaryNested));

				//reserialize JSON
				Add(Keys.reserialize, Masked.reserialize, Quotes(Masked.reserializeNoMask));
			}

			private string Unpack(string json)
			{
				var result = json.Substring(0, json.Length - 1).Substring(json.IndexOf(':') + 1);
				return result;
			}
		}

		private IMaskingProfile _profile;
		private Masker _masker;

		[Theory]
		//[MemberData(nameof(DummyDataRows))]
		[ClassData(typeof(TestRows))]
		public void MaskBuilder1Test(string keys, string expected, string expectedOnlyValue)
		{
			var profile = GetMaskingProfile();
			ModifyProfile(profile);

			var masker = new Masker(profile);
			var jsonMasker = new JsonMasker(masker, profile);

			var data = CreateLogEntry().Picks(keys.Split(','));

			StringBuilder sb = new StringBuilder();
			StringBuilder sb2 = new StringBuilder();
			jsonMasker.MaskObject(data, sb);


			if (data.Count == 1)
			{
				jsonMasker.MaskObject(data.First().Value, sb2);
			}

			var actual = sb.ToString();
			var actual2 = sb2.ToString();

			Assert.Equal(expected, actual);
			Assert.Equal(expectedOnlyValue, actual2);
		}

		[Theory]
		//[MemberData(nameof(DummyDataRows))]
		[ClassData(typeof(TestRows))]
		public void MaskBuilderTest(string keys, string expected, string expectedOnlyValue)
		{
			var profile = GetMaskingProfile();
			_profile = profile;
			ModifyProfile(profile);

			var masker = _masker = new Masker(profile);
			var objectMasker = new ObjectMasker(masker, profile);

			var jsonMasker = new JsonMasker(masker, profile);

			var data = CreateLogEntry().Picks(keys.Split(','));

			var sb = new StringBuilder();
			var sb2 = new StringBuilder();

			var jsonOptions = new JsonSerializerOptions()
			{
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			};
			var element = JsonSerializer.SerializeToElement(data, jsonOptions);

			MaskJsonElement(null, element, sb);

			if (data.Count == 1)
			{
				var element2 = JsonSerializer.SerializeToElement(data.First().Value, jsonOptions);
				MaskJsonElement(null, element2, sb2);
			}

			var actual = sb.ToString();
			var actual2 = sb2.ToString();
#if DEBUG
			var file = @"c:\work\a.log";
			File.WriteAllText(file, actual);

			if (actual2.Length > 0)
			{
				File.AppendAllText(file, "\r\n\r\nexpectedOnlyValue:\r\n" + expectedOnlyValue);
				File.AppendAllText(file, "\r\n\r\nactual2:\r\n" + actual2);
			}
#endif
			WriteLine(keys + "  expected:");
			WriteLine(actual);

			WriteLine("\r\n\r\n");
			WriteLine("expectedOnlyValue:");
			WriteLine(expectedOnlyValue);

			WriteLine("\r\n\r\n");
			WriteLine("actual2:");
			WriteLine(actual2);

			Assert.Equal(expected, actual);

			Assert.Equal(expectedOnlyValue, actual2);
			Assert.NotEqual(actual2, actual);

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
					{
						builder.Append('{');
						var isKvp = IsKvpObject(element, out string keyKey, out var key, out string valKey, out var value);
						if (isKvp)
						{
							var keyName = key.GetString();
							builder.Append('"').Append(keyKey).Append('"').Append(':')
								.Append('"').Append(keyName).Append('"').Append(',')
								.Append('"').Append(valKey).Append('"').Append(':');
							MaskJsonElement(keyName, value, builder);
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
					}
					break;
				case JsonValueKind.Array:
					builder.Append('[');

					var treatSingleArrayItemAsValue = true;
					if (treatSingleArrayItemAsValue && !string.IsNullOrEmpty(propertyName) && element.EnumerateArray().Count() == 1)
					{
						foreach (var child in element.EnumerateArray())
						{
							if (child.ValueKind == JsonValueKind.String
								|| (_profile.MaskJsonNumberEnabled && child.ValueKind == JsonValueKind.Number))
							{
								MaskJsonElement(propertyName, child, builder);
							}
							else
							{
								MaskJsonElement(null, child, builder);
							}
						}
					}
					else
					{
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
					}
					builder.Append(']');
					break;
				case JsonValueKind.String:
					//todo 
					{
						var value = element.GetString();

						if (!string.IsNullOrEmpty(propertyName) && _masker.TryMask(propertyName, value, out var masked))
						{
							builder.Append(string.Concat("\"", masked, "\"")); //todo quote
						}
						else if (_profile.MaskUrlEnabled && _profile.UrlKeys.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
						{
							masked = _masker.MaskUrl(value, true);

							builder.Append(string.Concat("\"", masked, "\"")); //todo quote
						}
						else if (propertyName != null && value != null && SerializedMaskAttempt(propertyName, value, builder))
						{
							//do nothing
						}
						else
						{
							//todo if Url enabled
							builder.Append(string.Concat("\"", value, "\"")); //todo quote
						}
					}
					break;
				case JsonValueKind.Number:
					{
						if (!string.IsNullOrEmpty(propertyName) && _profile.MaskJsonNumberEnabled && _masker.TryMask(propertyName, element.GetRawText(), out var masked))
						{
							if (masked == null) builder.Append("null");
							else if (masked != null && masked.All(char.IsDigit))
								builder.Append(masked);
							else
								builder.Append(string.Concat('"', masked, '"'));
						}
						else
						{
							builder.Append(element.GetRawText());
						}
					}
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
		private bool SerializedMaskAttempt(string key, string value, StringBuilder builder)
		{
			if (!_profile.MaskJsonSerializedEnabled || !_profile.SerializedKeys.Contains(key, StringComparer.OrdinalIgnoreCase)) return false;

			try
			{
				if (_profile.MaskJsonSerializedEnabled && TryParseJson(value, out var parsedNode))
				{
					//source[valueKeyName] = parsedNode;

					MaskJsonElement(key, parsedNode!.Value, builder);

					return true;
				}
				//else if (MaskXmlSerializedEnabled && TryParseXEle(value, out var element))
				//{
				//	var masked = MaskXmlElementString(element);
				//	source[valueKeyName] = masked;
				//	return true;
				//}
			}
			catch (Exception)
			{
				//todo Parse Json failed
			}
			return false;
		}

		bool TryParseJson(string value, out JsonElement? node)
		{
			node = null; value = value?.Trim();
			//here I think for JSON, it at least has 15 char?...
			if (value == null || value.Length < _profile.JsonMinLength || value == "null") return false;

			if (!(value.StartsWith("[") && value.EndsWith("]"))
				&& !(value.StartsWith("{") && value.EndsWith("}")))
				return false;

			var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(value), new JsonReaderOptions { });

			if (JsonElement.TryParseValue(ref reader, out var nodex))
			{
				node = nodex.Value;
				return true;
			}
			return false;
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
			foreach (var kv in _profile.KeyKeyValueKeys)
			{
				keyKey = kv.KeyKeyName;
				valKey = kv.ValueKeyName;
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
				if (flag == 3) return true;
			}
			return false;
		}
		#endregion
	}
}