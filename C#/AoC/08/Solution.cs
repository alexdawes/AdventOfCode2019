using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._08
{
    internal sealed class Solution : ISolution
    {
        public sealed class Layer
        {
            public List<List<int>> Rows { get; }

            public Layer(List<List<int>> rows)
            {
                Rows = rows;
            }

            public int Count(int value)
            {
                return Rows.SelectMany(i => i).Count(i => i == value);
            }

            public int Get(int i, int j)
            {
                return Rows[j][i];
            }

            public override string ToString()
            {
                return string.Join("\n", Rows.Select(r => string.Join("", r)));
            }
        }

        private async Task<List<Layer>> ParseInput()
        {
            var ints = (await File.ReadAllTextAsync("08/input")).Select(c => int.Parse(c.ToString())).ToList();
            var width = 25;
            var height = 6;

            var current = 0;
            var currentRow = new List<int>();
            var currentLayer = new List<List<int>>();
            var layers = new List<Layer>();
            while (current < ints.Count)
            {
                var i = ints[current];
                currentRow.Add(i);
                if (current % width == width - 1)
                {
                    currentLayer.Add(currentRow);
                    currentRow = new List<int>();
                }

                if (current % (width * height) == (width * height) - 1)
                {
                    layers.Add(new Layer(currentLayer));
                    currentLayer = new List<List<int>>();
                }

                current++;
            }

            return layers;
        }

        private async Task<int> Part1()
        {
            var layers = await ParseInput();

            var minZeroLayer = layers.OrderBy(l => l.Count(0)).First();
            return minZeroLayer.Count(1) * minZeroLayer.Count(2);
        }

        private async Task<string> Part2()
        {
            var layers = await ParseInput();

            var rows = new List<List<int>>();
            for (var j = 0; j < 6; j++)
            {
                var row = new List<int>();
                for (var i = 0; i < 25; i++)
                {
                    foreach (var layer in layers)
                    {
                        var value = layer.Get(i, j);
                        if (value == 0 || value == 1)
                        {
                            row.Add(value);
                            break;
                        }
                    }
                }
                rows.Add(row);
            }

            return string.Join("\n", rows.Select(r => string.Join("", r.Select(i => i == 1 ? 'X' : ' '))));
        }

        public async Task Run()
        {
            var p1 = await Part1();
            Console.WriteLine($"Part 1: {p1}");

            var p2 = await Part2();
            Console.WriteLine($"Part 2:\n{p2}");
        }
    }
}
