using System;
using System.Linq;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._09
{
    internal sealed class Solution : ISolution
    {
        private async Task<long> Part1()
        {
            var program = await IntCode.Program.Load("09/input");
            var computer = new IntCode.Computer(program);
            await computer.Input.Write(1);
            computer.Start();
            await computer.WaitUntilCompleted();
            return computer.Output.Last();
        }

        private async Task<long> Part2()
        {
            var program = await IntCode.Program.Load("09/input");
            var computer = new IntCode.Computer(program);
            await computer.Input.Write(2);
            computer.Start();
            await computer.WaitUntilCompleted();
            return computer.Output.Last();
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
