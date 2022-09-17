using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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

	internal class MaskFormatterParameterPool
	{
		internal static ConcurrentDictionary<string, MaskFormatterOptions> _pool = new ConcurrentDictionary<string, MaskFormatterOptions>();

		internal static readonly Regex FormatRegex;

		static MaskFormatterParameterPool()
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

			FormatRegex = new Regex(str, RegexOptions.Compiled);
		}

		public static bool IsFormatMatched(string format)
		{
			//special case here, that null result is not supported for Formatter.
			if (format == "null") return true;
			if (format == "" || format == null) return false;

			//all the formats begins one of: L:Left,R:Right or REPLACEMENT,E:EMPTY,* or #
			if ("LRE*#".IndexOf(format[0]) == -1 || format.Length > 30)
				return false;

			var m = FormatRegex.Match(format);

			return m.Success;
		}

		public static bool TryGetParameters(string format, out MaskFormatterOptions options)
		{
			if (_pool.TryGetValue(format, out options))
			{
				return true;
			}

			var m = FormatRegex.Match(format);

			if (!m.Success)
			{
				options = default(MaskFormatterOptions);
				return false;
			}

			options = CreateMaskFormatterOptions(m);
			//options.MaxLength = 16;//todo from configuration

			_pool.TryAdd(format, options);

			return true;
		}

		public static MaskFormatterOptions CreateMaskFormatterOptions(Match m)
		{
			var parameters = new MaskFormatterOptions
			{
				IsEmailMode = m.Value.EndsWith("@")
			};

			string gvLeft = m.Groups["left"].Value;
			string gvMid = m.Groups["middle"].Value;
			string gvRight = m.Groups["right"].Value;
			string gvChar = m.Groups["char"].Value;

			if (gvLeft.StartsWith("#") || gvRight.StartsWith("#") || gvChar.Length > 1)
			{
				//mode like ###***###, ***
				parameters.Left = gvLeft.Length;
				parameters.Right = gvRight.Length;
				if (gvChar.Length != 1) parameters.Middle = gvChar.Length;//here use gvChar for middle
				parameters.Char = '*';
			}
			else
			{
				//mode like L2*3R2, *
				parameters.Left = gvLeft == "" ? 0 : Convert.ToInt32(gvLeft);
				parameters.Middle = gvMid == "" ? 0 : Convert.ToInt32(gvMid);
				parameters.Right = gvRight == "" ? 0 : Convert.ToInt32(gvRight);
				parameters.Char = gvChar == "" ? '*' : gvChar[0];
			}

			return parameters;
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
		public static readonly MaskFormatter Default = new MaskFormatter();

		private const int MaxLength = 16;//todo maybe can get it from configuration

		public object GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}

		public bool IsFormatMatched(string format)
		{
			return MaskFormatterParameterPool.IsFormatMatched(format);
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			//NOTE: return null in MaskFormatter, will be ignored.
			//That is original value will actually be returned in String.Format(new MaskFormatter(), "{0:null}", arg).
			if (format == "null") return null;
			//cases
			//Quick simple check. all the formats begins one of: L:Left,R:Right or REPLACEMENT,E:EMPTY,* or #
			if ("LRE*#".IndexOf(format[0]) == -1 || format.Length > 30)
			{
				//Console.WriteLine($" INVALID FORMAT 1: {format}");
				return HandleOtherFormats(format, arg);
			}

			if (arg == null) throw new Exception("null object is not allowed for masking");

			//Console.WriteLine("format: {0}, type: {1}", fmt, t.Name);
			var t = arg.GetType();

			if (t.Name == "String") //For now we only support string value
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
					return format.Substring("REPLACEMENT=".Length);
				}

				if (!MaskFormatterParameterPool.TryGetParameters(format, out var parameters))
				{
					//Console.WriteLine($" NOT matched 2: {format}");
					return HandleOtherFormats(format, arg);
				}

				parameters.Normalize(value, MaxLength);

				if (!parameters.IsValid)
				{
					return HandleOtherFormats(format, arg);
				}
				Contract.Assert(parameters.IsValid, "paramters should be valid");

				var chars = new char[parameters.ActualLength];//(new string(parameters.Char, length)).AsSpan();
				for (var i = 0; i < parameters.ActualLength; i++) chars[i] = parameters.Char;

				if (parameters.IsEmailMode)
				{
					//length of '@abc.com'
					var atDomainLen = value.Length - parameters.OriginalAtCharIndex;

					//fill @xyz.abc
					value.CopyTo(parameters.OriginalAtCharIndex, chars,
						parameters.OriginalAtCharIndex - (value.Length - parameters.ActualLength),
						atDomainLen);

					if (parameters.Left > 0)
					{
						value.CopyTo(0, chars, 0, parameters.Left);
					}

					if (parameters.Right > 0)
					{
						value.CopyTo(value.Length - parameters.Right - atDomainLen, chars, parameters.ActualLength - parameters.Right - atDomainLen, parameters.Right);
					}
				}
				else
				{
					if (parameters.Left > 0)
					{
						value.CopyTo(0, chars, 0, parameters.Left);
					}
					if (parameters.Right > 0)
					{
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
