namespace Slin.Masking
{
	/// <summary>
	/// IMasker interface. Masking value as long as key/value matches the rules defined.
	/// It also support <see cref="IUrlMasker"/>
	/// </summary>
	public interface IMasker: IUrlMasker
	{
		/// <summary>
		/// check if the property name is defined as sensitive key in rules, it maybe found by regular expression.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="addToCache">if true, try to added to cache pool</param>
		/// <returns></returns>
		bool IsKeyDefined(string key, bool addToCache = false);

		/// <summary>
		/// mask the value if the key is configured in masking rules.
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		/// <param name="masked">masked result</param>
		/// <returns>if masker found, whatever it's masked or not, it will be true</returns>
		bool TryMask(string key, string value, out string masked);
	}

}
