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
		public class Masked
		{
			public const string user = "{\"Key\":\"user\",\"Value\":{\"UserName\":\"userslin\",\"FirstName\":\"Sh***\",\"LastName\":\"Sh***\",\"Password\":\"******\",\"DOB\":\"REDACTED\",\"SSN\":\"*********\",\"Pan\":\"6225********1234\",\"Amount\":null,\"Balance\":null}}";

			public const string data = "{\"Key\":\"data\",\"Value\":{\"SSN\":\"*********\",\"PAN\":\"1234********3456\"}}";

			public const string amount = "{\"Key\":\"amount\",\"Value\":null}";

			public const string transactionAmount = "{\"Key\":\"transactionAmount\",\"Value\":null}";

			//Maybe we will have enhancement in future to allow speicial chars
			public const string query = "{\"Key\":\"query\",\"Value\":\"?ssn=*********\\u0026pan=1234********3456\\u0026dob=REDACTED\\u0026from=\\u4E2D\\u56FD\\u0026to=\\u4E16\\u754C\\u0026accesstoken=123456789****uvwxyz\"}";

			public const string requestUrl = "{\"Key\":\"requestUrl\",\"Value\":\"https://jd.com/firstname/sh***/lastname/li*?ssn=*********\\u0026pan=1234********3456\\u0026dob=REDACTED\\u0026from=\\u4E2D\\u56FD\\u0026to=\\u4E16\\u754C\\u0026accesstoken=123456789****uvwxyz\"}";

			public const string dataInBytes = "{\"Key\":\"dataInBytes\",\"Value\":\"REDACTED\"}";

			public const string ResponseBody = "{\"Key\":\"ResponseBody\",\"Value\":\"\\u003CSOAP-ENV:Envelope xmlns:SOAP-ENV=\\u0022http://www.w3.org/2001/12/soap-envelope\\u0022 SOAP-ENV:encodingStyle=\\u0022http://www.w3.org/2001/12/soap-encoding\\u0022\\u003E\\u003CSOAP-ENV:Body xmlns:m=\\u0022http://www.xyz.org/quotation\\u0022\\u003E\\u003Cm:GetResponse\\u003E\\u003Cm:Body\\u003E{\\u0022FirstName\\u0022:\\u0022Sh***\\u0022,\\u0022ssn\\u0022:\\u0022*********\\u0022}\\u003C/m:Body\\u003E\\u003Cm:SSN\\u003E*********\\u003C/m:SSN\\u003E\\u003Cm:authorization\\u003EBearer 12****uvwxyz\\u003C/m:authorization\\u003E\\u003Cm:accessToken\\u003EBearer 12****uvwxyz\\u003C/m:accessToken\\u003E\\u003Cm:User\\u003E\\u003Cm:FirstName dOB=\\u0022REDACTED\\u0022\\u003ESh***\\u003C/m:FirstName\\u003E\\u003Cm:FirstName\\u003ELi*\\u003C/m:FirstName\\u003E\\u003Cm:SSN\\u003E*********\\u003C/m:SSN\\u003E\\u003Cm:DOB dob=\\u0022REDACTED\\u0022\\u003EREDACTED\\u003C/m:DOB\\u003E\\u003Cm:requestUrl\\u003Ehttps://jd.com/firstname/sh***/lastname/li*?ssn=*********\\u0026amp;pan=1234********3456\\u0026amp;dob=REDACTED\\u003C/m:requestUrl\\u003E\\u003Cm:query\\u003Essn=*********\\u0026amp;pan=1234********3456\\u0026amp;dob=REDACTED\\u003C/m:query\\u003E\\u003Cm:Body\\u003E{\\u0022FirstName\\u0022:\\u0022Shawn\\u0022,\\u0022ssn\\u0022:\\u0022123456789\\u0022BAD\\u003C/m:Body\\u003E\\u003Cm:kvplist dob=\\u0022REDACTED\\u0022\\u003E\\u003Cm:kvprow requestUrl=\\u0022https://jd.com/firstname/sh***/lastname/li*?ssn=*********\\u0026amp;pan=1234********3456\\u0026amp;dob=REDACTED\\u0022\\u003E\\u003Cm:Key\\u003ESSN\\u003C/m:Key\\u003E\\u003Cm:Value\\u003E*********\\u003C/m:Value\\u003E\\u003Cm:Body dob=\\u0022REDACTED\\u0022\\u003E{\\u0022FirstName\\u0022:\\u0022Sh***\\u0022,\\u0022ssn\\u0022:\\u0022*********\\u0022}\\u003C/m:Body\\u003E\\u003C/m:kvprow\\u003E\\u003Cm:kvprow dob=\\u0022REDACTED\\u0022\\u003E\\u003Cm:Key\\u003EDOB\\u003C/m:Key\\u003E\\u003Cm:Value\\u003EREDACTED\\u003C/m:Value\\u003E\\u003C/m:kvprow\\u003E\\u003Cm:kvprow body=\\u0022{\\u0026quot;ssn\\u0026quot;:\\u0026quot;123456789\\u0026quot;,\\u0026quot;dob\\u0026quot;:\\u0026quot;1988-01-01\\u0026quot;}\\u0022\\u003E\\u003Cm:Key\\u003E\\u003Cm:DOB\\u003EREDACTED\\u003C/m:DOB\\u003E\\u003C/m:Key\\u003E\\u003Cm:Value ssn=\\u0022*********\\u0022\\u003E\\u003Cm:requestUrl\\u003Ehttps://jd.com/firstname/sh***/lastname/li*?ssn=*********\\u0026amp;pan=1234********3456\\u0026amp;dob=REDACTED\\u003C/m:requestUrl\\u003E\\u003C/m:Value\\u003E\\u003C/m:kvprow\\u003E\\u003Cm:kvprow body=\\u0022\\u0026lt;data\\u0026gt;\\u0026lt;ssn\\u0026gt;*********\\u0026lt;/ssn\\u0026gt;\\u0026lt;/data\\u0026gt;\\u0022\\u003E\\u003Cm:Key\\u003EDOB\\u003C/m:Key\\u003E\\u003Cm:Value ssn=\\u0022*********\\u0022\\u003E\\u003Cm:requestUrl\\u003Ehttps://jd.com/firstname/sh***/lastname/li*?ssn=*********\\u0026amp;pan=1234********3456\\u0026amp;dob=REDACTED\\u003C/m:requestUrl\\u003E\\u003C/m:Value\\u003E\\u003C/m:kvprow\\u003E\\u003C/m:kvplist\\u003E\\u003C/m:User\\u003E\\u003C/m:GetResponse\\u003E\\u003C/SOAP-ENV:Body\\u003E\\u003C/SOAP-ENV:Envelope\\u003E\"}";

			public const string kvplist = "{\"Key\":\"kvplist\",\"Value\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"dob\",\"Value\":\"REDACTED\"},{\"Key\":\"requestUrl\",\"Value\":\"https://jd.com/firstname/sh***/lastname/li*\"}]}";

			public const string dictionary = "{\"Key\":\"dictionary\",\"Value\":[{\"Key\":\"ssn\",\"Value\":\"*********\"},{\"Key\":\"dob\",\"Value\":\"REDACTED\"},{\"Key\":\"requestUrl\",\"Value\":\"https://jd.com/firstname/sh***/lastname/li*\"}]}";
		}

		public static string SquareBrackes(string str)
		{
			return string.Concat("[", str, "]");
		}
	}
}
