using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Slin.Masking.Tests
{
	internal class XmlMaskerTestRows : TheoryData<string, string, bool, string>
	{
		public XmlMaskerTestRows()
		{
			//case
			var body = new StringBuilder().AppendSoapNode(DummyData.Keys.ssn, DummyData.SSN);
			var bodyMasked = new StringBuilder().AppendSoapNode(DummyData.Keys.ssn, DummyData.SSN.Mask("*"));
			AddRow("basic-ssn", body.WrapSoapEnv(), true, bodyMasked.WrapSoapEnv(false)); //todo <xml... is missed

			//case
			body = body.AppendSoapNode(DummyData.Keys.dob, DummyData.DobStr);
			bodyMasked = bodyMasked.AppendSoapNode(DummyData.Keys.dob, DummyData.SSN.Mask("REDACTED"));
			AddRow("basic-dob", body.WrapSoapEnv(), true, bodyMasked.WrapSoapEnv(false)); //todo <xml... is missed

			//case
			body = new StringBuilder().AppendSoapNode(DummyData.Keys.Authorization, "Bearer " + DummyData.AccessToken);
			bodyMasked = new StringBuilder().AppendSoapNode(DummyData.Keys.Authorization, ("Bearer " + DummyData.AccessToken).Mask("L9*4R6"));
			AddRow("authentication", body.WrapSoapEnv(), true, bodyMasked.WrapSoapEnv(false)); //todo <xml... is missed



			//case
			var dicDob = new Dictionary<string, object> {
				{ "doB", DummyData.DobStr}
			};
			var dicDobMasked = new Dictionary<string, object> {
				{ "doB", DummyData.DobStr.Mask("REDACTED")}
			};
			var dicSsn = new Dictionary<string, object> {
				{ DummyData.Keys.ssn, DummyData.SSN}
			};
			var dicSsnMasked = new Dictionary<string, object> {
				{ DummyData.Keys.ssn, DummyData.SSN.Mask("*")}
			};
			var dicRequestUrl = new Dictionary<string, object> {
				{ DummyData.Keys.requestUrl, DummyData.requestUrlEncoded}
			};
			var dicRequestUrlMasked = new Dictionary<string, object> {
				{ DummyData.Keys.requestUrl, DummyData.Masked.requestUrlEncoded.Unpack(true)}
			};
			var dicBodyOfXml = new Dictionary<string, object> {
				{ DummyData.Keys.Body, DummyData.BodyOfXml4Embed}
			};
			var dicBodyOfXmlMasked = new Dictionary<string, object> {
				{ DummyData.Keys.Body, DummyData.Masked.BodyOfXml4Embed}
			};
			//var dicBody = new Dictionary<string, object> {
			//	{ DummyData.Keys.Body, DummyData.requestUrlEncoded}
			//};

			var user = new StringBuilder().AppendSoapNode(DummyData.Keys.FirstName, DummyData.FirstName, dicDob)
				.AppendSoapNode(DummyData.Keys.LastName, DummyData.LastName)
				.AppendSoapNode(DummyData.Keys.ssn, DummyData.SSN)
				.AppendSoapNode(DummyData.Keys.dob, DummyData.DobStr, new Dictionary<string, object>
				{ { "doB", DummyData.DobStr} })
				.AppendSoapNode(DummyData.Keys.requestUrl, DummyData.requestUrlEncoded)
				.AppendSoapNode(DummyData.Keys.queryEncoded, DummyData.queryEncoded)
				.AppendSoapNode(DummyData.Keys.Body, DummyData.BodyOfJson4Xml)
				;

			user.AppendSoapNode("kvplist", "", dicDob, false)
				.AppendSoapNode("kvprow", "", dicRequestUrl, false)
				.AppendSoapNode(DummyData.Keys.Key, DummyData.Keys.ssn)
				.AppendSoapNode(DummyData.Keys.Value, DummyData.SSN)
				.CloseSoapNode("kvprow")
				.AppendSoapNode("kvprow", "", dicSsn, false)
				.AppendSoapNode(DummyData.Keys.Key, DummyData.Keys.dob)
				.AppendSoapNode(DummyData.Keys.Value, DummyData.DobStr)
				.CloseSoapNode("kvprow")
				.AppendSoapNode("kvprow", "", dicBodyOfXml, false)
				.AppendSoapNode(DummyData.Keys.Key, DummyData.Keys.dob)
				.AppendSoapNode(DummyData.Keys.Value, "", dicSsn, false)
				.AppendSoapNode(DummyData.Keys.requestUrl, DummyData.requestUrlEncoded)
				.CloseSoapNode(DummyData.Keys.Value)
				.CloseSoapNode("kvprow")
				.CloseSoapNode("kvplist");


			var userMasked = new StringBuilder().AppendSoapNode(DummyData.Keys.FirstName, DummyData.FirstName.Mask("L2"), dicDobMasked)
				.AppendSoapNode(DummyData.Keys.LastName, DummyData.LastName.Mask("L2"))
				.AppendSoapNode(DummyData.Keys.ssn, DummyData.SSN.Mask("*"))
				.AppendSoapNode(DummyData.Keys.dob, DummyData.DobStr.Mask("REDACTED"), new Dictionary<string, object>
				{ { "doB", DummyData.DobStr.Mask("REDACTED")} })
				.AppendSoapNode(DummyData.Keys.requestUrl, DummyData.Masked.requestUrlEncoded.Unpack(true))
				.AppendSoapNode(DummyData.Keys.queryEncoded, DummyData.Masked.queryEncoded.Unpack(true))
				.AppendSoapNode(DummyData.Keys.Body, DummyData.Masked.BodyOfJson4Xml)
				;

			userMasked.AppendSoapNode("kvplist", "", dicDobMasked, false)
				.AppendSoapNode("kvprow", "", dicRequestUrlMasked, false)
				.AppendSoapNode(DummyData.Keys.Key, DummyData.Keys.ssn)
				.AppendSoapNode(DummyData.Keys.Value, DummyData.SSN.Mask("*"))
				.CloseSoapNode("kvprow")
				.AppendSoapNode("kvprow", "", dicSsnMasked, false)
				.AppendSoapNode(DummyData.Keys.Key, DummyData.Keys.dob)
				.AppendSoapNode(DummyData.Keys.Value, DummyData.DobStr.Mask("REDACTED"))
				.CloseSoapNode("kvprow")
				.AppendSoapNode("kvprow", "", dicBodyOfXmlMasked, false)
				.AppendSoapNode(DummyData.Keys.Key, DummyData.Keys.dob)
				.AppendSoapNode(DummyData.Keys.Value, "", dicSsnMasked, false)
					.AppendSoapNode(DummyData.Keys.requestUrl, DummyData.Masked.requestUrlEncoded.Unpack(true))
				.CloseSoapNode(DummyData.Keys.Value)
				.CloseSoapNode("kvprow")
				.CloseSoapNode("kvplist");

			body.Append(user.Insert(0, "<m:User>").Append("</m:User>"));
			bodyMasked.Append(userMasked.Insert(0, "<m:User>").Append("</m:User>"));
			AddRow("full-soap", body.WrapSoapEnv(), true, bodyMasked.WrapSoapEnv(false)); //todo <xml... is missed


			//case 
			{
				var element = DummyData.GetXElement();
				var elementMasked = DummyData.GetXElementMasked();
				var xml = element.ToString();
				var xmlMasked = elementMasked.ToString(SaveOptions.DisableFormatting);
				AddRow("full-xElement", xml, true, xmlMasked); //todo <xml... is missed
			}

			{

				var element = new XElement("root",
						new XElement("kvprow",// new XAttribute(DummyData.Keys.Body, DummyData.BodyOfJson),
							new XElement(DummyData.Keys.key, DummyData.Keys.Body),
							new XElement(DummyData.Keys.val, DummyData.BodyOfJson)
						)
					);
				var elementMasked = new XElement("root",
						new XElement("kvprow",// new XAttribute(DummyData.Keys.Body, DummyData.Masked.BodyOfJson),
							new XElement(DummyData.Keys.key, DummyData.Keys.Body),
							new XElement(DummyData.Keys.val, DummyData.Masked.BodyOfJson)
						)
					);

				var xml = element.ToString();
				var xmlMasked = elementMasked.ToString(SaveOptions.DisableFormatting);
				AddRow("piece-xml", xml, true, xmlMasked); //todo <xml... is missed
			}
		}
	}

	internal class XmlJsonMaskerTestRows : TheoryData<string, string, bool, string>
	{
		public XmlJsonMaskerTestRows()
		{
			var d1 = new { firstName = DummyData.FirstName, ssN = DummyData.SSN, Dob = DummyData.DobStr };
			var d1Str = JsonSerializer.Serialize(d1);
			var d1Masked = new
			{
				firstName = DummyData.FirstName.Mask("L2"),
				ssN = DummyData.SSN.Mask("*"),
				Dob = DummyData.DobStr.Mask("REDACTED")
			};
			var d1MaskedStr = JsonSerializer.Serialize(d1Masked);
			var body = new StringBuilder().AppendSoapNode(DummyData.Keys.ResponseBody, d1Str);
			var bodyMasked = new StringBuilder().AppendSoapNode(DummyData.Keys.ResponseBody, d1MaskedStr);
			AddRow("x-responsebody", body.WrapSoapEnv(), true, bodyMasked.WrapSoapEnv(false)); //todo <xml... is missed
		}
	}
}
