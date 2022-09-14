using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Slin.Masking
{
	internal class MaskFormatterOptions
	{
		/*
		 * NOTE: when EmailMode, Left,Middel,Right is only for email left part(username, aka before '@').
		 */
		public int Left { get; set; }
		public int Middle { get; set; } = 16;
		public int Right { get; set; }
		public char Char { get; set; }

		public int MaxLength { get; set; } = 16;

		public bool IsEmailMode { get; set; }

		//public string OriginalValues { get; } = string.Empty;

		public int ActualLength { get; set; }

		/// <summary>
		/// the index of '@' in value. used only when it's email mode.
		/// </summary>
		public int OriginalAtCharIndex { get; set; } = -1;

		private void CalcResultCharCount(int inputLength)
		{
			Normalize(inputLength);

			var total = Left + Middle + Right;

			ActualLength = Math.Min(total, inputLength);
		}

		private void Normalize(int inputLength)
		{
			var max = Math.Min(inputLength, MaxLength);

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
		}

		public MaskFormatterOptions(Match m, string value)
		{
			IsEmailMode = m.Value.EndsWith("@");

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

			string gvLeft = m.Groups["left"].Value;
			string gvMid = m.Groups["middle"].Value;
			string gvRight = m.Groups["right"].Value;
			string gvChar = m.Groups["char"].Value;

			if (gvLeft.StartsWith("#") || gvRight.StartsWith("#") || gvChar.Length > 1)
			{
				//mode like ###***###, ***
				Left = gvLeft.Length;
				Right = gvRight.Length;
				if (gvChar.Length != 1) Middle = gvChar.Length;//here use gvChar for middle
				Char = '*';
			}
			else
			{
				//mode like L2*3R2, *
				Left = gvLeft == "" ? 0 : Convert.ToInt32(gvLeft);
				Middle = gvMid == "" ? 0 : Convert.ToInt32(gvMid);
				Right = gvRight == "" ? 0 : Convert.ToInt32(gvRight);
				Char = gvChar == "" ? '*' : gvChar[0];
			}

			//OriginalValues = $"Original: Left: {Left}, Middle:{Middle}, Right: {Right}";

			if (IsEmailMode)
			{
				CalcResultCharCount(OriginalAtCharIndex);
				ActualLength += value.Length - OriginalAtCharIndex;
			}
			else
			{
				CalcResultCharCount(value.Length);
			}
		}
	}

	public interface IMaskFormatter : IFormatProvider, ICustomFormatter
	{
		bool IsFormatMatched(string format);
	}

	/// <summary>
	/// custome mask formatter. You can use format like combinations of each of below(at lease one kind):
	/// 1: Optional: L + Char Count
	/// 2: Optional: * + optional asterisk count
	/// 3: Optional: R + Char Count
	/// The max length would be set as 16
	/// <remarks>Examples: L2  , R2, L2*R2, L4*8R4, *, *3 </remarks>
	/// </summary>
	public class MaskFormatter : IMaskFormatter, IFormatProvider, ICustomFormatter
	{
		protected static readonly Regex REG_FMT;

		static MaskFormatter()
		{
			var str = @"^REDACTED|EMPTY$"
			+ @"|^REPLACEMENT=(?<replacement>.{0,30})$"
			+ @"|^(?<char>\*+)(?<middle>\d{1,2})?@?$"
			+ @"|^L(?<left>\d{1,2})(?:(?<char>\*)(?<middle>\d{1,2})?)?@?$"
			+ @"|^R(?<right>\d{1,2})(?:(?<char>\*)(?<middle>\d{1,2})?)?@?$"
			+ @"|^L(?<left>\d{1,2})(?:(?<char>\*)(?<middle>\d{1,2})?)?R(?<right>\d{1,2})@?$"
			+ @"|^(?<left>#{1,10})(?<char>\*{1,10})(?<right>#{1,10})@?$"
			+ @"|^(?<left>#{1,10})(?<char>\*{1,10})@?$"
			+ @"|^(?<char>\*{1,10})(?<right>#{1,10})@?$";

			REG_FMT = new Regex(str, RegexOptions.Compiled);
		}

		public object GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}

		public bool IsFormatMatched(string format)
		{
			//special case here, that null result is not supported for Formatter.
			if (format == "null") return true;
			if (format == "" || format == null) return false;

			//all the formats begins one of: L:Left,R:Right or REPLACEMENT,E:EMPTY,* or #
			if ("LRE*#".IndexOf(format[0]) == -1 || format.Length > 30)
				return false;

			var m = REG_FMT.Match(format);

			return m.Success;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			//NOTE: return null in MaskFormatter, will be ignored.
			//That is original value will actually be returned in String.Format(new MaskFormatter(), "{0:null}", arg).
			if (format == "null") return null;
			//cases
			//all the formats begins one of: L:Left,R:Right or REPLACEMENT,E:EMPTY,* or #
			if ("LRE*#".IndexOf(format[0]) == -1 || format.Length > 30)
			{
				//Console.WriteLine($" INVALID FORMAT 1: {format}");
				return HandleOtherFormats(format, arg);
			}

			var m = REG_FMT.Match(format);
			if (!m.Success)
			{
				//Console.WriteLine($" NOT matched 2: {format}");
				return HandleOtherFormats(format, arg);
			}
			if (arg == null) throw new Exception("null object is not allowed for masking");//let's bypass this in code that should not passing null while masking

			//Console.WriteLine("format: {0}, type: {1}", fmt, t.Name);
			var t = arg.GetType();

			if (t.Name == "String")
			{
				var value = arg.ToString();
				if (string.IsNullOrEmpty(value)) return value;

				//special case:REDACTED
				if (format == "REDACTED") return format;
				//special case:EMPTY
				if (format == "EMPTY") return "";
				if (format == "null")
				{
					return null;
				}
				else if (format.StartsWith("REPLACEMENT="))
				{
					return m.Groups["replacement"].Value;
				}

				var parameters = new MaskFormatterOptions(m, value);

				//var length = parameters.CalcResultCharCount(value.Length);
				//$"Format: Left:{parameters.Left},Middle:{parameters.Middle},Right:{parameters.Right}! length:{length}"
				//.Dump($"format: {fmt}");

				var chars = new char[parameters.ActualLength];//(new string(parameters.Char, length)).AsSpan();
				for (var i = 0; i < parameters.ActualLength; i++) chars[i] = parameters.Char;

				if (parameters.IsEmailMode)
				{
					var lenStartsFromAtChar = value.Length - parameters.OriginalAtCharIndex;

					//fill @xyz.abc
					value.CopyTo(parameters.OriginalAtCharIndex, chars,
						parameters.OriginalAtCharIndex - (value.Length - parameters.ActualLength),
						lenStartsFromAtChar);

					var emailLeftPartLen = parameters.ActualLength - lenStartsFromAtChar;

					//var actualLeft = Math.Min(emailLeftPartLen, parameters.Left);
					if (parameters.Left > 0)
					{
						value.CopyTo(0, chars, 0, parameters.Left);
					}

					if (parameters.Right > 0)
					{
						//value.AsSpan(value.Length - parameters.Right, parameters.Right)
						//.CopyTo(chars.AsSpan(length - parameters.Right, parameters.Right));
						value.CopyTo(value.Length - parameters.Right - lenStartsFromAtChar, chars, parameters.ActualLength - parameters.Right - lenStartsFromAtChar, parameters.Right);
					}
				}
				else
				{
					if (parameters.Left > 0)
					{
						value.CopyTo(0, chars, 0, parameters.Left);
						//value.AsSpan(0, parameters.Left).CopyTo(chars);
					}
					if (parameters.Right > 0)
					{
						//value.AsSpan(value.Length - parameters.Right, parameters.Right)
						//.CopyTo(chars.AsSpan(length - parameters.Right, parameters.Right));
						value.CopyTo(value.Length - parameters.Right, chars, parameters.ActualLength - parameters.Right, parameters.Right);
					}
				}


				return new string(chars);
			}
			else
			{
				//todo 
				Console.WriteLine($" NOT string type: {t.FullName} in MaskFormatter");
				return HandleOtherFormats(format, arg);
			}
		}

		private string HandleOtherFormats(string format, object arg)
		{
			Console.WriteLine("HandleOtherFormats, format: {0}, arg: {1}", format, arg);
			if (arg is IFormattable)
				return ((IFormattable)arg).ToString(format, CultureInfo.CurrentCulture);
			else if (arg != null)
				return arg.ToString();
			else
				return String.Empty;
		}
	}
}
