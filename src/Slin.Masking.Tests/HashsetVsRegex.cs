using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Slin.Masking.Tests
{
    public class HashsetVsRegexTest : TestBase
    {
        public HashsetVsRegexTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(100, 1000)]
        [InlineData(1000, 1000)]
        [InlineData(2000, 1000)]
        public void HashsetCompareWithRegex(int keyCount, int repeatCount)
        {
            //we want to compare the performance to see if using Hashset would improve the 
            //performance by recuding execution of regular expressions

            //let's just use 4 expressions as example
            Regex[] regexes = new Regex[] {
                new Regex("(first|last)name", RegexOptions.IgnoreCase|RegexOptions.Compiled),
                new Regex("reg2", RegexOptions.IgnoreCase|RegexOptions.Compiled),
                new Regex("reg3", RegexOptions.IgnoreCase|RegexOptions.Compiled),
                new Regex("reg4", RegexOptions.IgnoreCase|RegexOptions.Compiled),
            };

            var unmatchedKeys = Enumerable.Range(1, keyCount)
                .Select(x => $"key{x}")
                .ToList();

            var hashset = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            unmatchedKeys.ForEach(x => hashset.Add(x));

            Stopwatch sw1 = Stopwatch.StartNew();
            //start calculte time cost base on hashset hit check
            for (var i = 0; i < repeatCount; i++)
            {
                foreach (var item in unmatchedKeys)
                {
                    if (!hashset.Contains(item))
                    {
                        WriteLine("[HashSet]this should not happen");
                    }
                }
            }
            var ts1 = sw1.Elapsed.TotalMilliseconds;
            WriteLine($"[HashSet]took {ts1}ms {keyCount} keys repeated {repeatCount} times");


            sw1.Restart();
            //start calculte time cost base on regular expression executions
            for (var i = 0; i < repeatCount; i++)
            {
                foreach (var item in unmatchedKeys)
                {
                    foreach (var regex in regexes)
                    {
                        if (regex.IsMatch(item))
                        {
                            WriteLine("[REGEX]this should not happen. Not matched");
                        }
                    }
                }
            }
            var ts2 = sw1.Elapsed.TotalMilliseconds;
            WriteLine($"[REGEX]took {ts2}ms for {keyCount} keys repeated {repeatCount} times");

            /*
             * output:
             *    [HashSet]took 2.2586ms 100 keys repeated 1000 times
             *    [REGEX]took 26.3308ms for 100 keys repeated 1000 times
             *    
             *    [HashSet]took 0.2397ms 100 keys repeated 100 times
             *    [REGEX]took 3.7085ms for 100 keys repeated 100 times   
             *    
             *    [HashSet]took 53.3716ms 2000 keys repeated 1000 times
             *    [REGEX]took 375.1216ms for 2000 keys repeated 1000 times
             *    
             * conclusion:
             *    use HashSet to keep the unmatched keys would reduce the execution of regular expressions, 
             *    and improve the performance, this would works especially when wen use a lot regular expressions in masking rules.
             **/
        }
    }
}
