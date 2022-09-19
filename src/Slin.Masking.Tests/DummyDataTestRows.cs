namespace Slin.Masking.Tests
{
	public class DummyDataTestRows : TheoryData<string[], string>
	{
		public DummyDataTestRows()
		{
			var actual = "";

			actual = SquareBrackes(DummyData.Masked.user);

			Add(new[] { DummyData.Keys.user }, actual);

			actual = SquareBrackes(DummyData.Masked.data);
			Add(new[] { DummyData.Keys.data }, actual);

			actual = SquareBrackes(DummyData.Masked.amount);
			Add(new[] { DummyData.Keys.amount }, actual);

			actual = SquareBrackes(DummyData.Masked.transactionAmount);
			Add(new[] { DummyData.Keys.transactionAmount }, actual);

			actual = SquareBrackes(string.Concat(DummyData.Masked.amount, ",", DummyData.Masked.transactionAmount));
			Add(new[] { DummyData.Keys.amount, DummyData.Keys.transactionAmount }, actual);

			actual = SquareBrackes(DummyData.Masked.query);
			Add(new[] { DummyData.Keys.query }, actual);

			actual = SquareBrackes(DummyData.Masked.requestUrl);
			Add(new[] { DummyData.Keys.requestUrl }, actual);

			actual = SquareBrackes(DummyData.Masked.dataInBytes);
			Add(new[] { DummyData.Keys.dataInBytes }, actual);

			actual = SquareBrackes(DummyData.Masked.ResponseBody);
			Add(new[] { DummyData.Keys.ResponseBody }, actual);

			actual = SquareBrackes(DummyData.Masked.kvplist);
			Add(new[] { DummyData.Keys.kvplist }, actual);

			actual = SquareBrackes(DummyData.Masked.dictionary);
			Add(new[] { DummyData.Keys.dictionary }, actual);

		}
		public static string SquareBrackes(string str)
		{
			return string.Concat("[", str, "]");
		}
	}
}
