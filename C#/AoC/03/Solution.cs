using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._03
{
    internal sealed class Solution : ISolution
    {
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

        public struct Instruction
        {
            public Direction Direction { get; }

            public int Distance { get; }

            public Instruction(Direction direction, int distance)
            {
                Direction = direction;
                Distance = distance;
            }

            private static Direction ParseDirection(char direction)
            {
                switch (direction)
                {
                    case 'L': return Direction.Left;
                    case 'R': return Direction.Right;
                    case 'U': return Direction.Up;
                    case 'D': return Direction.Down;
                    default: throw new InvalidOperationException($"Unrecognised {nameof(Direction)}: {direction}");
                }
            }

            public static Instruction Parse(string s)
            {
                var direction = ParseDirection(s[0]);
                var distance = Convert.ToInt32(s.Substring(1));
                return new Instruction(direction, distance);
            }
        }

        private List<(int X, int Y)> GetCoordsOnPath(List<Instruction> instructions, (int X, int Y) origin)
        {
            List<(int X, int Y)> points = new List<(int X, int Y)> { origin };
            var current = origin;
            foreach (var instruction in instructions)
            {
                Func<(int X, int Y), (int X, int Y)> step = c => c;
                if (instruction.Direction == Direction.Right)
                {
                    step = c => (c.X + 1, c.Y);
                }
                else if (instruction.Direction == Direction.Left)
                {
                    step = c => (c.X - 1, c.Y);
                }
                else if (instruction.Direction == Direction.Up)
                {
                    step = c => (c.X, c.Y + 1);
                }
                else if (instruction.Direction == Direction.Down)
                {
                    step = c => (c.X, c.Y - 1);
                }

                for (var i = 0; i < instruction.Distance; i++)
                {
                    var next = step(current);
                    points.Add(next);
                    current = next;
                }
            }

            return points;
        }

        private async Task<List<List<Instruction>>> ParseInput()
        {
            return (await File.ReadAllLinesAsync("03/input"))
                   .Select(line => line.Split(",").Select(Instruction.Parse).ToList()).ToList();
        }

        private async Task<int> Part1()
        {
            var instructionSets = await ParseInput();
            var paths = await Task.WhenAll(instructionSets.Select(async instructions =>
            {
                await Task.Yield();
                return GetCoordsOnPath(instructions, (0, 0));
            }));
            var intersections = paths[0].Intersect(paths[1]).Except(new [] {(X:0,Y:0)});
            return intersections.Min(p => Math.Abs(p.X) + Math.Abs(p.Y));
        }

        private async Task<int> Part2()
        {
            var instructionSets = await ParseInput();
            var paths = await Task.WhenAll(instructionSets.Select(async instructions =>
            {
                await Task.Yield();
                return GetCoordsOnPath(instructions, (0, 0));
            }));
            var intersections = paths[0].Intersect(paths[1]).Except(new[] { (X: 0, Y: 0) });
            return intersections.Min(p => paths[0].IndexOf(p) + paths[1].IndexOf(p));
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
