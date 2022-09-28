namespace Slin.Masking
{
	/// <summary>
	/// this only will handel string and number (if MaskJsonNumberEnabled is true)
	/// </summary>
	public enum ModeIfArray
	{
		/// <summary>
		/// default way, ignore property name. It will not mask {"ssn":["123456789"]}.
		/// For keyed definition, if default it will check <see cref="IObjectMaskingOptions.GlobalModeForArray"/>
		/// </summary>
		Default = 0,
		/// <summary>
		/// {"ssn":["123456789"]}, will be masked only when array item count is 1
		/// </summary>
		HandleSingle = 1,
		/// <summary>
		/// {"ssn":["123456789","101123456"]}, all items will be masked
		/// </summary>
		HandleAll = 2,
	}
}