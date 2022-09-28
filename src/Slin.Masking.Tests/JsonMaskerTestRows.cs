using static Slin.Masking.Tests.DummyData;

namespace Slin.Masking.Tests
{
	public class JsonMaskerTestRows : TheoryData<string, bool, string, string>
	{
		public JsonMaskerTestRows()
		{
			//simple types
			Add(Keys.boolOfTrue,true, Masked.boolOfTrue, "true");
			Add(Keys.ssn, true, Masked.ssn, Quotes(SSN));
			Add(Keys.dob, true, Masked.dob, Quotes(DobStr));
			Add("ts", true, "{\"ts\":\"5.99ms\"}", Quotes("5.99ms"));
			Add(Keys.PrimaryAccountnumBER, true, Masked.PrimaryAccountnumBER, Quotes(PAN));
			Add(Keys.transactionAmount, true, Masked.transactionAmount, Amount.ToString());

			//object
			Add(Keys.data, true, Masked.data, Unpack(Masked.data));
			Add(Keys.user, true, Masked.user, Unpack(Masked.user));
			Add(Keys.kvp, true, Masked.kvp, Unpack(Masked.kvp));
			Add(Keys.kvpObj, true, Masked.kvpObj, Unpack(Masked.kvpObj));
			Add(Keys.kvpCls, true, Masked.kvpCls, Unpack(Masked.kvpCls));
			Add(Keys.Key, true, Masked.Key, Unpack(Masked.Key));
			//headers array of key-values
			Add(Keys.flatHeaders, true, Masked.flatHeaders, Unpack(Masked.flatHeaders));
			Add(Keys.headers, true, Masked.headers, Unpack(Masked.headers));

			//url, query/form-data? support decode?
			Add(Keys.query, true, Masked.query, Quotes(query));
			Add(Keys.formdata, true, Masked.formdata, Quotes(query));
			Add(Keys.requestUrl, true, Masked.requestUrl, Quotes(requestUrl));

			//arrays
			Add(Keys.dataInBytes, true, Masked.dataInBytes, Quotes(Convert.ToBase64String(DataInBytes)));
			Add(Keys.arrayOfInt, true, Masked.arrayOfInt, Unpack(Masked.arrayOfInt));
			Add(Keys.arrayOfStr, true, Masked.arrayOfStr, Unpack(Masked.arrayOfStr));
			Add(Keys.arrayOfObj, true, Masked.arrayOfObj, Unpack(Masked.arrayOfObj));
			Add(Keys.arrayOfKvpCls, true, Masked.arrayOfKvpCls, Unpack(Masked.arrayOfKvpCls));

			//arrays complex
			Add(Keys.arrayOfKvp, true, Masked.arrayOfKvp, Unpack(Masked.arrayOfKvp));
			Add(Keys.arrayOfKvpNested, true, Masked.arrayOfKvpNested, Unpack(Masked.arrayOfKvpNested));
			Add(Keys.dictionary, true, Masked.dictionary, Unpack(Masked.dictionary));
			Add(Keys.dictionaryNested, true, Masked.dictionaryNested, Unpack(Masked.dictionaryNested));

			//reserialize JSON
			Add(Keys.reserialize, true, Masked.reserialize, Quotes(Masked.reserializeNoMask));
			Add(Keys.reserialize, false, Masked.reserialize2, Quotes(Masked.reserializeNoMask));
			//reserialize XML
			Add(Keys.ResponseBody, true, Masked.ResponseBody, string.Concat('"', DummyData.Xml1, '"'));
		}

		private string Unpack(string json)
		{
			var result = json.Substring(0, json.Length - 1).Substring(json.IndexOf(':') + 1);
			return result;
		}
	}
}