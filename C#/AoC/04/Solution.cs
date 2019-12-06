using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._04
{
    internal sealed class Solution : ISolution
    {
        private bool Validate1(string pw)
        {
            return pw.OrderBy(c => c).SequenceEqual(pw) && pw.GroupBy(c => c).Any(g => g.Count() >= 2);
        }

        private bool Validate2(string pw)
        {
            return pw.OrderBy(c => c).SequenceEqual(pw) && pw.GroupBy(c => c).Any(g => g.Count() == 2);
        }

        private string GetPassword(int value)
        {
            var s = value.ToString();
            while (s.Length < 6)
            {
                s = "0" + s;
            }
            return s;
        }
        
        private async Task<(int Lower, int Upper)> ParseInput()
        {
            var values = (await File.ReadAllTextAsync("04/input")).Split("-").Select(int.Parse).ToList();
            return (values[0], values[1]);
        }

        private async Task<int> Part1()
        {
            var bounds = await ParseInput();
            return Enumerable.Range(bounds.Lower, bounds.Upper - bounds.Lower + 1).Select(GetPassword).Count(Validate1);
        }
        private async Task<int> Part2()
        {
            var bounds = await ParseInput();
            return Enumerable.Range(bounds.Lower, bounds.Upper - bounds.Lower + 1).Select(GetPassword).Count(Validate2);
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
