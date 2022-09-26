using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Slin.Masking.Tests
{

	public partial class DummyData
	{
		public static readonly JavaScriptEncoder MyEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
		public static readonly JsonSerializerOptions MyJsonSerializerOptions = new()
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		public const string FirstName = "Shawn";
		public const string LastName = "Lin";
		public const string SSN = "123456789";
		public const string PAN = "1234567890123456";
		public const string DobStr = "1988-01-01";
		public const decimal Amount = 9.99m;
		public static readonly DateTime DOB = new(1988, 1, 1);

		public const string query = "ssn=123456789&pan=1234567890123456&dob=1988-07-14&from=中国&to=世界&accesstoken=" + AccessToken;
		public const string queryWithQMark = "?" + query;
		public const string queryEncoded = "ssn=123456789&amp;pan=1234567890123456&amp;dob=1988-07-14&amp;from=中国&amp;to=世界&amp;accesstoken=" + AccessToken;
		public const string queryEncodedWithQMark = "?" + queryEncoded;

		public const string Url1 = "https://jd.com/firstname/shawn/lastname/lin";
		public const string requestUrl = Url1 + queryWithQMark;
		public const string requestUrlEncoded = Url1 + queryEncodedWithQMark;

		public static readonly byte[] DataInBytes = new byte[] { 147, 60, 217, 97, 159, 120, 123, 223, 72, 80, 239, 21, 138, 213, 44, 157, 246, 183, 25, 122, 72, 178, 15, 99, 189, 73, 64, 164, 228, 72, 190, 168, 10, 217, 251, 168, 235 };
		public const string AccessToken = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		//basic auth: Convert.ToBase64String(Encoding.UTF8.GetBytes("username:password"))
		public const string AuthorizationBasic = "dXNlcm5hbWU6cGFzc3dvcmQ=";

		public static readonly string BodyOfJson = JsonSerializer.Serialize(new { ssn = SSN, requestUrl = requestUrl, dob = DOB }, MyJsonSerializerOptions);
		public static readonly string BodyOfJson4Xml = JsonSerializer.Serialize(new { ssn = SSN, requestUrl = requestUrlEncoded, dob = DOB }, MyJsonSerializerOptions);
		public static readonly string BodyOfXml = $"<data><ssn>{SSN}</ssn><Dob>{DobStr}</Dob><requestUrl>{requestUrlEncoded}</requestUrl></data>";
		public static readonly string BodyOfXml4Embed = BodyOfXml.Replace("&amp;", "&amp;amp;").Replace("<", "&lt;").Replace(">", "&gt;");

		public static readonly string Xml1 = $@"<?xml version = ""1.0""?>
<SOAP-ENV:Envelope xmlns:SOAP-ENV = ""http://www.w3.org/2001/12/soap-envelope"" SOAP-ENV:encodingStyle = ""http://www.w3.org/2001/12/soap-encoding"">

   <SOAP-ENV:Body xmlns:m = ""http://www.xyz.org/quotation"">
      <m:GetResponse>
         <m:Body>{{""FirstName"":""{FirstName}"",""ssn"":""{SSN}""}}</m:Body>
         <m:SSN>{SSN}</m:SSN>
         <m:authorization>Bearer {AccessToken}</m:authorization>
         <m:accessToken>Bearer {AccessToken}</m:accessToken>
         <m:User>
			<m:FirstName dOB=""{DobStr}"">{FirstName}</m:FirstName>
			<m:FirstName>{LastName}</m:FirstName>
			<m:SSN>{SSN}</m:SSN>
			<m:DOB dob=""{DobStr}"">{DobStr}</m:DOB>
			<m:requestUrl>{requestUrlEncoded}</m:requestUrl>
			<m:query>{queryEncoded}</m:query>
            <m:Body>{{""FirstName"":""{FirstName}"",""ssn"":""{SSN}""BAD</m:Body>
			<m:kvplist dob=""{DobStr}"">
               <m:kvprow requestUrl=""{requestUrlEncoded}"">
                   <m:Key>SSN</m:Key>
                   <m:Value>{SSN}</m:Value>
                   <m:Body dob=""{DobStr}"">{{""FirstName"":""{FirstName}"",""ssn"":""{SSN}""}}</m:Body>
               </m:kvprow>
               <m:kvprow dob=""{DobStr}"">
                   <m:Key>DOB</m:Key>
                   <m:Value>{DobStr}</m:Value>
               </m:kvprow>
               <m:kvprow Body=""{{&quot;ssn&quot;:&quot;{SSN}&quot;,&quot;dob&quot;:&quot;{DobStr}&quot;}}"">
                   <m:Key><m:DOB>{DobStr}</m:DOB></m:Key>
                   <m:Value ssn=""{SSN}""><m:requestUrl>{requestUrlEncoded}</m:requestUrl></m:Value>
               </m:kvprow>
               <m:kvprow body=""&lt;data&gt;&lt;ssn&gt;{SSN}&lt;/ssn&gt;&lt;/data&gt;"">
                   <m:Key>DOB</m:Key>
                   <m:Value ssn=""{SSN}""><m:requestUrl>{requestUrlEncoded}</m:requestUrl></m:Value>
               </m:kvprow>
            </m:kvplist>
         </m:User>
      </m:GetResponse>
   </SOAP-ENV:Body>
</SOAP-ENV:Envelope>".TrimLinesAndSpaces();

		public static readonly object User = new
		{
			UserName = "userslin",
			FirstName = "Shawn",
			LastName = "Shawn",
			Password = "123456",
			DOB = new DateTime(1988, 1, 1),
			SSN = "123456789",
			Pan = "6225000099991234",
			Amount = 9.9m,
			Balance = 9999.99m,
		};

		public static IDictionary<string, object> CreateLogEntry()
		{
			var data = new Dictionary<string, object>();

			var kvp = new KeyValuePair<string, object>(Keys.ssn, SSN);
			var kvplist = new[]{
					new KeyValuePair<string,object>(Keys.ssn, SSN),
					new KeyValuePair<string,object>(Keys.dob, DobStr)
				};
			var kvpCls = new KvpClass { Key = "ssn", Value = SSN };

			data.Add(Keys.boolOfTrue, true);
			data.Add(Keys.ssn, SSN);
			data.Add(Keys.dob, DobStr);
			data.Add(Keys.amount, Amount);
			data.Add(Keys.transactionAmount, Amount);
			data.Add(Keys.PrimaryAccountnumBER, "1234567890123456");
			data.Add("ts", "5.99ms");

			data.Add(Keys.query, query);
			data.Add(Keys.formdata, query);
			data.Add(Keys.requestUrl, requestUrl);


			var d1 = new { id = 1, amount = 9.99m, ssn = SSN };
			var kvpObj = new { key = "ssn", val = SSN, amount = Amount };

			data.Add(Keys.user, User);
			data.Add(Keys.data, new { SSN = SSN, PAN = PAN });
			data.Add(Keys.kvp, kvp);
			data.Add(Keys.kvpObj, kvpObj);
			data.Add(Keys.kvpCls, kvpCls);
			data.Add(Keys.Key, new { Firstname = "Shawn" });

			data.Add(Keys.arrayOfInt, new[] { 1, 2, 3 });
			data.Add(Keys.arrayOfStr, new[] { "good", "bad", "ugly" });
			data.Add(Keys.arrayOfObj, new[] { d1 });
			data.Add(Keys.arrayOfKvpCls, new[] { kvpCls });
			data.Add(Keys.dataInBytes, DataInBytes);



			data.Add(Keys.arrayOfKvp, new List<KeyValuePair<string, object>>
			{
				new KeyValuePair<string,object>(Keys.ssn, SSN),
				new KeyValuePair<string,object>(Keys.dob, DobStr ),
				new KeyValuePair<string,object>(Keys.requestUrl, Url1 )
			});

			var arrOfKvpNested = new List<KeyValuePair<string, object>>
			{
				new KeyValuePair<string,object>(Keys.ssn, SSN),
				new KeyValuePair<string,object>(Keys._nestedKvp, kvp),
				new KeyValuePair<string,object>(Keys._nestedKvpList, kvplist),
				new KeyValuePair<string,object>(Keys._nestedObj, d1 )
			};

			data.Add(Keys.arrayOfKvpNested, arrOfKvpNested);


			data.Add(Keys.dictionary, new Dictionary<string, object>
			{
				{ Keys.ssn, SSN },
				{ Keys.dob, DobStr },
			});

			data.Add(Keys.dictionaryNested, new Dictionary<string, object>
			{
				{ Keys.transactionAmount, 8.59m },
				{ Keys.ssn, SSN },
				{ Keys._nestedObj, d1 },
				{ Keys._nestedKvp, kvp},
				{ Keys._nestedKvpList, kvplist},
			});




			data.Add(Keys.reserialize, JsonSerializer.Serialize(arrOfKvpNested, MyJsonSerializerOptions));//new JsonSerializerOptions { Encoder = MyEncoder }));

			data.Add(Keys.ResponseBody, Xml1);
			data.Add(Keys.excludedX, new { key = "ssn", val = SSN });
			data.Add(Keys.excludedY, "ok");

			var headers = new List<KeyValuePair<string, object[]>> {
				new KeyValuePair<string, object[]>(Keys.Authorization, new string[] { AuthorizationBasic }),
				new KeyValuePair<string, object[]>(Keys.amount, new object[] {Amount }),
				new KeyValuePair<string, object[]>("X-Request-Id", new string[] { Guid.Parse("1f53f1b6-862e-4f9f-ac83-def5b81f69eb").ToString() })
				};

			data.Add(Keys.headers, headers);

			//actually same as arrayOfKvp
			var flatHeaders = new List<KeyValuePair<string, object>> {
				new KeyValuePair<string, object>(Keys.Authorization,  AuthorizationBasic ),
				new KeyValuePair<string, object>(Keys.amount,  Amount ),
				new KeyValuePair<string, object>("X-Request-Id", Guid.Parse("1f53f1b6-862e-4f9f-ac83-def5b81f69eb").ToString())
				};

			data.Add(Keys.flatHeaders, flatHeaders);

			return data;
		}


		public static XElement GetXElement()
		{
			var element = new XElement("root",
					new XAttribute(Keys.boolOfTrue, "true"),
					new XAttribute(Keys.ssn, SSN),
					new XElement(Keys.NULL, default(string)),
					new XElement(Keys.amount, Amount),
					new XElement(Keys.transactionAmount, 9.99m),
					new XElement(Keys.PrimaryAccountnumBER, new XCData(PAN)),
					new XElement(Keys.user,
						new XElement(Keys.FirstName, FirstName),
						new XElement(Keys.ssn, SSN),
						new XElement(Keys.dob, DobStr)
					),
					new XElement("kvplist", new XAttribute(Keys.requestUrl, requestUrl),
						new XElement("kvprow", new XAttribute(Keys.ssn, SSN),
							new XElement(Keys.Key, new XCData(Keys.ssn)),
							new XElement(Keys.Value, new XCData(SSN)),
							new XElement(Keys.boolOfTrue, true)
						),
						new XElement("kvprow", new XAttribute(Keys.ssn, SSN),
							new XElement(Keys.Key, new XCData(Keys.dob)),
							new XElement(Keys.Value, DOB)
						),
						new XElement("kvprow", new XAttribute(Keys.ssn, SSN),
							new XElement(Keys.Key.ToLowerInvariant(), Keys.dob),
							new XElement(Keys.Value.ToLowerInvariant(), DOB)
						),
						new XElement("kvprow", new XAttribute(Keys.Body, BodyOfJson),
							new XElement(Keys.key, Keys.Body),
							new XElement(Keys.val, BodyOfJson)
						),
						new XElement("kvprow",
							new XElement(Keys.key, Keys.ResponseBody),
							new XElement(Keys.val, BodyOfXml)
						)
					),
					new XElement(Keys.Body, new XCData(BodyOfJson)),
					new XElement(Keys.ResponseBody, new XCData(BodyOfXml))
				);

			return element;
		}

		public static XElement GetXElementMasked()
		{

			var elementMasked = new XElement("root",
					new XAttribute(Keys.boolOfTrue, "true"),
					new XAttribute(Keys.ssn, SSN.Mask("*")),
					new XElement(Keys.NULL, default(string)),
					new XElement(Keys.amount, Amount),//null is not supported in XML.
					new XElement(Keys.transactionAmount, 9.99m),
					new XElement(Keys.PrimaryAccountnumBER, new XCData(PAN.Mask("L4R4"))),
					new XElement(Keys.user,
						new XElement(Keys.FirstName, FirstName.Mask("L2")),
						new XElement(Keys.ssn, SSN.Mask("*")),
						new XElement(Keys.dob, DobStr.Mask("REDACTED"))
					),
					new XElement("kvplist", new XAttribute(Keys.requestUrl, Masked.requestUrl.Unpack(true)),
						new XElement("kvprow", new XAttribute(Keys.ssn, SSN.Mask("*")),
							new XElement(Keys.Key, new XCData(Keys.ssn)),
							new XElement(Keys.Value, new XCData(SSN.Mask("*"))),
							new XElement(Keys.boolOfTrue, true)
						),
						new XElement("kvprow", new XAttribute(Keys.ssn, SSN.Mask("*")),
							new XElement(Keys.Key, new XCData(Keys.dob)),
							new XElement(Keys.Value, DobStr.Mask("REDACTED"))
						),
						new XElement("kvprow", new XAttribute(Keys.ssn, SSN.Mask("*")),
							new XElement(Keys.Key.ToLowerInvariant(), Keys.dob),
							new XElement(Keys.Value.ToLowerInvariant(), DobStr.Mask("REDACTED"))
						),
						new XElement("kvprow", new XAttribute(Keys.Body, Masked.BodyOfJson),
							new XElement(Keys.key, Keys.Body),
							new XElement(Keys.val, Masked.BodyOfJson)
						),
						new XElement("kvprow",
							new XElement(Keys.key, Keys.ResponseBody),
							new XElement(Keys.val, Masked.BodyOfXml)
						)
					),
					new XElement(Keys.Body, new XCData(Masked.BodyOfJson)),
					new XElement(Keys.ResponseBody, new XCData(Masked.BodyOfXml))
				);

			return elementMasked;
		}
	}

	public static class DummyDataExtensions
	{
		public static IDictionary<string, object> Picks(this IDictionary<string, object> source, params string[] keys2keep)
		{
			var result = source.Where(x => keys2keep.Contains(x.Key)).ToList();

			return result.ToDictionary(x => x.Key, x => x.Value);
		}

		public static IEnumerable<KeyValuePair<string, object>> Picks(this IEnumerable<KeyValuePair<string, object>> source, params string[] keys2keep)
		{
			var result = source.Where(x => keys2keep.Contains(x.Key)).ToList();

			return result;
		}

		public static string TrimLinesAndSpaces(this string str, bool removeLines = true, bool removeStartSpaces = true, bool removeEndingSpaces = true)
		{
			var result = str;
			if (removeStartSpaces) result = Regex.Replace(result, @"\r\n\s+", "\r\n");
			if (removeEndingSpaces) result = Regex.Replace(result, @"\s+\r\n", "\r\n");

			if (removeLines) result = Regex.Replace(result, "\r\n", "");

			return result;
		}
		public static string Unpack(this string json, bool trimQuotes = false)
		{
			var result = json.Substring(0, json.Length - 1).Substring(json.IndexOf(':') + 1);
			if (trimQuotes) result = result.Trim('"');
			return result;
		}
		public static string Quotes(this string str)
		{
			return string.Concat("\"", str, '"');
		}
		public static string CurlyBraces(this string str)
		{
			return string.Concat("{", str, "}");
		}
		public static string SquareBrackes(this string str)
		{
			return string.Concat("[", str, "]");
		}
		public static string WrapXml(this string input)
		{
			return string.Concat("<xml version = \"1.0\"?>", input, "</xml>");
		}
		public static string WrapSoapEnv(this string input, bool withDeclaration = true)
		{
			return string.Concat(withDeclaration ? "<?xml version = \"1.0\"?>" : "",
				@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://www.w3.org/2001/12/soap-envelope"" SOAP-ENV:encodingStyle=""http://www.w3.org/2001/12/soap-encoding""><SOAP-ENV:Body xmlns:m=""http://www.xyz.org/quotation""><m:GetResponse>",
				input,
				@"</m:GetResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
		}
		public static string WrapSoapEnv(this StringBuilder input, bool withDeclaration = true)
		{
			return string.Concat(withDeclaration ? "<?xml version = \"1.0\"?>" : "",
				@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://www.w3.org/2001/12/soap-envelope"" SOAP-ENV:encodingStyle=""http://www.w3.org/2001/12/soap-encoding""><SOAP-ENV:Body xmlns:m=""http://www.xyz.org/quotation""><m:GetResponse>",
				input.ToString(),
				@"</m:GetResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
		}

		public static StringBuilder AppendSoapNode(this StringBuilder sb, string key, string value, bool autoEnd = true, string ns = "m:")
		{
			//return input + string.Concat($"<{ns}{key}>", value, $"</{ns}{key}>");

			return sb.AppendSoapNode(key, value, new Dictionary<string, object>(), autoEnd, ns);
		}

		public static StringBuilder AppendSoapNode(this StringBuilder sb, string key, string value, Dictionary<string, object> attributes, bool autoEnd = true, string ns = "m:")
		{
			//var sb = new StringBuilder(input.Length + 50);
			//sb.Append(input);

			sb.Append($"<{ns}{key}");
			if (attributes != null && attributes.Any())
			{
				foreach (var item in attributes)
				{
					sb.Append(' ').Append(item.Key).Append('=');
					sb.Append('"').Append(item.Value).Append('"');  //XML always has quotes
				}
			}
			sb.Append('>');

			if (value != null) sb.Append(value);
			if (autoEnd) sb.CloseSoapNode(key);

			return sb;
			//return input + string.Concat($"<{ns}{key}>", value, $"</{ns}{key}>");
		}

		public static StringBuilder CloseSoapNode(this StringBuilder sb, string key, string ns = "m:")
		{
			return sb.Append($"</{ns}{key}>");
		}
	}
}
