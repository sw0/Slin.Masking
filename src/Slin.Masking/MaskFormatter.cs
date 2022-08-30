using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Slin.Masking
{
	public class MaskFormatterOptions
	{
		public int Left { get; set; }
		public int Middle { get; set; } = 16;
		public int Right { get; set; }
		public char Char { get; set; }

		public int MaxLength { get; set; } = 16;

		public void Normalize(int inputLength)
		{
			var max = Math.Min(inputLength, MaxLength);

			if (Left == 0 && Right == 0)
				Middle = Middle > 0 ? Middle : max;

			if (Left > 0 || Right > 0)
			{
				Middle = Middle > 0 ? Middle : (max - Right);
			}
		}

		public int CalcResultCharCount(int inputLength)
		{
			Normalize(inputLength);

			var total = Left + Middle + Right;
			//if (Left > 0 && Middle == 0 && Right == 0)
			//	return Math.Min(inputLength, MaxLength);
			//if (Left == 0 && Middle == 0 && Right > 0)
			//	return Math.Min(inputLength, MaxLength);
			return Math.Min(total, inputLength);
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
			+ @"|^(?<char>[\*xX])(?<middle>\d{1,2})?$"
			+ @"|^L(?<left>\d{1,2})(?:(?<char>[\*xX])(?<middle>\d{1,2})?)?$"
			+ @"|^R(?<right>\d{1,2})(?:(?<char>[\*xX])(?<middle>\d{1,2})?)?$"
			+ @"|^L(?<left>\d{1,2})(?:(?<char>[\*xX])(?<middle>\d{1,2})?)?R(?<right>\d{1,2})$";

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

			var m = REG_FMT.Match(format);

			return m.Success;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			//NOTE: return null in MaskFormatter, will be ignored.
			//That is original value will actually be returned in String.Format(new MaskFormatter(), "{0:null}", arg).
			if (format == "null") return null;
			//cases
			var m = REG_FMT.Match(format);
			if (m.Success)
			{
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

					var parameters = new MaskFormatterOptions
					{
						Left = m.Groups["left"].Value == "" ? 0 : Convert.ToInt32(m.Groups["left"].Value),
						Middle = m.Groups["middle"].Value == "" ? 0 : Convert.ToInt32(m.Groups["middle"].Value),
						Right = m.Groups["right"].Value == "" ? 0 : Convert.ToInt32(m.Groups["right"].Value),
						Char = m.Groups["char"].Value == "" ? '*' : m.Groups["char"].Value[0]
					};
					var length = parameters.CalcResultCharCount(value.Length);
					//$"Format: Left:{parameters.Left},Middle:{parameters.Middle},Right:{parameters.Right}! length:{length}"
					//.Dump($"format: {fmt}");

					var chars = new char[length];//(new string(parameters.Char, length)).AsSpan();
					for (var i = 0; i < length; i++) chars[i] = parameters.Char;

					if (parameters.Left > 0)
					{
						value.CopyTo(0, chars, 0, parameters.Left);
						//value.AsSpan(0, parameters.Left).CopyTo(chars);
					}
					//if (parameters.Right > 0 && parameters.Left > 0)
					//{
					//	//value.AsSpan(value.Length - parameters.Right - 1, parameters.Right)
					//	//.CopyTo(chars.AsSpan(parameters.Left + parameters.Middle, parameters.Right));
					//	value.CopyTo(value.Length - parameters.Right - 1, chars, parameters.Left + parameters.Middle, parameters.Right);
					//}
					if (parameters.Right > 0)
					{
						//value.AsSpan(value.Length - parameters.Right, parameters.Right)
						//.CopyTo(chars.AsSpan(length - parameters.Right, parameters.Right));
						value.CopyTo(value.Length - parameters.Right, chars, length - parameters.Right, parameters.Right);
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
			else
			{
				Console.WriteLine($" NOT matched: {format}");
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
