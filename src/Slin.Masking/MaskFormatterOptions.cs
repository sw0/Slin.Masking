using System;

namespace Slin.Masking
{
	internal struct MaskFormatterOptions
	{
		/*
		 * NOTE: when EmailMode, Left,Middel,Right is only for email left part(username, aka before '@').
		 */
		public int Left { get; set; }
		public int Middle { get; set; }
		public int Right { get; set; }
		public char Char { get; set; }

		//public int MaxLength { get; set; }

		public bool IsEmailMode { get; set; }

		public int ActualLength { get; set; }

		/// <summary>
		/// the index of '@' in value. used only when it's email mode.
		/// </summary>
		public int OriginalAtCharIndex { get; set; }

		public bool IsValid => ActualLength > 0;

		public void Normalize(string value, int maxLength)
		{
			if (value.Length <= 0) throw new Exception("value length is expected greater than 0");

			//if (maxLength <= 0)
			//	maxLength = MaxLength;

			if (IsEmailMode)
			{
				OriginalAtCharIndex = value.LastIndexOf('@');

				if (OriginalAtCharIndex == -1 || value.Length - OriginalAtCharIndex <= 5 || value.Length <= 5)
				{
					//Not Email if no '@'
					//and not a email probably if length is enough. at least 'a@a.cn'
					IsEmailMode = false;
					OriginalAtCharIndex = -1;
				}
			}

			//email components: local_part@domain
			var localPathLen = value.Length;
			int atDomainLen = 0; //length for '@abc.com'

			if (IsEmailMode)
			{
				localPathLen = OriginalAtCharIndex;
				atDomainLen = value.Length - OriginalAtCharIndex;
			}
			var max = Math.Min(localPathLen, maxLength);

			if (Left == 0 && Right == 0)
				Middle = Middle > 0 ? Middle : max;

			if (Left > 0 || Right > 0)
			{
				Middle = Middle > 0 ? Middle : Math.Max(0, (max - Right - Left));

				if (Left > max) { Left = max; Right = 0; }
				else if (Right > max) { Left = 0; Right = max; }
				else if (Left + Right > max)
				{
					Right = max - Left;
				}
			}

			var total = Left + Middle + Right;

			ActualLength = Math.Min(total, localPathLen);

			if (IsEmailMode && ActualLength > 0)
			{
				ActualLength += atDomainLen;
			}
		}
	}
}
