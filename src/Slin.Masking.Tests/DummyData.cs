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
		public static readonly JavaScriptEncoder MyEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);

		public const string FirstName = "Shawn";
		public const string LastName = "Lin";
		public const string SSN = "123456789";
		public const string PAN = "1234567890123456";
		public const string DobStr = "1988-01-01";
		public const decimal Amount = 9.99m;
		public static readonly DateTime DOB = new(1988, 1, 1);

		public const string UrlQuery = "ssn=123456789&pan=1234567890123456&dob=1988-07-14&from=中国&to=世界&accesstoken=" + AccessToken;
		public const string UrlQuery1Encoded = "ssn=123456789&amp;pan=1234567890123456&amp;dob=1988-07-14";
		public const string UrlQueryWithQuestionMark = "?" + UrlQuery;
		public const string UrlQuery2Encoded = "?" + UrlQuery1Encoded;

		public const string Url1 = "https://jd.com/firstname/shawn/lastname/lin";
		public const string UrlFull = Url1 + UrlQueryWithQuestionMark;
		public const string UrlFullEncoded = Url1 + UrlQuery2Encoded;

		public static readonly byte[] DataInBytes = new byte[] { 147, 60, 217, 97, 159, 120, 123, 223, 72, 80, 239, 21, 138, 213, 44, 157, 246, 183, 25, 122, 72, 178, 15, 99, 189, 73, 64, 164, 228, 72, 190, 168, 10, 217, 251, 168, 235 };
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

			data.Add(Keys.query, UrlQueryWithQuestionMark);
			data.Add(Keys.formdata, UrlQuery);
			data.Add(Keys.requestUrl, UrlFull);


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




			data.Add(Keys.reserialize, JsonSerializer.Serialize(arrOfKvpNested, new JsonSerializerOptions { Encoder = MyEncoder }));

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
	}
}
