using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._07
{
    internal sealed class Solution : ISolution
    {
        private IEnumerable<List<int>> GetPermutations(params int[] choices)
        {
            if (!choices.Any())
            {
                return new [] {new List<int>()};
            }

            return choices.SelectMany((c, cIdx) =>
            {
                return GetPermutations(choices.Where((_, idx) => idx != cIdx).ToArray())
                    .Select(perm => new[] {c}.Concat(perm).ToList());
            }).ToList();
        }
        
        private async Task<long> RunForPermutation(IntCode.Program program, List<int> permutation)
        {
            var (a, b, c, d, e) = (permutation[0], permutation[1], permutation[2], permutation[3], permutation[4]);
            await Task.Yield();

            var programA = program.Clone();
            var programB = program.Clone();
            var programC = program.Clone();
            var programD = program.Clone();
            var programE = program.Clone();

            var computerA = new IntCode.Computer(programA);
            var computerB = new IntCode.Computer(programB);
            var computerC = new IntCode.Computer(programC);
            var computerD = new IntCode.Computer(programD);
            var computerE = new IntCode.Computer(programE);

            var streamA = new IntCode.IoStream { a, 0 };
            var streamB = new IntCode.IoStream {b};
            var streamC = new IntCode.IoStream {c};
            var streamD = new IntCode.IoStream {d};
            var streamE = new IntCode.IoStream {e};

            await Task.WhenAll(new[]
            {
                computerA.RunToCompletion(streamA, streamB),
                computerB.RunToCompletion(streamB, streamC),
                computerC.RunToCompletion(streamC, streamD),
                computerD.RunToCompletion(streamD, streamE),
                computerE.RunToCompletion(streamE, streamA)
            });

            return streamA.Last();
        }
        
        private async Task<long> Part1()
        {
            var permutations = GetPermutations(0, 1, 2, 3, 4);
            var program = await IntCode.Program.Load("07/input");

            var results = await Task.WhenAll(permutations.Select(async permutation => await RunForPermutation(program, permutation)));
            return results.Max();
        }

        private async Task<long> Part2()
        {
            var permutations = GetPermutations(5, 6, 7, 8, 9);
            var program = await IntCode.Program.Load("07/input");

            var results = await Task.WhenAll(permutations.Select(async permutation => await RunForPermutation(program, permutation)));
            return results.Max();
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
