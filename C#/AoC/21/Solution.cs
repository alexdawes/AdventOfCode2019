using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._21
{
    public sealed class Solution : ISolution
    {
        private async Task<long> Run(string[] instructions, bool log = false)
        {
            var computer = new IntCode.Computer(await IntCode.Program.Load("21/input"));
            computer.Start();

            await computer.Input.WriteString(string.Join("\n", instructions) + "\n");

            var cts = new CancellationTokenSource();
            if (log)
            {
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            Console.Write(await computer.Output.ReadLine(cts.Token));
                        }
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }, cts.Token);
            }

            await computer.WaitUntilCompleted();
            cts.CancelAfter(200);

            return computer.Output.ToList().Last();
        }

        private async Task<long> Part1()
        {
            var instructions = new[]
            {
                "NOT A J",
                "NOT B T",
                "OR T J",
                "NOT C T",
                "OR T J",
                "AND D J",
                "WALK"
            };
            return await Run(instructions);
        }

        private async Task<long> Part2()
        {
            var instructions = new[]
            {
                "NOT A J",
                "NOT B T",
                "OR T J",
                "NOT C T",
                "OR T J",
                "AND D J",
                "NOT H T",
                "NOT T T",
                "OR E T",
                "AND T J",
                "RUN"
            };
            return await Run(instructions);
        }

        public async Task Run()
        {
            var part1 = await Part1();
            Console.WriteLine($"Part 1: {part1}");

            var part2 = await Part2();
            Console.WriteLine($"Part 1: {part2}");
        }
    }
}
