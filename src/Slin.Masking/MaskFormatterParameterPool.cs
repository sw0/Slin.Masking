using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Slin.Masking
{
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
}
