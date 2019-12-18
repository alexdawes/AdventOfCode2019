using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
            North = 0,
            East = 1,
            South = 2,
            West = 3
        }

        public enum Move
        {
            Forward,
            Right,
            Left,
        }

        public static IReadOnlyCollection<Direction> Directions => new[]
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        public static Direction GetOpposite(Direction direction)
        {
            return (Direction)(((int)direction + 2) % 4);
        }

        public static Direction TurnLeft(Direction direction)
        {
            return (Direction)(((int)direction + 3) % 4);
        }

        public static Direction TurnRight(Direction direction)
        {
            return (Direction)(((int)direction + 1) % 4);
        }

        public static List<Move> GetTurn(Direction from, Direction to)
        {
            var val = ((int)to - (int)from) % 4;
            if (val < 0)
            {
                val += 4;
            }
            

            switch (val)
            {
                case 0: return new List<Move>();
                case 1: return new List<Move> {Move.Right};
                case 2: return new List<Move> {Move.Right, Move.Right};
                case 3: return new List<Move> {Move.Left};
                default: throw new InvalidOperationException("Something went wrong.");
            }
        }

        public static List<string> GetInstructions(List<Move> path)
        {
            if (!path.Any())
            {
                return new List<string>();
            }

            var result = new List<string>();
            var forwardCount = 0;
            foreach (var move in path)
            {
                switch (move)
                {
                    case Move.Left:
                        if (forwardCount > 0)
                        {
                            result.Add(forwardCount.ToString());
                            forwardCount = 0;
                        }
                        result.Add("L");
                        break;
                    case Move.Right:
                        if (forwardCount > 0)
                        {
                            result.Add(forwardCount.ToString());
                            forwardCount = 0;
                        }
                        result.Add("R");
                        break;
                    case Move.Forward:
                        forwardCount++;
                        break;
                }
            }

            if (forwardCount > 0)
            {
                result.Add(forwardCount.ToString());
            }
            return result;
        }

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
            private readonly HashSet<(int X, int Y)> _scaffolds;
            public ((int X, int Y) Position, Direction Direction) Robot { get; }

            public int Width => _scaffolds.Max(p => p.X) - _scaffolds.Min(p => p.X);
            public int Height => _scaffolds.Max(p => p.Y) - _scaffolds.Min(p => p.Y);
            
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
                                Robot = ((i, j), Direction.North);
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            case '<':
                                Robot = ((i, j), Direction.West);
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            case '>':
                                Robot = ((i, j), Direction.East);
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            case 'v':
                                Robot = ((i, j), Direction.South);
                                tiles[(i, j)] = TileType.Scaffolding;
                                break;
                            default:
                                tiles[(i, j)] = TileType.Empty;
                                break;
                        }
                    }
                }

                _tiles = tiles;
                _scaffolds = new HashSet<(int X, int Y)>(_tiles.Keys.Where(t => _tiles[t] == TileType.Scaffolding));
            }
            
            public TileType GetTileType((int X, int Y) coord)
            {
                return _tiles.TryGetValue(coord, out TileType t) ? t : TileType.Empty;
            }

            public bool IsIntersection((int X, int Y) coord)
            {
                return GetTileType(coord) == TileType.Scaffolding && GetNeighbours(coord).Count(c => GetTileType(c) == TileType.Scaffolding) >= 3;
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

            public HashSet<(int X, int Y)> GetEndpoints()
            {
                return new HashSet<(int X, int Y)>(_tiles.Keys.Where(t => GetTileType(t) == TileType.Scaffolding &&
                                                                          GetNeighbours(t)
                                                                              .Count(n => GetTileType(n) ==
                                                                                          TileType.Scaffolding) == 1));
            }

            private IEnumerable<List<Move>> GeneratePaths((int X, int Y) currentPosition, Direction currentDirection, HashSet<(int X, int Y)> previousPositions, int maxIntersections)
            {
                if (previousPositions.SetEquals(_scaffolds))
                {
                    return new[] {new List<Move>()};
                }

                return new [] { currentDirection, TurnRight(currentDirection), TurnLeft(currentDirection), GetOpposite(currentDirection) }
                       .Select(d => (Direction: d, Position: GetNext(currentPosition, d)))
                       .Where(t => GetTileType(t.Position) == TileType.Scaffolding)
                       .Select(t => (Direction: t.Direction, Position: t.Position,
                                     Intersection: previousPositions.Contains(t.Position)))
                       .Where(t => (t.Intersection ? maxIntersections - 1 : maxIntersections) >= 0)
                       .SelectMany(
                           t =>
                           {
                               var moves = GetTurn(currentDirection, t.Direction).Concat(new List<Move> {Move.Forward})
                                                                                 .ToList();
                               return GeneratePaths(
                                       t.Position,
                                       t.Direction,
                                       new HashSet<(int X, int Y)>(previousPositions.Concat(new[] {t.Position})),
                                       t.Intersection ? maxIntersections - 1 : maxIntersections)
                                   .Select(p => moves.Concat(p).ToList());
                           });
            }

            public IEnumerable<List<Move>> GeneratePaths()
            {
                var previousPaths = new List<List<Move>>();
                var maxIntersections = GetIntersections().Count;
                while (true)
                {
                    var paths = GeneratePaths(Robot.Position, Robot.Direction, new HashSet<(int X, int Y)> { Robot.Position },
                                              maxIntersections);
                    foreach (var p in paths)
                    {
                        if (!previousPaths.Any(p2 => p2.SequenceEqual(p)))
                        {
                            previousPaths.Add(p);
                            yield return p;
                        }
                    }

                    maxIntersections++;
                }
            }

            public List<Move> GetSimplePath()
            {
                var path = new List<Move>();
                var position = Robot.Position;
                var direction = Directions.First(d => GetTileType(GetNext(position, d)) == TileType.Scaffolding);
                var turn = GetTurn(Robot.Direction, direction);
                path.AddRange(turn);
                while (true)
                {
                    var next = GetNext(position, direction);
                    if (GetTileType(next) == TileType.Scaffolding)
                    {
                        path.Add(Move.Forward);
                        position = next;
                        continue;
                    }

                    var right = TurnRight(direction);
                    next = GetNext(position, right);
                    if (GetTileType(next) == TileType.Scaffolding)
                    {
                        path.Add(Move.Right);
                        path.Add(Move.Forward);
                        position = next;
                        direction = right;
                        continue;
                    }

                    var left = TurnLeft(direction);
                    next = GetNext(position, left);
                    if (GetTileType(next) == TileType.Scaffolding)
                    {
                        path.Add(Move.Left);
                        path.Add(Move.Forward);
                        position = next;
                        direction = left;
                        continue;
                    }

                    return path;
                }
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
                        if (Robot.Position.Equals((x, y)))
                        {
                            char robotChar = 'X';
                            if (tile != TileType.Empty)
                            {
                                switch (Robot.Direction)
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

        private bool TestPath(List<Move> path, List<Move> a, List<Move> b, List<Move> c, out List<string> sequence)
        {
            if (!path.Any())
            {
                sequence = new List<string>();
                return true;
            }
            
            if (a.Any() && path.Take(a.Count).SequenceEqual(a) && TestPath(path.Skip(a.Count).ToList(), a, b, c, out var seqA) && seqA.Count < 10)
            {
                sequence = new List<string> {"A"}.Concat(seqA).ToList();
                return true;
            }

            if (b.Any() && path.Take(b.Count).SequenceEqual(b) && TestPath(path.Skip(b.Count).ToList(), a, b, c, out var seqB) && seqB.Count < 10)
            {
                sequence = new List<string> { "B" }.Concat(seqB).ToList();
                return true;
            }

            if (c.Any() && path.Take(c.Count).SequenceEqual(c) && TestPath(path.Skip(c.Count).ToList(), a, b, c, out var seqC) && seqC.Count < 10)
            {
                sequence = new List<string> { "C" }.Concat(seqC).ToList();
                return true;
            }

            sequence = null;
            return false;
        }

        private bool TestPath(List<string> path, List<string> a, List<string> b, List<string> c, out List<string> sequence)
        {
            if (!path.Any())
            {
                sequence = new List<string>();
                return true;
            }

            if (a.Any() && path.Take(a.Count).SequenceEqual(a) && TestPath(path.Skip(a.Count).ToList(), a, b, c, out var seqA) && seqA.Count < 10)
            {
                sequence = new List<string> { "A" }.Concat(seqA).ToList();
                return true;
            }

            if (b.Any() && path.Take(b.Count).SequenceEqual(b) && TestPath(path.Skip(b.Count).ToList(), a, b, c, out var seqB) && seqB.Count < 10)
            {
                sequence = new List<string> { "B" }.Concat(seqB).ToList();
                return true;
            }

            if (c.Any() && path.Take(c.Count).SequenceEqual(c) && TestPath(path.Skip(c.Count).ToList(), a, b, c, out var seqC) && seqC.Count < 10)
            {
                sequence = new List<string> { "C" }.Concat(seqC).ToList();
                return true;
            }

            sequence = null;
            return false;
        }

        private IEnumerable<int> GenerateOffsets(int min, int max, int step)
        {
            for (var i = min; i <= max; i += step)
            {
                yield return i;
            }
        }

        private IEnumerable<int> GenerateOffsets(int min, int max, int step1, int step2)
        {
            if (step2 == 0)
            {
                return GenerateOffsets(min, max, step1);
            }
            var result = new HashSet<int>();
            for (var i = min; i <= max; i += step1)
            {
                for (var j = i; j <= max; j += step2)
                {
                    result.Add(j);
                }
            }

            return result.ToList();
        }

        public bool TestPath2(List<Move> path,
                              out (List<string> A, List<string> B, List<string> C, List<string> Sequence) pieces)
        {
            var instructions = GetInstructions(path).ToList();

            for (var lenA = 1; lenA <= Math.Min(instructions.Count, 10); lenA++)
            {
                var a = instructions.Take(lenA).ToList();
                foreach (var offsetB in GenerateOffsets(lenA, instructions.Count, lenA))
                {
                    for (var lenB = 0; lenB <= Math.Min(instructions.Count - offsetB, 10); lenB++)
                    {
                        var b = instructions.Skip(offsetB).Take(lenB).ToList();
                        foreach (var offsetC in GenerateOffsets(offsetB + lenB, instructions.Count, lenA, lenB))
                        {
                            for (var lenC = 0; lenC <= Math.Min(path.Count - offsetC, 10); lenC++)
                            {
                                var c = instructions.Skip(offsetC).Take(lenC).ToList();
                                if (TestPath(instructions, a, b, c, out var seq))
                                {
                                    pieces = (a, b, c, seq);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            pieces = (new List<string>(), new List<string>(), new List<string>(), new List<string>());
            return false;
        }

        public bool TestPath(List<Move> path, out (List<Move> A, List<Move> B, List<Move> C, List<string> Sequence) pieces)
        {
            var instructions = new Dictionary<(int Offset, int Length), List<string>>();
            for (var len = 0; len <= path.Count; len++)
            {
                for (var offset = 0; offset <= path.Count - len; offset++)
                {
                    var inst = GetInstructions(path.Skip(offset).Take(len).ToList());
                    instructions[(offset, len)] = inst;
                }
            }

            for (var lenA = path.Count; lenA >= 1; lenA--)
            {
                var a = path.Take(lenA).ToList();
                var aInstCount = instructions[(0, lenA)].Count;
                if (aInstCount > 10)
                {
                    continue;
                }
                foreach (var offsetB in GenerateOffsets(lenA, path.Count, lenA))
                {
                    for (var lenB = path.Count - offsetB; lenB >= 0; lenB--)
                    {
                        var b = path.Skip(offsetB).Take(lenB).ToList();
                        var bInstCount = instructions[(offsetB, lenB)].Count;
                        if (bInstCount > 10)
                        {
                            continue;
                        }
                        foreach (var offsetC in GenerateOffsets(offsetB + lenB, path.Count, lenA, lenB))
                        {
                            for (var lenC = path.Count - offsetC; lenC >= 0; lenC--)
                            {
                                var c = path.Skip(offsetC).Take(lenC).ToList();
                                var cInstCount = instructions[(offsetC, lenC)].Count;
                                if (cInstCount > 10)
                                {
                                    continue;
                                }

                                if (new [] { aInstCount, bInstCount, cInstCount }.Max() <= instructions[(0, path.Count)].Count / 10)
                                {
                                    break;
                                }
                                
                                if (TestPath(path, a, b, c, out var seq))
                                {
                                    pieces = (a, b, c, seq);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            pieces = (new List<Move>(), new List<Move>(), new List<Move>(), new List<string>());
            return false;
        }

        private async Task<Map> GetMap()
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
            return map;
        }
        
        private async Task<int> Part1()
        {
            var map = await GetMap();
            var intersections = map.GetIntersections();
            return intersections.Sum(p => p.X * p.Y);
        }
        
        private async Task<long> Part2()
        {
            var map = await GetMap();
            
            var paths = map.GeneratePaths();
            foreach (var path in paths)
            {
                //if (TestPath(path, out var pieces))
                //{
                //    var a = GetInstructions(pieces.A);
                //    var b = GetInstructions(pieces.B);
                //    var c = GetInstructions(pieces.C);

                if (TestPath2(path, out var pieces))
                {
                    var a = pieces.A;
                    var b = pieces.B;
                    var c = pieces.C;
                    var seqStr = string.Join(",", pieces.Sequence);
                    var aStr = string.Join(",", a);
                    var bStr = string.Join(",", b);
                    var cStr = string.Join(",", c);
                    var inputStr = $"{seqStr}\n{aStr}\n{bStr}\n{cStr}\nn\n";
                    var input = inputStr.Select(ch => (long)ch).ToList();

                    var program = await IntCode.Program.Load("17/input");
                    program.Set(0, 2);
                    var computer = new IntCode.Computer(program);
                    computer.Start();

                    foreach (var i in input)
                    {
                        await computer.Input.Write(i);
                    }

                    await computer.WaitUntilCompleted();

                    var output = computer.Output.ToList().Last();
                    computer.Stop();
                    return output;
                }
            }

            return -1;
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
