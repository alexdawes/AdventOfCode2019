using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._20
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

        public sealed class Maze
        {
            public HashSet<(int X, int Y)> Paths { get; }

            public Dictionary<(int X, int Y), (int X, int Y)> InnerPortals { get; }

            public Dictionary<(int X, int Y), (int X, int Y)> OuterPortals { get; }

            public Dictionary<(int X, int Y), (int X, int Y)> Portals =>
                InnerPortals.Concat(OuterPortals).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            public (int X, int Y) Start { get; }

            public (int X, int Y) End { get; }

            public Maze(HashSet<(int X, int Y)> paths, Dictionary<(int X, int Y), (int X, int Y)> innerPortals, Dictionary<(int X, int Y), (int X, int Y)> outerPortals,
                        (int X, int Y) start, (int X, int Y) end)
            {
                Paths = paths;
                InnerPortals = innerPortals;
                OuterPortals = outerPortals;
                Start = start;
                End = end;
            }
            
            public int GetPathLength((int X, int Y) start, (int X, int Y) end)
            {
                var count = 0;
                var toProcess = new List<(int X, int Y)> {start};
                HashSet<(int X, int Y)> visited = new HashSet<(int X, int Y)>();
                while (toProcess.Any())
                {
                    var next = new List<(int X, int Y)>();

                    foreach (var coord in toProcess)
                    {
                        if (coord.Equals(end))
                        {
                            return count;
                        }
                        
                        foreach (var n in GetMazeNeighbours(coord, false).Except(visited))
                        {
                            next.Add(n);
                            visited.Add(n);
                        }
                    }

                    count++;

                    toProcess = next;
                }

                return -1;
            }

            public async Task<int> GetPathLength1((int X, int Y) start, (int X, int Y) end)
            {
                var pathLengths = new Dictionary<(int X, int Y), Dictionary<(int X, int Y), int>>();
                var portals = Portals;
                var points = portals.Keys.Append(start).Append(end).ToList();

                var lockObj = new object();
                await Task.WhenAll(points.Select(async from =>
                {
                    await Task.WhenAll(
                        points.Except(new[] {from}).Where(t => t.X > from.X || (t.X == from.X && t.Y > from.Y)).Select(
                            async to =>
                            {
                                await Task.Yield();
                                var pathLength = GetPathLength(from, to);
                                if (pathLength != -1)
                                {
                                    lock (lockObj)
                                    {
                                        if (!pathLengths.ContainsKey(from))
                                        {
                                            pathLengths[from] = new Dictionary<(int X, int Y), int>();
                                        }

                                        pathLengths[from][to] = pathLength;
                                    }

                                    lock (lockObj)
                                    {
                                        if (!pathLengths.ContainsKey(to))
                                        {
                                            pathLengths[to] = new Dictionary<(int X, int Y), int>();
                                        }

                                        pathLengths[to][from] = pathLength;
                                    }
                                }
                            }));
                }));

                var count = 0;
                var visited = new Dictionary<(int X, int Y), int> {{start, 0}};
                while (true)
                {
                    var toProcess = visited.Where(kvp => kvp.Value == count).Select(kvp => kvp.Key).ToList();

                    foreach (var from in toProcess)
                    {
                        if (from.Equals(end))
                        {
                            return count;
                        }

                        var tos = pathLengths[from].Keys;
                        foreach (var to in tos)
                        {
                            var distance = count + pathLengths[from][to];
                            if (!visited.ContainsKey(to) || visited[to] > distance)
                            {
                                visited[to] = distance;
                            }

                            if (portals.ContainsKey(to))
                            {
                                var to2 = portals[to];
                                if (!visited.ContainsKey(to2) || visited[to2] > distance)
                                {
                                    visited[to2] = distance + 1;
                                }
                            }
                        }
                    }

                    count++;
                }
            }

            public async Task<int> GetPathLength2(((int X, int Y) Position, int Level) start, ((int X, int Y) Position, int Level) end)
            {
                var pathLengths = new Dictionary<(int X, int Y), Dictionary<(int X, int Y), int>>();
                var portals = Portals;
                var points = portals.Keys.Append(start.Position).Append(end.Position).ToList();

                var lockObj = new object();
                await Task.WhenAll(points.Select(async from =>
                {
                    await Task.WhenAll(
                        points.Except(new[] { from }).Where(t => t.X > from.X || (t.X == from.X && t.Y > from.Y)).Select(
                            async to =>
                            {
                                await Task.Yield();
                                var pathLength = GetPathLength(from, to);
                                if (pathLength != -1)
                                {
                                    lock (lockObj)
                                    {
                                        if (!pathLengths.ContainsKey(from))
                                        {
                                            pathLengths[from] = new Dictionary<(int X, int Y), int>();
                                        }

                                        pathLengths[from][to] = pathLength;
                                    }

                                    lock (lockObj)
                                    {
                                        if (!pathLengths.ContainsKey(to))
                                        {
                                            pathLengths[to] = new Dictionary<(int X, int Y), int>();
                                        }

                                        pathLengths[to][from] = pathLength;
                                    }
                                }
                            }));
                }));
                
                var count = 0;
                var visited = new Dictionary<((int X, int Y) Position, int Level), int> { { start, 0 } };
                while (true)
                {
                    var toProcess = visited.Where(kvp => kvp.Value == count).Select(kvp => kvp.Key).ToList();

                    foreach (var from in toProcess)
                    {
                        if (from.Equals(end))
                        {
                            return count;
                        }

                        var tos = pathLengths[from.Position].Keys;
                        foreach (var to in tos)
                        {
                            var distance = count + pathLengths[from.Position][to];
                            if (!visited.ContainsKey((to, from.Level)) || visited[(to, from.Level)] > distance)
                            {
                                visited[(to, from.Level)] = distance;
                            }

                            if (InnerPortals.ContainsKey(to))
                            {
                                var to2 = portals[to];
                                if (!visited.ContainsKey((to2, from.Level + 1)) || visited[(to2, from.Level + 1)] > distance)
                                {
                                    visited[(to2, from.Level + 1)] = distance + 1;
                                }
                            }

                            if (from.Level > 0 && OuterPortals.ContainsKey(to))
                            {
                                var to2 = portals[to];
                                if (!visited.ContainsKey((to2, from.Level - 1)) || visited[(to2, from.Level - 1)] > distance)
                                {
                                    visited[(to2, from.Level - 1)] = distance + 1;
                                }
                            }
                        }
                    }

                    count++;
                }
            }

            private IEnumerable<(int X, int Y)> GetMazeNeighbours((int X, int Y) position, bool allowPortals = true)
            {
                var neighbours = GetNeighbours(position).Intersect(Paths);

                if (allowPortals)
                {
                    if (InnerPortals.ContainsKey(position))
                    {
                        return neighbours.Append(InnerPortals[position]);
                    }

                    if (OuterPortals.ContainsKey(position))
                    {
                        return neighbours.Append(OuterPortals[position]);
                    }
                }

                return neighbours;
            }

            public static Maze Parse(string s)
            {
                var chars = new Dictionary<(int X, int Y), char>();
                var lines = s.Split('\n');

                for (var y = 0; y < lines.Length; y++)
                {
                    var line = lines[y];
                    for (var x = 0; x < line.Length; x++)
                    {
                        chars[(x, y)] = line[x];
                    }
                }

                var mazeBody = chars.Where(kvp => kvp.Value == '#' || kvp.Value == '.').Select(kvp => kvp.Key).ToList();
                
                var portalsDict = new Dictionary<string, List<(int X, int Y)>>();

                foreach (var position in mazeBody)
                {
                    foreach (var direction in Directions)
                    {
                        var neighbour = GetNext(position, direction);
                        if (char.IsLetter(chars[neighbour]))
                        {
                            string key;
                            if (direction == Direction.South || direction == Direction.East)
                            {
                                var first = chars[neighbour];
                                var second = chars[GetNext(neighbour, direction)];
                                key = first.ToString() + second;
                            }
                            else
                            {
                                var first = chars[neighbour];
                                var second = chars[GetNext(neighbour, direction)];
                                key = second.ToString() + first;
                            }
                            
                            if (!portalsDict.ContainsKey(key))
                            {
                                portalsDict[key] = new List<(int X, int Y)>();
                            }

                            portalsDict[key].Add(position);
                        }
                    }

                }

                var paths = new HashSet<(int X, int Y)>(chars.Keys.Where(k => chars[k] == '.'));

                var (x1, x2, y1, y2) = (mazeBody.Min(p => p.X), mazeBody.Max(p => p.X), mazeBody.Min(p => p.Y),
                                        mazeBody.Max(p => p.Y));
                var outerEdge = mazeBody.Where(p => p.X == x1 || p.X == x2 || p.Y == y1 || p.Y == y2).ToList();

                var innerPortals = new Dictionary<(int X, int Y), (int X, int Y)>();
                var outerPortals = new Dictionary<(int X, int Y), (int X, int Y)>();
                (int X, int Y) start = (-1, -1);
                (int X, int Y) end = (-1, -1);
                foreach (var kvp in portalsDict)
                {
                    if (kvp.Key == "AA")
                    {
                        start = kvp.Value.Single();
                    }
                    else if (kvp.Key == "ZZ")
                    {
                        end = kvp.Value.Single();
                    }
                    else
                    {
                        var first = kvp.Value[0];
                        var second = kvp.Value[1];

                        if (outerEdge.Contains(first))
                        {
                            outerPortals[first] = second;
                        }
                        else
                        {
                            innerPortals[first] = second;
                        }

                        if (outerEdge.Contains(second))
                        {
                            outerPortals[second] = first;
                        }
                        else
                        {
                            innerPortals[second] = first;
                        }
                    }
                }
                
                return new Maze(paths, innerPortals, outerPortals, start, end);
            }
        }

        private async Task<Maze> ParseInput()
        {
            return Maze.Parse(await File.ReadAllTextAsync("20/input"));
        }
    
        public async Task<int> Part1()
        {
            var map = await ParseInput();
            return await map.GetPathLength1(map.Start, map.End);
        }

        public async Task<int> Part2()
        {
            var map = await ParseInput();
            return await map.GetPathLength2((map.Start, 0), (map.End, 0));
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
