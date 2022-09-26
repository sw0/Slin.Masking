using System;
using System.Collections.Generic;

namespace Slin.Masking
{
	/// <summary>
	/// like {"name":"ssn","val":"123456789"}, here KeyKeyName is 'name', ValueKeyName is 'val'
	/// </summary>
	public class KeyKeyValueKey
	{
		public static readonly IReadOnlyCollection<KeyKeyValueKey> DefaultKeyKeyValueKeys = new List<KeyKeyValueKey> {
			new KeyKeyValueKey("Key","Value"),
			new KeyKeyValueKey("key","value"),
		};

		/// <summary>
		/// the name of key.
		/// </summary>
		public string KeyKeyName { get; set; }
		/// <summary>
		/// the name of value
		/// </summary>
		public string ValueKeyName { get; set; }

		public KeyKeyValueKey()
		{
		}

		public static implicit operator KeyKeyValueKey(string input)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));

			var tmp = input.Trim().Split(',', ':');
			if (tmp.Length != 2)
			{
#if DEBUG
				throw new ArgumentException("input must use ',' or ':' to split the key and value");
#else
				return null;
#endif
			}
			return new KeyKeyValueKey(tmp[0], tmp[1]);
		}

		public KeyKeyValueKey(string keyKey, string valKey)
		{
			if (string.IsNullOrEmpty(keyKey))
				throw new ArgumentNullException(nameof(keyKey));
			if (string.IsNullOrEmpty(valKey))
				throw new ArgumentNullException(nameof(valKey));
			KeyKeyName = keyKey;
			ValueKeyName = valKey;
		}
	}
}
