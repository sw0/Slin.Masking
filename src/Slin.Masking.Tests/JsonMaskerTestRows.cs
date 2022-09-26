using static Slin.Masking.Tests.DummyData;

namespace Slin.Masking.Tests
{
	public class JsonMaskerTestRows : TheoryData<string, string, string>
	{
		public JsonMaskerTestRows()
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
			Add(Keys.query, Masked.query, Quotes(query));
			Add(Keys.formdata, Masked.formdata, Quotes(query));
			Add(Keys.requestUrl, Masked.requestUrl, Quotes(requestUrl));

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
			//reserialize XML
			Add(Keys.ResponseBody, Masked.ResponseBody, string.Concat('"', DummyData.Xml1, '"'));
		}

		private string Unpack(string json)
		{
			var result = json.Substring(0, json.Length - 1).Substring(json.IndexOf(':') + 1);
			return result;
		}
	}
}