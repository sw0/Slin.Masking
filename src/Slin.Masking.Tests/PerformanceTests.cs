using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Slin.Masking.Tests
{
	public class PerformanceTests : TestBase
	{
		public PerformanceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{ }

		[Theory]
		[InlineData(1)]
		[InlineData(1000)]
		[InlineData(5000)]
		public void MaskObjectPerfTestWithOldway(int count)
		{
			var profile = GetMaskingProfile();
			ModifyProfile(profile);

			string output1 = "";
			string output2 = "";
			long ts1, ts2;

			var masker = new Masker(profile);
			var objMasker = new ObjectMasker(masker, profile);
			var objMaskerV1 = new ObjectMaskerV1(masker, profile);


			var data = DummyData.CreateLogEntry();

			Stopwatch sw = Stopwatch.StartNew();
#if DEBUG
			for (int i = 0; i < count; i++)
			{
				output1 = objMaskerV1.MaskObjectOldway(data);
			}
			ts1 = sw.ElapsedMilliseconds;
			WriteLine($"using MaskObject without StringBuilder, mask {count} times, took {sw.ElapsedMilliseconds}ms");
#endif

			sw.Restart();

			for (int i = 0; i < count; i++)
			{
				var sb = new StringBuilder();
				objMasker.MaskObject(data, sb);
				output2 = sb.ToString();
			}
			ts2 = sw.ElapsedMilliseconds;
			WriteLine($"using MaskObject with StringBuilder, mask {count} times, took {sw.ElapsedMilliseconds}ms");
			sw.Stop();
			/*
				Standard Output: 
				using MaskObject without StringBuilder, mask 10000 times, took 57002ms
				using MaskObject with StringBuilder, mask 10000 times, took 8898ms
			*/
			Assert.True(ts2 * 3 < ts1);
			Assert.Equal(output2, output1.Replace("\\u0026", "&").Replace("\\u4E2D\\u56FD", "中国").Replace("\\u4E16\\u754C", "世界"));
		}

		[Theory]
		[InlineData(1)]
		[InlineData(1000)]
		[InlineData(5000)]
		public void MaskObjectPerfTestWithSibling(int count)
		{
			var profile = GetMaskingProfile();
			ModifyProfile(profile);

			string output1 = "";
			string output2 = "";
			long ts1, ts2;

			var masker = new Masker(profile);
			var objMasker = new ObjectMasker(masker, profile);


			var data = DummyData.CreateLogEntry();

			Stopwatch sw = Stopwatch.StartNew();
			for (int i = 0; i < count; i++)
			{
				output1 = objMasker.MaskObject(data);
			}
			ts1 = sw.ElapsedMilliseconds;
			WriteLine($"using MaskObject without StringBuilder, mask {count} times, took {ts1}ms");

			sw.Restart();

			for (int i = 0; i < count; i++)
			{
				var sb = new StringBuilder();
				objMasker.MaskObject(data, sb);
				output2 = sb.ToString();
			}
			ts2 = sw.ElapsedMilliseconds;
			WriteLine($"using MaskObject with StringBuilder, mask {count} times, took {ts2}ms");
			sw.Stop();
			/*
			  Standard Output: 
			    using MaskObject without StringBuilder, mask 10000 times, took 9218ms
			    using MaskObject with StringBuilder, mask 10000 times, took 8452ms
			*/
			//Assert.True(Math.Abs(ts1 - ts2) < ts1 * 0.2);
			Assert.Equal(output2, output1);
		}
	}
}
