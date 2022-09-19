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

namespace Slin.Masking.Tests
{

	public partial class DummyData
	{
		public sealed class Keys
		{
			public const string user = nameof(user);
			public const string data = nameof(data);
			public const string amount = nameof(amount);
			public const string transactionAmount = nameof(transactionAmount);
			public const string PrimaryAccountnumBER = nameof(PrimaryAccountnumBER);
			public const string query = nameof(query);
			public const string requestUrl = nameof(requestUrl);
			public const string dataInBytes = nameof(dataInBytes);
			public const string objectfield = nameof(objectfield);
			public const string kvpfield = nameof(kvpfield);
			public const string reserialize = nameof(reserialize);
			public const string Key = nameof(Key);
			public const string ResponseBody = nameof(ResponseBody);
			public const string ssn = nameof(ssn);
			public const string dob = nameof(dob);
			public const string kvplist = nameof(kvplist);
			public const string dictionary = nameof(dictionary);
			public const string kvp1 = nameof(kvp1);
			public const string excludedX = nameof(excludedX);
			public const string excludedY = nameof(excludedY);
			public const string Authorization = nameof(Authorization);
			public const string headers = nameof(headers);
		}

		public static readonly JavaScriptEncoder MyEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);

		public const string FirstName = "Shawn";
		public const string LastName = "Lin";
		public const string SSN = "123456789";
		public const string PAN = "1234567890123456";
		public const string DobStr = "1988-01-01";
		public static readonly DateTime DOB = new(1988, 1, 1);

		public const string UrlQuery1 = "ssn=123456789&pan=1234567890123456&dob=1988-07-14&from=中国&to=世界&accesstoken=" + AccessToken;
		public const string UrlQuery1Encoded = "ssn=123456789&amp;pan=1234567890123456&amp;dob=1988-07-14";
		public const string UrlQuery2 = "?" + UrlQuery1;
		public const string UrlQuery2Encoded = "?" + UrlQuery1Encoded;

		public const string Url1 = "https://jd.com/firstname/shawn/lastname/lin";
		public const string UrlFull = Url1 + UrlQuery2;
		public const string UrlFullEncoded = Url1 + UrlQuery2Encoded;

		public static readonly byte[] DataInBytes = new byte[0];
		public const string AccessToken = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		//basic auth: Convert.ToBase64String(Encoding.UTF8.GetBytes("username:password"))
		public const string AuthorizationBasic = "dXNlcm5hbWU6cGFzc3dvcmQ=";

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
			<m:requestUrl>{UrlFullEncoded}</m:requestUrl>
			<m:query>{UrlQuery1Encoded}</m:query>
            <m:Body>{{""FirstName"":""{FirstName}"",""ssn"":""{SSN}""BAD</m:Body>
			<m:kvplist dob=""{DobStr}"">
               <m:kvprow requestUrl=""{UrlFullEncoded}"">
                   <m:Key>SSN</m:Key>
                   <m:Value>{SSN}</m:Value>
                   <m:Body dob=""{DobStr}"">{{""FirstName"":""{FirstName}"",""ssn"":""{SSN}""}}</m:Body>
               </m:kvprow>
               <m:kvprow dob=""{DobStr}"">
                   <m:Key>DOB</m:Key>
                   <m:Value>{DobStr}</m:Value>
               </m:kvprow>
               <m:kvprow body=""{{&quot;ssn&quot;:&quot;{SSN}&quot;,&quot;dob&quot;:&quot;{DobStr}&quot;}}"">
                   <m:Key><m:DOB>{DobStr}</m:DOB></m:Key>
                   <m:Value ssn=""{SSN}""><m:requestUrl>{UrlFullEncoded}</m:requestUrl></m:Value>
               </m:kvprow>
               <m:kvprow body=""&lt;data&gt;&lt;ssn&gt;{SSN}&lt;/ssn&gt;&lt;/data&gt;"">
                   <m:Key>DOB</m:Key>
                   <m:Value ssn=""{SSN}""><m:requestUrl>{UrlFullEncoded}</m:requestUrl></m:Value>
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

		static DummyData()
		{
			DataInBytes = new byte[200];
			Random.Shared.NextBytes(DataInBytes);
		}

		public static List<KeyValuePair<string, object>> CreateLogEntry()
		{
			var data = new List<KeyValuePair<string, object>>();

			data.Add(new KeyValuePair<string, object>(Keys.user, DummyData.User));
			data.Add(new KeyValuePair<string, object>(Keys.data, new { SSN = "123456789", PAN = DummyData.PAN }));
			data.Add(new KeyValuePair<string, object>(Keys.amount, 99.99m));
			data.Add(new KeyValuePair<string, object>(Keys.transactionAmount, 99.99m));
			data.Add(new KeyValuePair<string, object>(Keys.PrimaryAccountnumBER, "1234567890123456"));
			data.Add(new KeyValuePair<string, object>("ts", "5.99ms"));
			data.Add(new KeyValuePair<string, object>(Keys.query, DummyData.UrlQuery2));
			data.Add(new KeyValuePair<string, object>(Keys.requestUrl, DummyData.UrlFull));
			data.Add(new KeyValuePair<string, object>(Keys.dataInBytes, DummyData.DataInBytes));

			var d1 = new
			{
				transId = "12",
				amount = 9.99m,
				ssn = DummyData.SSN,
				from = "中国"
			};
			data.Add(new KeyValuePair<string, object>(Keys.objectfield, d1));
			data.Add(new KeyValuePair<string, object>(Keys.kvpfield, DummyData.UrlQuery1));
			data.Add(new KeyValuePair<string, object>(Keys.reserialize, JsonSerializer.Serialize(d1, new JsonSerializerOptions { Encoder = MyEncoder })));
			data.Add(new KeyValuePair<string, object>(Keys.Key, new { Firstname = "Sghawn" }));
			data.Add(new KeyValuePair<string, object>(Keys.ResponseBody, Xml1));
			data.Add(new KeyValuePair<string, object>(Keys.kvplist, new List<KeyValuePair<string, object>>
			{
				new KeyValuePair<string, object>(Keys.ssn, DummyData.SSN ),
				new KeyValuePair<string, object>(Keys.dob, DummyData.DobStr ),
				new KeyValuePair<string, object>(Keys.requestUrl, DummyData.Url1 )
			}));
			data.Add(new KeyValuePair<string, object>(Keys.dictionary, new List<KeyValuePair<string, object>>
			{
				new KeyValuePair<string, object>(Keys.ssn, DummyData.SSN ),
				new KeyValuePair<string, object>(Keys.dob, DummyData.DobStr ),
				new KeyValuePair<string, object>(Keys.requestUrl, DummyData.Url1 )
			}));
			data.Add(new KeyValuePair<string, object>(Keys.kvp1, new { key = "ssn", val = DummyData.SSN }));
			data.Add(new KeyValuePair<string, object>(Keys.excludedX, new { key = "ssn", val = DummyData.SSN }));
			data.Add(new KeyValuePair<string, object>(Keys.excludedY, "ok"));

			var headers = new List<KeyValuePair<string, string[]>> {
				new KeyValuePair<string, string[]>(Keys.Authorization, new string[] { AuthorizationBasic }),
				new KeyValuePair<string, string[]>("X-Request-Id", new string[] { Guid.NewGuid().ToString() })
				};

			data.Add(new KeyValuePair<string, object>(Keys.headers, headers));

			return data;
		}
	}

	public static class DummyDataExtensions
	{
		public static List<KeyValuePair<string, object>> Picks(this List<KeyValuePair<string, object>> source, params string[] keys2keep)
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
	}
}
