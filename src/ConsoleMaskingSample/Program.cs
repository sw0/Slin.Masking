using Slin.Masking;

internal class Program
{
	private static void Main(string[] args)
	{
		Console.WriteLine("Hello, World!");


	}

	public class Sample
	{
		public void Run()
		{

			var obj = new
			{
				FirstName = "",
				LastName = "lin",
				SSN = "123456789",
			};

			var profile = GetProfile1();

			var masker = new Masker(profile);
			var objMasker = new ObjectMasker(masker, profile.)
		}

		MaskingProfile GetProfile1()
		{
			var profile = new MaskingProfile();

			profile.Enabled = true;
			profile.MaskUrlEnabled = true;
			profile.MaskingOptions = new MaskingOptions
			{
				KeyCaseInsensitive = true,
				ValueCaseInsensitive = true,
			};
			profile.Rules.Add("ssn", new MaskRuleDefinition
			{
				Formatters = new List<ValueFormatterDefinition> {
				 new ValueFormatterDefinition{ Format="*" }
				}
			});
		}
	}

}