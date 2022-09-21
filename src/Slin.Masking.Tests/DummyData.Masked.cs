using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Web;

namespace Slin.Masking.Tests
{
	public partial class DummyData
	{
		public sealed class Keys
		{
			public const string boolOfTrue = nameof(boolOfTrue);
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
			public const string formdata = nameof(formdata);
			public const string requestUrl = nameof(requestUrl);

			//request headers
			public const string headers = nameof(headers);
			public const string flatHeaders = nameof(flatHeaders);
			public const string Authorization = nameof(Authorization);


			public const string objectfield = nameof(objectfield);
			public const string reserialize = nameof(reserialize);
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
			public const string query = "{\"query\":\"?ssn=*********&pan=1234********3456&dob=REDACTED&from=中国&to=世界&accesstoken=123456789****uvwxyz\"}";
			public const string formdata = "{\"formdata\":\"ssn=*********&pan=1234********3456&dob=REDACTED&from=中国&to=世界&accesstoken=123456789****uvwxyz\"}";
			public const string requestUrl = "{\"requestUrl\":\"https://jd.com/firstname/sh***/lastname/li*?ssn=*********&pan=1234********3456&dob=REDACTED&from=中国&to=世界&accesstoken=123456789****uvwxyz\"}";

			//headers, treatSingleArrayItemAsValue
			public const string flatHeaders = "{\"flatHeaders\":[{\"Key\":\"Authorization\",\"Value\":\"dXNlcm5hb****dvcmQ=\"},{\"Key\":\"amount\",\"Value\":null},{\"Key\":\"X-Request-Id\",\"Value\":\"1f53f1b6-862e-4f9f-ac83-def5b81f69eb\"}]}";
			//treatSingleArrayItemAsValue=true
			public const string headers = "{\"headers\":[{\"Key\":\"Authorization\",\"Value\":[\"dXNlcm5hb****dvcmQ=\"]},{\"Key\":\"amount\",\"Value\":[null]},{\"Key\":\"X-Request-Id\",\"Value\":[\"1f53f1b6-862e-4f9f-ac83-def5b81f69eb\"]}]}";


			public const string reserialize = "{\"reserialize\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"nestedKvp\",\"Value\":{\"Key\":\"ssn\",\"Value\":\"*********\"}},{\"Key\":\"nestedKvpList\",\"Value\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"dob\",\"Value\":\"REDACTED\"}]},{\"Key\":\"nestedObj\",\"Value\":{\"id\":1,\"amount\":null,\"ssn\":\"*********\"}}]}";
			public const string reserializeNoMask = "[{\"Key\":\"ssn\",\"Value\":\"123456789\"},{\"Key\":\"nestedKvp\",\"Value\":{\"Key\":\"ssn\",\"Value\":\"123456789\"}},{\"Key\":\"nestedKvpList\",\"Value\":[{\"Key\":\"ssn\",\"Value\":\"123456789\"},{\"Key\":\"dob\",\"Value\":\"1988-01-01\"}]},{\"Key\":\"nestedObj\",\"Value\":{\"id\":1,\"amount\":9.99,\"ssn\":\"123456789\"}}]";
			public const string ResponseBody = "";

		}

		public static string Quotes(string str)
		{
			return string.Concat("\"", str, '"');
		}
		public static string CurlyBraces(string str)
		{
			return string.Concat("{", str, "}");
		}
		public static string SquareBrackes(string str)
		{
			return string.Concat("[", str, "]");
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
