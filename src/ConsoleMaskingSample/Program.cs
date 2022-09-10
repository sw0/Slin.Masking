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
			var objMasker = new ObjectMasker(masker, profile);
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
			profile.Rules.Add("socialsecuritynumber", new MaskRuleDefinition
			{
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Format="*" }
				}
			});
			profile.Rules.Add("pan", new MaskRuleDefinition
			{
				KeyName = "^pan|creditcard|cardnumber$",
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Format="L3*R3", ValuePattern="^\\d{15}$" },
					new ValueFormatterDefinition{ Format="L4*R4", ValuePattern="^\\d{16}$" }
				}
			});
			profile.Rules.Add("name", new MaskRuleDefinition
			{
				KeyName = "^[fir|la]stname$",
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Format="L3"},
				}
			});
			profile.Rules.Add("authorization", new MaskRuleDefinition
			{
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Format="L8"},
				}
			});
			profile.Rules.Add("accesstoken", new MaskRuleDefinition
			{
				KeyName= "^(?:access)?token$",
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Format="L8"},
				}
			});
			profile.Rules.Add("rightx", new MaskRuleDefinition
			{
				Formatters = new List<ValueFormatterDefinition> {
					new ValueFormatterDefinition{ Format="R3"},
				}
			});

			return profile;
		}
	}

}