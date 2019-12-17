using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._17
{
    public sealed class Solution : ISolution
    {
        public enum TileType
        {
            Empty,
            Scaffolding
        }

        public enum Direction
        {
            North,
            East,
            South,
            West
        }

        public static IReadOnlyCollection<Direction> Directions => new[]
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        private static (int X, int Y) GetNext((int X, int Y) coord, Direction direction)
        {
            var (x, y) = coord;
            switch (direction)
            {
                case Direction.North: return (x, y - 1);
                case Direction.South: return (x, y + 1);
                case Direction.West: return (x - 1, y);
                case Direction.East: return (x + 1, y);
                default: throw new InvalidOperationException($"Invalid {nameof(Direction)}: {direction}");
            }
        }

        private static IEnumerable<(int X, int Y)> GetNeighbours((int X, int Y) coord)
        {
            return Directions.Select(d => GetNext(coord, d));
        }

        public sealed class Map
        {
            private readonly IDictionary<(int X, int Y), TileType> _tiles;
            private readonly ((int X, int Y) Position, Direction Direction) _robot;
            public Map(string image)
            {
                var split = image.Split("\n").Where(s => s != string.Empty).ToList();
                var (width, height) = (split.First().Length, split.Count);
                var tiles = new Dictionary<(int X, int Y), TileType>();
                for (var i = 0; i < width; i++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        var c = split[j][i];
                        switch (c)
                        {
                            case '#':
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            case '^':
                                _robot = ((i, j), Direction.North);
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            case '<':
                                _robot = ((i, j), Direction.West);
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            case '>':
                                _robot = ((i, j), Direction.East);
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            case 'v':
                                _robot = ((i, j), Direction.South);
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            default:
                                tiles[(i, j)] = TileType.Empty;
                                break;
                        }
                    }
                }

                _tiles = tiles;
            }

            public TileType GetTileType((int X, int Y) coord)
            {
                return _tiles.TryGetValue(coord, out TileType t) ? t : TileType.Empty;
            }

            public bool IsIntersection((int X, int Y) coord)
            {
                return GetNeighbours(coord).Concat(new [] { coord }).All(c => GetTileType(c) == TileType.Scaffolding);
            }
            
            public HashSet<(int X, int Y)> GetIntersections()
            {
                var (x1, x2, y1, y2) = (_tiles.Keys.Min(k => k.X), _tiles.Keys.Max(k => k.X), _tiles.Keys.Min(k => k.Y),
                                        _tiles.Keys.Max(k => k.Y));
                var hashSet = new HashSet<(int X, int Y)>();
                for (var x = x1; x <= x2; x++)
                {
                    for (var y = y1; y <= y2; y++)
                    {
                        if (IsIntersection((x, y)))
                        {
                            hashSet.Add((x, y));
                        }
                    }
                }

                return hashSet;
            }

            public override string ToString()
            {
                return ToString(false);
            }

            public string ToString(bool showIntersections)
            {
                var result = new StringBuilder();
                var (x1, x2, y1, y2) = (_tiles.Keys.Min(k => k.X), _tiles.Keys.Max(k => k.X), _tiles.Keys.Min(k => k.Y),
                                        _tiles.Keys.Max(k => k.Y));
                for (var y = y1; y <= y2; y++)
                {
                    var line = new StringBuilder();
                    for (var x = x1; x <= x2; x++)
                    {
                        var tile = GetTileType((x, y));
                        if (_robot.Position.Equals((x, y)))
                        {
                            char robotChar = 'X';
                            if (tile != TileType.Empty)
                            {
                                switch (_robot.Direction)
                                {
                                    case Direction.North:
                                        robotChar = '^';
                                        break;
                                    case Direction.East:
                                        robotChar = '>';
                                        break;
                                    case Direction.South:
                                        robotChar = 'v';
                                        break;
                                    case Direction.West:
                                        robotChar = '<';
                                        break;
                                }
                            }

                            line.Append(robotChar);
                        }
                        else
                        {
                            line.Append(tile == TileType.Scaffolding ? (showIntersections && IsIntersection((x, y)) ? 'O' : '#') : '.');
                        }
                    }

                    result.AppendLine(line.ToString());
                }

                return result.ToString();
            }
        }

        private async Task<int> Part1()
        {
            var program = await IntCode.Program.Load("17/input");
            var computer = new IntCode.Computer(program);
            computer.Start();
            await computer.WaitUntilCompleted();
            var output = computer.Output.ToList();

            var image = string.Join("", output.Select(i => (char)i));
            //var image =
            //    "..#..........\n..#..........\n#######...###\n#.#...#...#.#\n#############\n..#...#...#..\n..#####...^..";
            var map = new Map(image);
            var intersections = map.GetIntersections();
            return intersections.Sum(p => p.X * p.Y);
        }

        private async Task<long> Part2()
        {
            var program = await IntCode.Program.Load("17/input");
            program.Set(0, 2);
            var computer = new IntCode.Computer(program);
            computer.Start();

            var instructionString = "A,B,B,A,C,A,C,A,C,B\nL,6,R,12,R,8\nR,8,R,12,L,12\nR,12,L,12,L,4,L,4\nn\n";
            var instructions = instructionString.Select(c => (long)c).ToList();

            foreach (var instruction in instructions)
            {
                await computer.Input.Write(instruction);
            }

            // var output = new List<long>();

            //var t = Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        var str = "";
            //        while (true)
            //        {
            //            var next = await computer.Output.Read();
            //            output.Add(next);
            //            if (next == (long)'\n' && str.LastOrDefault() == '\n')
            //            {
            //                break;
            //            }

            //            str += (char)next;
            //        }

            //        Console.WriteLine(str);
            //    }
            //});

            //await computer.WaitUntilCompleted();
            //await Task.Delay(1000);
            //return output.Last();

            await computer.WaitUntilCompleted();

            var output = computer.Output.ToList();

            return output.Last();
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
