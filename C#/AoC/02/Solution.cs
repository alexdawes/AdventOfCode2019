using System;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._02
{
    internal sealed class Solution : ISolution
    {
        private async Task<long> Part1()
        {
            var program = await IntCode.Program.Load("02/input");
            program.Set(1, 12);
            program.Set(2, 2);
            var computer = new IntCode.Computer(program);
            computer.Start();
            await computer.WaitUntilCompleted();
            return program.Get(0);
        }

        private async Task<long> Part2()
        {
            var expected = 19690720;
            for (var noun = 0; noun < 100; noun++)
            {
                for (var verb = 0; verb < 100; verb++)
                {
                    var program = await IntCode.Program.Load("02/input");
                    program.Set(1, noun);
                    program.Set(2, verb);
                    var computer = new IntCode.Computer(program);
                    computer.Start();
                    await computer.WaitUntilCompleted();
                    var actual = program.Get(0);
                    if (actual == expected)
                    {
                        return 100 * noun + verb;
                    }
                }
            }

            throw new Exception("No answer found.");
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
