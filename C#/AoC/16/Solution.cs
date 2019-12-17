using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._16
{
    internal sealed class Solution : ISolution
    {
        private int[] GetPattern(int index, int length)
        {
            var basePattern = new[] {0, 1, 0, -1};
            var repeat = index + 1;
            var list = new List<int>();
            foreach (var value in basePattern)
            {
                for (var i = 0; i < repeat; i++)
                {
                    list.Add(value);
                }
            }

            return Enumerable.Range(0, length).Select(i => list[(i + 1) % list.Count]).ToArray();
        }

        private int Process(int[] sequence, int[] pattern)
        {
            var result = 0;
            for (var i = 0; i < sequence.Length; i++)
            {
                result += sequence[i] * pattern[i];
            }

            result = Math.Abs(result) % 10;
            //Console.WriteLine($"{string.Join(" + ", Enumerable.Range(0, sequence.Length).Select(i => $"{sequence[i]}*{pattern[i]}"))} = {result}");
            return result;
        }

        private int[] ApplyFlawedFrequencyTransmissionAlgorithm(int[] sequence, List<int[]> patterns)

    {
            int[] result = new int[sequence.Length];
            for (var i = 0; i < sequence.Length; i++)
            {
                var pattern = patterns[i];
                var res = Process(sequence, pattern);
                result[i] = res;
            }

            return result;
        }
        
        private async Task<int[]> ParseInput()
        {
            return (await File.ReadAllTextAsync("16/input")).Select(c => int.Parse(c.ToString())).ToArray();
        }

        private async Task<string> Part1()
        {
            var sequence = await ParseInput();
            var patterns = Enumerable.Range(0, sequence.Length).Select(i => GetPattern(i, sequence.Length)).ToList();
            //Console.WriteLine($"Input signal: {string.Join(string.Empty, sequence.Take(8))}\n");
            for (var i = 0; i < 100; i++)
            {
                sequence = ApplyFlawedFrequencyTransmissionAlgorithm(sequence, patterns);
                //Console.WriteLine($"\nAfter {i+1} phases: {string.Join(string.Empty,sequence.Take(8))}\n");
            }
            
            return string.Join(string.Empty, sequence.Take(8));
        }

        private async Task<string> Part2()
        {
            var sequence = await ParseInput();
            var messageOffset = int.Parse(string.Join(string.Empty, sequence.Take(7)));
            var repeatedSequence = new List<int>();
            for (var i = 0; i < 10000; i++)
            {
                repeatedSequence.AddRange(sequence);
            }
            sequence = repeatedSequence.Skip(messageOffset).ToArray();

            for (var i = 0; i < 100; i++)
            {
                var newSequence = new int[sequence.Length];
                var sum = 0;
                for (var j = 0; j < sequence.Length; j++)
                {
                    sum += sequence[sequence.Length - j - 1];
                    sum = sum % 10;
                    newSequence[newSequence.Length - j - 1] = sum;
                }

                sequence = newSequence;
            }

            return string.Join(string.Empty, sequence.Take(8));
        }

        public async Task Run()
        {
            var part1 = await Part1();
            Console.WriteLine($"Part 1: {part1}");

            var part2 = await Part2();
            Console.WriteLine($"Part 2: {part2}");
        }
    }
}
