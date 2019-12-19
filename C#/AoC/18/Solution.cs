using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AoC._18
{
    public sealed class Solution : ISolution
    {
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
                case 1: return new List<Move> { Move.Right };
                case 2: return new List<Move> { Move.Right, Move.Right };
                case 3: return new List<Move> { Move.Left };
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
            public HashSet<(int X, int Y)> Walls { get; }

            public Dictionary<char, (int X, int Y)> Keys { get; }

            public Dictionary<char, (int X, int Y)> Doors { get; }

            public (int X, int Y) Entrance { get; }

            public Map(HashSet<(int X, int Y)> walls, Dictionary<char, (int X, int Y)> keys, Dictionary<char, (int X, int Y)> doors, (int X, int Y) entrance)
            {
                Walls = walls;
                Keys = keys;
                Doors = doors;
                Entrance = entrance;
            }


            public (List<Direction> Path, List<char> Doors) GetPath((int X, int Y) start, (int X, int Y) end)
            {
                var explored = new HashSet<(int X, int Y)> { start };
                var toProcess = new HashSet<(int X, int Y)> { start };
                var paths = new Dictionary<(int X, int Y), (List<Direction> Path, List<char> Doors)> { { start, (new List<Direction>(), new List<char>()) } };

                while (!explored.Contains(end))
                {
                    var toProcessNext = new HashSet<(int X, int Y)>();
                    foreach (var coord in toProcess)
                    {
                        foreach (var direction in Directions)
                        {
                            var next = GetNext(coord, direction);
                            if (!explored.Contains(next))
                            {
                                if (!Walls.Contains(next))
                                {
                                    toProcessNext.Add(next);
                                    explored.Add(next);
                                    paths[next] = (paths[coord].Path.Concat(new[] { direction }).ToList(), paths[coord].Doors.Concat(Doors.Where(kvp => kvp.Value.Equals(next)).Select(kvp => kvp.Key)).ToList());
                                }
                            }
                        }
                    }

                    toProcess = toProcessNext;
                }

                return paths[end];
            }

            public static Map Parse(string s)
            {
                var lines = s.Split("\n").Where(l => !string.IsNullOrEmpty(l)).ToList();
                var walls = new HashSet<(int X, int Y)>();
                var keys = new Dictionary<char, (int X, int Y)>();
                var doors = new Dictionary<char, (int X, int Y)>();
                (int X, int Y) entrance = (-1, -1);

                for (var y = 0; y < lines.Count; y++)
                {
                    var line = lines[y];
                    for (var x = 0; x < line.Length; x++)
                    {
                        var c = line[x];
                        if (c == '#')
                        {
                            walls.Add((x, y));
                        }
                        else if (c == '@')
                        {
                            entrance = (x, y);
                        }
                        else if (char.IsLetter(c) && char.IsLower(c))
                        {
                            keys.Add(c.ToString().ToUpperInvariant()[0], (x, y));
                        }
                        else if (char.IsLetter(c) && char.IsUpper(c))
                        {
                            doors.Add(c, (x, y));
                        }
                    }
                }

                return new Map(walls, keys, doors, entrance);
            }

            public override string ToString()
            {
                var (x1, x2, y1, y2) = (Walls.Min(w => w.X), Walls.Max(w => w.X), Walls.Min(w => w.Y),
                                        Walls.Max(w => w.Y));
                var result = new StringBuilder();
                for (var y = y1; y <= y2; y++)
                {
                    var line = new StringBuilder();
                    for (var x = x1; x <= x2; x++)
                    {
                        if (Entrance.Equals((x, y)))
                        {
                            line.Append('@');
                        }
                        else if (Keys.Any(k => k.Value.Equals((x, y))))
                        {
                            line.Append(Keys.First(k => k.Value.Equals((x, y))).Key.ToString().ToLowerInvariant());
                        }
                        else if (Doors.Any(k => k.Value.Equals((x, y))))
                        {
                            line.Append(Doors.First(k => k.Value.Equals((x, y))).Key.ToString().ToUpperInvariant());
                        }
                        else if (Walls.Contains((x, y)))
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

                return result.ToString();
            }
        }

        private async Task<Map> ParseInput()
        {
            return Map.Parse(await File.ReadAllTextAsync("18/input"));
        }
        
        private async Task<long> Part1()
        {
            var map = await ParseInput();

            var letters = map.Keys.Keys.Append('@').ToList();
            var points = map.Keys.Append(new KeyValuePair<char, (int X, int Y)>('@', map.Entrance))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var pathLengths = new Dictionary<(char Start, char End), int>();
            var pathRequirements = new Dictionary<(char Start, char End), List<char>>();

            foreach (var l1 in letters)
            {
                foreach (var l2 in letters.Except(new[] {l1}))
                {
                    var p1 = points[l1];
                    var p2 = points[l2];
                    var p = map.GetPath(p1, p2);
                    pathLengths[(l1, l2)] = p.Path.Count;
                    pathRequirements[(l1, l2)] = p.Doors;
                }
            }

            var toProcess = new Dictionary<int, Queue<string>> { { 0, new Queue<string>() } };
            toProcess[0].Enqueue("@");
            var visited = new HashSet<string>();

            while (true)
            {
                var nextDistance = toProcess.Keys.Min();
                var next = toProcess[nextDistance].Dequeue();
                if (!toProcess[nextDistance].Any())
                {
                    toProcess.Remove(nextDistance);
                }

                if (next.Length == letters.Count)
                {
                    return nextDistance;
                }

                var key = next.Last() + string.Join("", next.Skip(1).OrderBy(c => c));
                if (visited.Contains(key))
                {
                    continue;
                }

                visited.Add(key);

                var options = letters.Where(l => !next.Contains(l) &&
                                                 pathRequirements[(next.Last(), l)]
                                                     .All(k => next.Contains(k)));

                foreach (var option in options)
                {
                    var distance = nextDistance + pathLengths[(next.Last(), option)];
                    var path = next + option;

                    if (!toProcess.ContainsKey(distance))
                    {
                        toProcess[distance] = new Queue<string>();
                    }
                    toProcess[distance].Enqueue(path);
                }

            }


        }

        private async Task<long> Part2()
        {
            var map = await ParseInput();
            var entrance = map.Entrance;

            var map1 = new Map(
                new HashSet<(int X, int Y)>(map.Walls.Where(p => p.X <= entrance.X && p.Y <= entrance.Y).Concat(new [] { entrance, (entrance.X - 1, entrance.Y), (entrance.X, entrance.Y - 1) })),
                map.Keys.Where(kvp => kvp.Value.X <= entrance.X && kvp.Value.Y <= entrance.Y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                map.Doors.Where(kvp => kvp.Value.X <= entrance.X && kvp.Value.Y <= entrance.Y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                (entrance.X - 1, entrance.Y - 1));

            var map2 = new Map(
                new HashSet<(int X, int Y)>(map.Walls.Where(p => p.X >= entrance.X && p.Y <= entrance.Y).Concat(new[] { entrance, (entrance.X + 1, entrance.Y), (entrance.X, entrance.Y - 1) })),
                map.Keys.Where(kvp => kvp.Value.X >= entrance.X && kvp.Value.Y <= entrance.Y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                map.Doors.Where(kvp => kvp.Value.X >= entrance.X && kvp.Value.Y <= entrance.Y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                (entrance.X + 1, entrance.Y - 1));

            var map3 = new Map(
                new HashSet<(int X, int Y)>(map.Walls.Where(p => p.X <= entrance.X && p.Y >= entrance.Y).Concat(new[] { entrance, (entrance.X - 1, entrance.Y), (entrance.X, entrance.Y + 1) })),
                map.Keys.Where(kvp => kvp.Value.X <= entrance.X && kvp.Value.Y >= entrance.Y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                map.Doors.Where(kvp => kvp.Value.X <= entrance.X && kvp.Value.Y >= entrance.Y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                (entrance.X - 1, entrance.Y + 1));

            var map4 = new Map(
                new HashSet<(int X, int Y)>(map.Walls.Where(p => p.X >= entrance.X && p.Y >= entrance.Y).Concat(new[] { entrance, (entrance.X + 1, entrance.Y), (entrance.X, entrance.Y + 1) })),
                map.Keys.Where(kvp => kvp.Value.X >= entrance.X && kvp.Value.Y >= entrance.Y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                map.Doors.Where(kvp => kvp.Value.X >= entrance.X && kvp.Value.Y >= entrance.Y).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                (entrance.X + 1, entrance.Y + 1));

            var map1Keys = map1.Keys.Keys.Append('1').ToList();
            var map2Keys = map2.Keys.Keys.Append('2').ToList();
            var map3Keys = map3.Keys.Keys.Append('3').ToList();
            var map4Keys = map4.Keys.Keys.Append('4').ToList();
            var letters = map1Keys.Concat(map2Keys).Concat(map3Keys).Concat(map4Keys).ToList();
            var points = map1.Keys.Concat(map2.Keys).Concat(map3.Keys).Concat(map4.Keys)
                             .Concat(new Dictionary<char, (int X, int Y)>
                             {
                                 {'1', map1.Entrance}, {'2', map2.Entrance}, {'3', map3.Entrance}, {'4', map4.Entrance}
                             }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            var pathLengths = new Dictionary<(char Start, char End), int>();
            var pathRequirements = new Dictionary<(char Start, char End), List<char>>();

            foreach (var l1 in map1Keys)
            {
                foreach (var l2 in map1Keys.Except(new[] {l1}))
                {
                    var p1 = points[l1];
                    var p2 = points[l2];
                    var p = map1.GetPath(p1, p2);
                    pathLengths[(l1, l2)] = p.Path.Count;
                    pathRequirements[(l1, l2)] = p.Doors;
                }
            }
            foreach (var l1 in map2Keys)
            {
                foreach (var l2 in map2Keys.Except(new[] { l1 }))
                {
                    var p1 = points[l1];
                    var p2 = points[l2];
                    var p = map2.GetPath(p1, p2);
                    pathLengths[(l1, l2)] = p.Path.Count;
                    pathRequirements[(l1, l2)] = p.Doors;
                }
            }
            foreach (var l1 in map3Keys)
            {
                foreach (var l2 in map3Keys.Except(new[] { l1 }))
                {
                    var p1 = points[l1];
                    var p2 = points[l2];
                    var p = map3.GetPath(p1, p2);
                    pathLengths[(l1, l2)] = p.Path.Count;
                    pathRequirements[(l1, l2)] = p.Doors;
                }
            }
            foreach (var l1 in map4Keys)
            {
                foreach (var l2 in map4Keys.Except(new[] { l1 }))
                {
                    var p1 = points[l1];
                    var p2 = points[l2];
                    var p = map4.GetPath(p1, p2);
                    pathLengths[(l1, l2)] = p.Path.Count;
                    pathRequirements[(l1, l2)] = p.Doors;
                }
            }


            var toProcess = new Dictionary<int, Queue<(string Path1, string Path2, string Path3, string Path4)>> { { 0, new Queue<(string Path1, string Path2, string Path3, string Path4)>() } };
            toProcess[0].Enqueue(("1", "2", "3", "4"));
            var visited = new HashSet<string>();

            while (true)
            {
                var nextDistance = toProcess.Keys.Min();
                var next = toProcess[nextDistance].Dequeue();
                if (!toProcess[nextDistance].Any())
                {
                    toProcess.Remove(nextDistance);
                }

                if (next.Path1.Length + next.Path2.Length + next.Path3.Length + next.Path4.Length == letters.Count)
                {
                    return nextDistance;
                }

                var (c1, c2, c3, c4) = (next.Path1.Last(), next.Path2.Last(), next.Path3.Last(), next.Path4.Last());

                var keysCollected = string.Join("", next.Path1.ToList().Concat(next.Path2).Concat(next.Path3).Concat(next.Path4)
                                        .Except(new[] {'1', '2', '3', '4'}).OrderBy(c => c));

                var key = "" + c1 + c2 + c3 + c4 +
                          string.Join("", next.Path1.ToList().Concat(next.Path2).Concat(next.Path3).Concat(next.Path4).OrderBy(c => c));
                if (visited.Contains(key))
                {
                    continue;
                }

                visited.Add(key);

                var options1 = map1.Keys.Keys.Except(keysCollected)
                                   .Where(k => pathRequirements[(c1, k)].All(r => keysCollected.Contains(r)));
                var options2 = map2.Keys.Keys.Except(keysCollected)
                                   .Where(k => pathRequirements[(c2, k)].All(r => keysCollected.Contains(r)));
                var options3 = map3.Keys.Keys.Except(keysCollected)
                                   .Where(k => pathRequirements[(c3, k)].All(r => keysCollected.Contains(r)));
                var options4 = map4.Keys.Keys.Except(keysCollected)
                                   .Where(k => pathRequirements[(c4, k)].All(r => keysCollected.Contains(r)));

                foreach (var option in options1)
                {
                    var distance = nextDistance + pathLengths[(c1, option)];
                    var path = next.Path1 + option;

                    if (!toProcess.ContainsKey(distance))
                    {
                        toProcess[distance] = new Queue<(string Path1, string Path2, string Path3, string Path4)>();
                    }
                    toProcess[distance].Enqueue((path, next.Path2, next.Path3, next.Path4));
                }

                foreach (var option in options2)
                {
                    var distance = nextDistance + pathLengths[(c2, option)];
                    var path = next.Path2 + option;

                    if (!toProcess.ContainsKey(distance))
                    {
                        toProcess[distance] = new Queue<(string Path1, string Path2, string Path3, string Path4)>();
                    }
                    toProcess[distance].Enqueue((next.Path1, path, next.Path3, next.Path4));
                }

                foreach (var option in options3)
                {
                    var distance = nextDistance + pathLengths[(c3, option)];
                    var path = next.Path3 + option;

                    if (!toProcess.ContainsKey(distance))
                    {
                        toProcess[distance] = new Queue<(string Path1, string Path2, string Path3, string Path4)>();
                    }
                    toProcess[distance].Enqueue((next.Path1, next.Path2, path, next.Path4));
                }

                foreach (var option in options4)
                {
                    var distance = nextDistance + pathLengths[(c4, option)];
                    var path = next.Path4 + option;

                    if (!toProcess.ContainsKey(distance))
                    {
                        toProcess[distance] = new Queue<(string Path1, string Path2, string Path3, string Path4)>();
                    }
                    toProcess[distance].Enqueue((next.Path1, next.Path2, next.Path3, path));
                }

            }
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
