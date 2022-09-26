using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Slin.Masking
{
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
