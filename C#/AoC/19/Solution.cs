using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._19
{
    public sealed class Solution : ISolution
    {
        private async Task<IntCode.Program> ParseInput()
        {
            return await IntCode.Program.Load("19/input");
        }

        private IEnumerable<(long X, long Y)> GetCoords(long x1, long x2, long y1, long y2)
        {
            for (var x = x1; x <= y2; x++)
            {
                for (var y = y1; y <= y2; y++)
                {
                    yield return (x, y);
                }
            }
        }

        private void Print(IDictionary<(long X, long Y), bool> results)
        {
            var (x1, x2, y1, y2) = (results.Keys.Min(r => r.X), results.Keys.Max(r => r.X), results.Keys.Min(r => r.Y),
                                    results.Keys.Max(r => r.Y));
            var result = new StringBuilder();
            for (var y = y1; y <= y2; y++)
            {
                var line = new StringBuilder();
                for (var x = x1; x <= x2; x++)
                {
                    if (!results.ContainsKey((x, y)))
                    {
                        line.Append(' ');
                    }
                    else if (results[(x, y)])
                    {
                        line.Append('#');
                    }
                    else
                    {
                        line.Append('.');
                    }
                }

                result.AppendLine(line.ToString());
            }
            Console.WriteLine(result.ToString());
        }

        private async Task<IDictionary<(long X, long Y), bool>> Scan(long x1, long x2, long y1, long y2)
        {
            var program = await ParseInput();
            var coords = GetCoords(x1, x2, y1, y2);

            return (await Task.WhenAll(coords.Select(async coord =>
            {
                var computer = new IntCode.Computer(program.Clone());
                computer.Start();
                await computer.Input.Write(coord.X);
                await computer.Input.Write(coord.Y);
                var r = await computer.Output.Read();
                computer.Stop();
                return (X: coord.X, Y: coord.Y, Result: r == 1);
            }))).ToDictionary(r => (X: r.X, Y: r.Y), r => r.Result);
        }

        private async Task<bool> Test(long x1, long x2, long y1, long y2)
        {
            var program = await ParseInput();
            var coords = GetCoords(x1, x2, y1, y2).Where(c => (c.X == x1 || c.X == x2) && (c.Y == y1 || c.Y == y2));

            return (await Task.WhenAll(coords.Select(async coord =>
            {
                var computer = new IntCode.Computer(program.Clone());
                computer.Start();
                await computer.Input.Write(coord.X);
                await computer.Input.Write(coord.Y);
                var r = await computer.Output.Read();
                computer.Stop();
                return (X: coord.X, Y: coord.Y, Result: r == 1);
            }))).All(r => r.Result);
        }

        private async Task<bool> TestPoint(long x, long y)
        {
            return await Test(x, x, y, y);
        }

        private async Task<bool> TestRow(long x, long y)
        {
            return await Test(x, x + 99, y, y);
        }

        private async Task<bool> TestColumn(long x, long y)
        {
            return await Test(x, x, y, y + 99);
        }

        private async Task<bool> TestBlock(long x, long y)
        {
            return await Test(x, x + 99, y, y + 99);
        }

        private async Task<double> GetSlope(int rowNum = 400)
        {
            var width = rowNum;
            var result = await Scan(0, width, rowNum, rowNum);
            while (result[(width, rowNum)])
            {
                width = width + rowNum;
                result = await Scan(0, width, rowNum, rowNum);
            }

            var (min, max) = (result.Where(r => r.Value).Select(r => r.Key.X).Min(),
                              result.Where(r => r.Value).Select(r => r.Key.X).Max());

            var mid = (min + max) / 2;

            return (double)rowNum / mid;
        }

        private async Task<long> Part1()
        {
            var results = await Scan(0, 49, 0, 49);
            return results.Count(r => r.Value);
        }

        private async Task<long> Part2()
        {
            var slope = await GetSlope();

            var (x, y) = ((long)(1000 / slope), 1000L);
            while (!await TestBlock(x, y))
            {
                y = y + 100;
                x = (long)(y / slope);
            }

            while (true)
            {
                var shifts = new List<(int X, int Y)>
                {
                    (1, 0),
                    (0, 1),
                    (1, 1),
                    (2, 1),
                    (1, 2),
                    (3, 1),
                    (1, 3),
                    (4, 1),
                    (1, 4),
                    (2, 2),
                    (2, 3),
                    (3, 2),
                    (2, 4),
                    (4, 2),
                    (3, 3),
                    (4, 3),
                    (3, 4),
                    (4, 4)
                };
                var cont = false;
                foreach (var s in shifts)
                {
                    if (await TestBlock(x - s.X, y - s.Y))
                    {
                        x -= s.X;
                        y -= s.Y;
                        cont = true;
                        break;
                    }
                }

                if (cont)
                {
                    continue;
                }
                break;
            }

            return 10000 * x + y;
        }

        public async Task Run()
        {
            //var part1 = await Part1();
            //Console.WriteLine($"Part 1: {part1}");

            var part2 = await Part2();
            Console.WriteLine($"Part 2: {part2}");
        }
    }
}
