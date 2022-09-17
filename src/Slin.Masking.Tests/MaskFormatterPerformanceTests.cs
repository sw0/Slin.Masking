using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Slin.Masking.Tests
{
	public class MaskFormatterPerformanceTests : TestBase
	{
		public MaskFormatterPerformanceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{ }

		private List<string> GetFormats(bool wrapped)
		{
			var formats = "L3*R3,L4*6R4,L4,R4,*4,L9*3R9".Split(',').Select(x => wrapped ? $"{{0:{x}}}" : x).ToList();

			return formats;
		}

		[Fact]
		public void MaskExecutionTimeCostTest()
		{
			var formats = GetFormats(true);

			int count = 100000;
			Stopwatch stopwatch = Stopwatch.StartNew();
			var formatter1 = new MaskFormatter();
			var formatter2 = new MaskFormatter();
			string result = "";
			var input = "1234567890123456";
			for (var i = 0; i < count; i++)
			{
				foreach (var item in formats)
				{
					result = string.Format(formatter1, item, input);
				}
			}
			var t1 = stopwatch.ElapsedMilliseconds;
			stopwatch.Stop();

			WriteLine($"mask with {formats.Count} formmats on {input} took {t1}ms for {count} rounds");
		}


		[Fact]
		public void RegexVsCacheTest()
		{
			var formats = GetFormats(false);

			int count = 100000;
			Stopwatch stopwatch = Stopwatch.StartNew();
			string result = "";
			var input = "1234567890123456";
			for (var i = 0; i < count; i++)
			{
				foreach (var item in formats)
				{
					var m = MaskFormatterParameterPool.FormatRegex.Match(item);
					if (m.Success)
					{
						MaskFormatterOptions opt = MaskFormatterParameterPool.CreateMaskFormatterOptions(m);
					}
					else
					{
						throw new Exception($"not match a: {item}");
					}
				}
			}
			var t1 = stopwatch.ElapsedMilliseconds;
			stopwatch.Restart();
			for (var i = 0; i < count; i++)
			{
				foreach (var item in formats)
				{
					var success = MaskFormatterParameterPool.TryGetParameters(item, out var options);
					if (!success)
					{
						throw new Exception($"not match B: {item}");
					}
				}
			}
			var t2 = stopwatch.ElapsedMilliseconds;
			stopwatch.Stop();

			WriteLine($"non-cached way time cost:{t1}ms, cached way time cost: {t2}ms");
			Assert.True(t2 < t1);
		}
	}
}
