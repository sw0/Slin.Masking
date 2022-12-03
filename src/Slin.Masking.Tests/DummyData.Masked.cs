using System.Globalization;
using System.Text;
using System.Text.Json;
using Slin.Masking;

namespace Slin.Masking.Tests
{
	public partial class DummyData
	{
		public sealed class Keys
		{
			public const string boolOfTrue = nameof(boolOfTrue);
			public const string NULL = nameof(NULL);
			public const string FirstName = nameof(FirstName);
			public const string LastName = nameof(LastName);
			public const string ssn = nameof(ssn);
			public const string dob = nameof(dob);
			public const string amount = nameof(amount);
			public const string transactionAmount = nameof(transactionAmount);
			public const string PrimaryAccountnumBER = nameof(PrimaryAccountnumBER);

			public const string user = nameof(user);
			public const string data = nameof(data);
			public const string kvp = nameof(kvp);
			public const string kvpObj = nameof(kvpObj);
			public const string kvpCls = nameof(kvpCls);
			public const string Key = nameof(Key);
			public const string Value = nameof(Value);
			public const string key = nameof(key);
			public const string val = nameof(val);

			public const string dataInBytes = nameof(dataInBytes);
			public const string arrayOfInt = nameof(arrayOfInt);
			public const string arrayOfStr = nameof(arrayOfStr);
			public const string arrayOfObj = nameof(arrayOfObj);
			public const string arrayOfKvpCls = nameof(arrayOfKvpCls);
			public const string arrayOfKvp = nameof(arrayOfKvp);
			public const string _nestedKvp = "nestedKvp";
			public const string _nestedKvpList = "nestedKvpList";
			public const string _nestedObj = "nestedObj";
			public const string _nestedKvpClass = "nestedKvpCls";
			public const string _nestedKvpClassList = "nestedKvpClsList";
			public const string arrayOfKvpNested = nameof(arrayOfKvpNested);

			public const string dictionary = nameof(dictionary);
			public const string dictionaryNested = nameof(dictionaryNested);

			public const string query = nameof(query);
			public const string queryEncoded = nameof(query);
			public const string formdata = nameof(formdata);
			public const string requestUrl = nameof(requestUrl);

			//request headers
			public const string headers = nameof(headers);
			public const string flatHeaders = nameof(flatHeaders);
			public const string Authorization = nameof(Authorization);

			//mix arbitrary data
            public const string MixedDataArbitrary = nameof(MixedDataArbitrary);

            public const string objectfield = nameof(objectfield);
			public const string reserialize = nameof(reserialize);
			public const string Body = nameof(Body);
			public const string ResponseBody = nameof(ResponseBody);

			public const string excludedX = nameof(excludedX);
			public const string excludedY = nameof(excludedY);
		}

		public class Masked
		{
			//simple types
			public const string boolOfTrue = "{\"boolOfTrue\":true}";
			public const string ssn = "{\"ssn\":\"*********\"}";
			public const string dob = "{\"dob\":\"REDACTED\"}";
			public const string amount = "{\"amount\":null}";
			public const string PrimaryAccountnumBER = "{\"PrimaryAccountnumBER\":\"1234********3456\"}";
			public const string transactionAmount = "{\"transactionAmount\":null}";


			//simple poco
			public const string user = "{\"user\":{\"UserName\":\"userslin\",\"FirstName\":\"Sh***\",\"LastName\":\"Sh***\",\"Password\":\"******\",\"DOB\":\"REDACTED\",\"SSN\":\"*********\",\"Pan\":\"6225********1234\",\"Amount\":null,\"Balance\":null}}";
			public const string data = "{\"data\":{\"SSN\":\"*********\",\"PAN\":\"1234********3456\"}}";
			public const string kvp = "{\"kvp\":{\"Key\":\"ssn\",\"Value\":\"*********\"}}";
			public const string kvpObj = "{\"kvpObj\":{\"key\":\"ssn\",\"val\":\"*********\",\"amount\":null}}";
			public const string kvpCls = "{\"kvpCls\":{\"Key\":\"ssn\",\"Value\":\"*********\",\"Amount\":null}}";
			//poco, Key without Value
			public const string Key = "{\"Key\":{\"Firstname\":\"Sh***\"}}";

			//array
			public const string dataInBytes = "{\"dataInBytes\":\"REDACTED\"}";

			public const string arrayOfInt = "{\"arrayOfInt\":[1,2,3]}";
			public const string arrayOfStr = "{\"arrayOfStr\":[\"good\",\"bad\",\"ugly\"]}";
			public const string arrayOfObj = "{\"arrayOfObj\":[{\"id\":1,\"amount\":null,\"ssn\":\"*********\"}]}";
			public const string arrayOfKvpCls = "{\"arrayOfKvpCls\":[{\"Key\":\"ssn\",\"Value\":\"*********\",\"Amount\":null}]}";

			//array complex
			public const string arrayOfKvp = "{\"arrayOfKvp\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"dob\",\"Value\":\"REDACTED\"},{\"Key\":\"requestUrl\",\"Value\":\"https://jd.com/firstname/sh***/lastname/li*\"}]}";
			public const string arrayOfKvpNested = "{\"arrayOfKvpNested\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"nestedKvp\",\"Value\":{\"Key\":\"ssn\",\"Value\":\"*********\"}},{\"Key\":\"nestedKvpList\",\"Value\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"dob\",\"Value\":\"REDACTED\"}]},{\"Key\":\"nestedObj\",\"Value\":{\"id\":1,\"amount\":null,\"ssn\":\"*********\"}}]}";

			//dictionary & dictionary complex
			public const string dictionary = "{\"dictionary\":{\"ssn\":\"*********\",\"dob\":\"REDACTED\"}}";
			public const string dictionaryNested = "{\"dictionaryNested\":{\"transactionAmount\":null,\"ssn\":\"*********\",\"nestedObj\":{\"id\":1,\"amount\":null,\"ssn\":\"*********\"},\"nestedKvp\":{\"Key\":\"ssn\",\"Value\":\"*********\"},\"nestedKvpList\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"dob\",\"Value\":\"REDACTED\"}]}}";


			//url, query/form-data
			public const string query = "{\"query\":\"ssn=*********&pan=1234********3456&dob=REDACTED&from=中国&to=世界&accesstoken=123456789****uvwxyz\"}";
			public const string queryEncoded = "{\"query\":\"ssn=*********&amp;pan=1234********3456&amp;dob=REDACTED&amp;from=中国&amp;to=世界&amp;accesstoken=123456789****uvwxyz\"}";
			public const string formdata = "{\"formdata\":\"ssn=*********&pan=1234********3456&dob=REDACTED&from=中国&to=世界&accesstoken=123456789****uvwxyz\"}";
			public const string requestUrl = "{\"requestUrl\":\"https://jd.com/firstname/sh***/lastname/li*?ssn=*********&pan=1234********3456&dob=REDACTED&from=中国&to=世界&accesstoken=123456789****uvwxyz\"}";
			public const string requestUrlEncoded = "{\"requestUrl\":\"https://jd.com/firstname/sh***/lastname/li*?ssn=*********&amp;pan=1234********3456&amp;dob=REDACTED&amp;from=中国&amp;to=世界&amp;accesstoken=123456789****uvwxyz\"}";

			//headers, treatSingleArrayItemAsValue
			public const string flatHeaders = "{\"flatHeaders\":[{\"Key\":\"Authorization\",\"Value\":\"dXNlcm5hb****dvcmQ=\"},{\"Key\":\"amount\",\"Value\":null},{\"Key\":\"X-Request-Id\",\"Value\":\"1f53f1b6-862e-4f9f-ac83-def5b81f69eb\"}]}";
			//treatSingleArrayItemAsValue=true
			public const string headers = "{\"headers\":[{\"Key\":\"Authorization\",\"Value\":[\"dXNlcm5hb****dvcmQ=\"]},{\"Key\":\"amount\",\"Value\":[null]},{\"Key\":\"X-Request-Id\",\"Value\":[\"1f53f1b6-862e-4f9f-ac83-def5b81f69eb\"]}]}";


			public const string reserialize = "{\"reserialize\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"nestedKvp\",\"Value\":{\"Key\":\"ssn\",\"Value\":\"*********\"}},{\"Key\":\"nestedKvpList\",\"Value\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"dob\",\"Value\":\"REDACTED\"}]},{\"Key\":\"nestedObj\",\"Value\":{\"id\":1,\"amount\":null,\"ssn\":\"*********\"}}]}";

			public const string reserialize2 = "{\"reserialize\":\"[{\\\"Key\\\":\\\"ssn\\\",\\\"Value\\\":\\\"*********\\\"},{\\\"Key\\\":\\\"nestedKvp\\\",\\\"Value\\\":{\\\"Key\\\":\\\"ssn\\\",\\\"Value\\\":\\\"*********\\\"}},{\\\"Key\\\":\\\"nestedKvpList\\\",\\\"Value\\\":[{\\\"Key\\\":\\\"ssn\\\",\\\"Value\\\":\\\"*********\\\"},{\\\"Key\\\":\\\"dob\\\",\\\"Value\\\":\\\"REDACTED\\\"}]},{\\\"Key\\\":\\\"nestedObj\\\",\\\"Value\\\":{\\\"id\\\":1,\\\"amount\\\":null,\\\"ssn\\\":\\\"*********\\\"}}]\"}";

			public const string reserializeNoMask = "[{\"Key\":\"ssn\",\"Value\":\"123456789\"},{\"Key\":\"nestedKvp\",\"Value\":{\"Key\":\"ssn\",\"Value\":\"123456789\"}},{\"Key\":\"nestedKvpList\",\"Value\":[{\"Key\":\"ssn\",\"Value\":\"123456789\"},{\"Key\":\"dob\",\"Value\":\"1988-01-01\"}]},{\"Key\":\"nestedObj\",\"Value\":{\"id\":1,\"amount\":9.99,\"ssn\":\"123456789\"}}]";
			
			public static readonly string ResponseBody = @"{""ResponseBody"":""<SOAP-ENV:Envelope xmlns:SOAP-ENV=\""http://www.w3.org/2001/12/soap-envelope\"" SOAP-ENV:encodingStyle=\""http://www.w3.org/2001/12/soap-encoding\""><SOAP-ENV:Body xmlns:m=\""http://www.xyz.org/quotation\""><m:GetResponse><m:Body>{\""FirstName\"":\""Sh***\"",\""ssn\"":\""*********\""}</m:Body><m:SSN>*********</m:SSN><m:authorization>Bearer 12****uvwxyz</m:authorization><m:accessToken>Bearer 12****uvwxyz</m:accessToken><m:User><m:FirstName dOB=\""REDACTED\"">Sh***</m:FirstName><m:FirstName>Li*</m:FirstName><m:SSN>*********</m:SSN><m:DOB dob=\""REDACTED\"">REDACTED</m:DOB><m:requestUrl>https://jd.com/firstname/sh***/lastname/li*?ssn=*********&amp;pan=1234********3456&amp;dob=REDACTED&amp;from=中国&amp;to=世界&amp;accesstoken=123456789****uvwxyz</m:requestUrl><m:query>ssn=*********&amp;pan=1234********3456&amp;dob=REDACTED&amp;from=中国&amp;to=世界&amp;accesstoken=123456789****uvwxyz</m:query><m:Body>{\""FirstName\"":\""Shawn\"",\""ssn\"":\""123456789\""BAD</m:Body><m:kvplist dob=\""REDACTED\""><m:kvprow requestUrl=\""https://jd.com/firstname/sh***/lastname/li*?ssn=*********&amp;pan=1234********3456&amp;dob=REDACTED&amp;from=中国&amp;to=世界&amp;accesstoken=123456789****uvwxyz\""><m:Key>SSN</m:Key><m:Value>*********</m:Value><m:Body dob=\""REDACTED\"">{\""FirstName\"":\""Sh***\"",\""ssn\"":\""*********\""}</m:Body></m:kvprow><m:kvprow dob=\""REDACTED\""><m:Key>DOB</m:Key><m:Value>REDACTED</m:Value></m:kvprow><m:kvprow Body=\""{&quot;ssn&quot;:&quot;*********&quot;,&quot;dob&quot;:&quot;REDACTED&quot;}\""><m:Key><m:DOB>REDACTED</m:DOB></m:Key><m:Value ssn=\""*********\""><m:requestUrl>https://jd.com/firstname/sh***/lastname/li*?ssn=*********&amp;pan=1234********3456&amp;dob=REDACTED&amp;from=中国&amp;to=世界&amp;accesstoken=123456789****uvwxyz</m:requestUrl></m:Value></m:kvprow><m:kvprow body=\""&lt;data&gt;&lt;ssn&gt;*********&lt;/ssn&gt;&lt;/data&gt;\""><m:Key>DOB</m:Key><m:Value ssn=\""*********\""><m:requestUrl>https://jd.com/firstname/sh***/lastname/li*?ssn=*********&amp;pan=1234********3456&amp;dob=REDACTED&amp;from=中国&amp;to=世界&amp;accesstoken=123456789****uvwxyz</m:requestUrl></m:Value></m:kvprow></m:kvplist></m:User></m:GetResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>""}";


			public static readonly string MixedDataArbitrary = JsonSerializer.Serialize(DummyData.CreateLogEntry().Picks(Keys.MixedDataArbitrary), MyJsonSerializerOptions);

            public static readonly string BodyOfJson = JsonSerializer.Serialize(new { ssn = DummyData.SSN.Mask("*"), requestUrl = requestUrl.Unpack(true), dob = "REDACTED" }, MyJsonSerializerOptions);
			public static readonly string BodyOfJson4Xml = JsonSerializer.Serialize(new { ssn = DummyData.SSN.Mask("*"), requestUrl = requestUrlEncoded.Unpack(true), dob = "REDACTED" }, MyJsonSerializerOptions);
			public static readonly string BodyOfXml = $"<data><ssn>{DummyData.SSN.Mask("*")}</ssn><Dob>{DummyData.DobStr.Mask("REDACTED")}</Dob><requestUrl>{requestUrlEncoded.Unpack(true)}</requestUrl></data>";
			public static readonly string BodyOfXml4Embed = BodyOfXml.Replace("&amp;", "&amp;amp;").Replace("<", "&lt;").Replace(">", "&gt;");

		}

		public static string Quotes(string str)
		{
			//return string.Concat("\"", str, '"');
			var sb = new StringBuilder();
			sb.Append('"');
			AppendStringEscape(sb, str, false, false);
			sb.Append('"');
			var strValueMasked = sb.ToString();
			return strValueMasked;
		}
		public static string CurlyBraces(string str)
		{
			return string.Concat("{", str, "}");
		}
		public static string SquareBrackes(string str)
		{
			return string.Concat("[", str, "]");
		}



        /// <summary>
        /// Checks input string if it needs JSON escaping, and makes necessary conversion
        /// </summary>
        /// <param name="destination">Destination Builder</param>
        /// <param name="text">Input string</param>
        /// <param name="escapeUnicode">Should non-ASCII characters be encoded</param>
        /// <param name="escapeForwardSlash"></param>
        /// <returns>JSON escaped string</returns>
        internal static void AppendStringEscape(StringBuilder destination, string text, bool escapeUnicode, bool escapeForwardSlash)
        {
            if (string.IsNullOrEmpty(text))
                return;

            StringBuilder sb = null;

            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];
                if (!RequiresJsonEscape(ch, escapeUnicode, escapeForwardSlash))
                {
                    sb?.Append(ch);
                    continue;
                }
                else if (sb is null)
                {
                    sb = destination;
                    sb.Append(text, 0, i);
                }

                switch (ch)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;

                    case '\\':
                        sb.Append("\\\\");
                        break;

                    case '\b':
                        sb.Append("\\b");
                        break;

                    case '/':
                        if (escapeForwardSlash)
                        {
                            sb.Append("\\/");
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;

                    case '\r':
                        sb.Append("\\r");
                        break;

                    case '\n':
                        sb.Append("\\n");
                        break;

                    case '\f':
                        sb.Append("\\f");
                        break;

                    case '\t':
                        sb.Append("\\t");
                        break;

                    default:
                        if (EscapeChar(ch, escapeUnicode))
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)ch);
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            if (sb is null)
                destination.Append(text);   // Faster to make single Append
        }

        internal static bool RequiresJsonEscape(char ch, bool escapeUnicode, bool escapeForwardSlash)
        {
            if (!EscapeChar(ch, escapeUnicode))
            {
                switch (ch)
                {
                    case '/': return escapeForwardSlash;
                    case '"':
                    case '\\':
                        return true;
                    default:
                        return false;
                }
            }
            return true;
        }

        private static bool EscapeChar(char ch, bool escapeUnicode)
        {
            if (ch < 32)
                return true;
            else
                return escapeUnicode && ch > 127;
        }
    }


	/// <summary>
	/// sample class with properties Key, Value (like KeyValuePair)
	/// </summary>
	public class KvpClass
	{
		public string? Key { get; set; }

		public object? Value { get; set; }

		public decimal Amount { get; set; } = 9.8m;

		public KvpClass()
		{

		}

		public KvpClass(string key, object value, decimal amount = 9.9m)
		{
			Key = key; Value = value; Amount = amount;
		}
	}
}
