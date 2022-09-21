namespace Slin.Masking.Tests
{
	public class DummyDataTestRows : TheoryData<string[], string>
	{
		public DummyDataTestRows()
		{
			var actual = "";

			actual = DummyData.Masked.user;

			Add(new[] { DummyData.Keys.user }, actual);

			actual = DummyData.Masked.data;
			Add(new[] { DummyData.Keys.data }, actual);

			actual = CurlyBraces(DummyData.Masked.amount);
			Add(new[] { DummyData.Keys.amount }, actual);

			actual = CurlyBraces(DummyData.Masked.transactionAmount);
			Add(new[] { DummyData.Keys.transactionAmount }, actual);

			actual = CurlyBraces(string.Concat(DummyData.Masked.amount, ",", DummyData.Masked.transactionAmount));
			Add(new[] { DummyData.Keys.amount, DummyData.Keys.transactionAmount }, actual);

			actual = CurlyBraces(DummyData.Masked.query);
			Add(new[] { DummyData.Keys.query }, actual);

			actual = CurlyBraces(DummyData.Masked.requestUrl);
			Add(new[] { DummyData.Keys.requestUrl }, actual);

			actual = CurlyBraces(DummyData.Masked.dataInBytes);
			Add(new[] { DummyData.Keys.dataInBytes }, actual);

			actual = CurlyBraces(DummyData.Masked.ResponseBody);
			Add(new[] { DummyData.Keys.ResponseBody }, actual);

			actual = CurlyBraces(DummyData.Masked.dictionary);
			Add(new[] { DummyData.Keys.dictionary }, actual);

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
}
