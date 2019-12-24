using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC._24
{
    public sealed class Solution : ISolution
    {
        public sealed class Map : IEquatable<Map>
        {
            private readonly bool[,] _tiles;
            private readonly int _width;
            private readonly int _height;

            public Map(bool[,] tiles)
            {
                _tiles = tiles;
                _width = _tiles.GetLength(0);
                _height = _tiles.GetLength(1);
            }

            public long Biodiversity
            {
                get
                {
                    var total = 0;
                    var exponent = 1;
                    for (var y = 0; y < _height; y++)
                    {
                        for (var x = 0; x < _width; x++)
                        {
                            if (_tiles[x, y])
                            {
                                total += exponent;
                            }

                            exponent *= 2;
                        }
                    }

                    return total;
                }
            }

            public Map Next()
            {
                var arr = new bool[_width, _height];

                for (var y = 0; y < _height; y++)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var neighbours = GetNeighbours(x, y);
                        var neighbourBugsCount = neighbours.Count(n => _tiles[n.X, n.Y]);
                        if (_tiles[x, y])
                        {
                            arr[x, y] = neighbourBugsCount == 1;
                        }
                        else
                        {
                            arr[x, y] = neighbourBugsCount == 1 || neighbourBugsCount == 2;
                        }
                    }
                }

                return new Map(arr);
            }

            public List<(int X, int Y)> GetNeighbours(int x, int y)
            {
                var neighbours = new[]
                {
                    (X: x - 1, Y: y),
                    (X: x + 1, Y: y),
                    (X: x, Y: y - 1),
                    (X: x, Y: y + 1),
                };
                return neighbours.Where(c => c.X >= 0 && c.X < _width && c.Y >= 0 && c.Y < _height).ToList();
            }

            public static Map Parse(string s)
            {
                var lines = s.Split("\r\n");
                var height = lines.Length;
                var width = lines[0].Length;
                var map = new bool[width, height];
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var c = lines[y][x];
                        map[x, y] = c == '#';
                    }
                }

                return new Map(map);
            }

            public bool Equals(Map other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return other.ToString() == ToString();
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj is Map other && Equals(other);
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override string ToString()
            {
                var result = new StringBuilder();
                for (var y = 0; y < _height; y++)
                {
                    var line = new StringBuilder();
                    for (var x = 0; x < _width; x++)
                    {
                        line.Append(_tiles[x, y] ? '#' : '.');
                    }

                    result.AppendLine(line.ToString());
                }

                return result.ToString();
            }
        }


        public sealed class RecursiveMap
        {
            private readonly bool[,,] _tiles;
            private readonly int _width;
            private readonly int _height;
            private readonly int _depth;
            private readonly int _midWidth;
            private readonly int _midHeight;

            public RecursiveMap(bool[,,] tiles)
            {
                _tiles = tiles;
                _depth = tiles.GetLength(0);
                _width = tiles.GetLength(1);
                _height = tiles.GetLength(2);
                _midWidth = _width / 2;
                _midHeight = _height / 2;
            }

            public RecursiveMap Next()
            {
                var tiles = new bool[_depth + 2, _width, _height];

                for (var l = -1; l < _depth + 1; l++)
                {
                    for (var y = 0; y < _height; y++)
                    {
                        for (var x = 0; x < _width; x++)
                        {
                            if (x == _midWidth && y == _midHeight)
                            {
                                tiles[l + 1, x, y] = false;
                                continue;
                            }
                            var neighbours = GetNeighbours(l, x, y);
                            var neighbourBugsCount = neighbours.Count(n => ContainsBug(n.Layer, n.X, n.Y));
                            if (ContainsBug(l, x, y))
                            {
                                tiles[l+1, x, y] = neighbourBugsCount == 1;
                            }
                            else
                            {
                                tiles[l + 1, x, y] = neighbourBugsCount == 1 || neighbourBugsCount == 2;
                            }
                        }
                    }
                }

                return new RecursiveMap(tiles);
            }

            public long CountBugs()
            {
                var count = 0;
                for (var l = 0; l < _depth; l++)
                {
                    for (var y = 0; y < _height; y++)
                    {
                        for (var x = 0; x < _width; x++)
                        {
                            if (ContainsBug(l, x, y))
                            {
                                count++;
                            }
                        }
                    }
                }

                return count;
            }

            private bool ContainsBug(int l, int x, int y)
            {
                if (l >= 0 && l < _depth)
                {
                    return _tiles[l, x, y];
                }
                return false;
            }

            public List<(int Layer, int X, int Y)> GetNeighbours(int layer, int x, int y)
            {
                var neighbours = new List<(int Layer, int X, int Y)>();

                var neighboursInLayer = new[]
                {
                    (X: x - 1, Y: y),
                    (X: x + 1, Y: y),
                    (X: x, Y: y - 1),
                    (X: x, Y: y + 1),
                }.Where(c => c.X >= 0 && c.X < _width && c.Y >= 0 && c.Y < _height && !c.Equals((_midWidth, _midHeight))).ToList();
                neighbours.AddRange(neighboursInLayer.Select(n => (layer, n.X, n.Y)));

                if (x == 0)
                {
                    neighbours.Add((layer-1, _midWidth - 1, _midHeight));
                }

                if (x == _width - 1)
                {
                    neighbours.Add((layer - 1, _midWidth + 1, _midHeight));
                }

                if (y == 0)
                {
                    neighbours.Add((layer - 1, _midWidth, _midHeight - 1));
                }

                if (y == _height - 1)
                {
                    neighbours.Add((layer - 1, _midWidth, _midHeight + 1));
                }

                if (x == _midWidth - 1 && y == _midHeight)
                {
                    neighbours.AddRange(Enumerable.Range(0, _height).Select(j => (layer + 1, 0, j)));
                }

                if (x == _midWidth + 1 && y == _midHeight)
                {
                    neighbours.AddRange(Enumerable.Range(0, _height).Select(j => (layer + 1, _width - 1, j)));
                }

                if (y == _midHeight - 1 && x == _midWidth)
                {
                    neighbours.AddRange(Enumerable.Range(0, _width).Select(j => (layer + 1, j, 0)));
                }

                if (y == _midHeight + 1 && x == _midWidth)
                {
                    neighbours.AddRange(Enumerable.Range(0, _width).Select(j => (layer + 1, j, _height - 1)));
                }

                return neighbours;
            }

            public List<(int X, int Y)> GetNeighbours(int x, int y)
            {
                var neighbours = new[]
                {
                    (X: x - 1, Y: y),
                    (X: x + 1, Y: y),
                    (X: x, Y: y - 1),
                    (X: x, Y: y + 1),
                };
                return neighbours.Where(c => c.X >= 0 && c.X < _width && c.Y >= 0 && c.Y < _height).ToList();
            }

            public static RecursiveMap Parse(string s)
            {
                var lines = s.Split("\r\n");
                var height = lines.Length;
                var width = lines[0].Length;
                var map = new bool[1, width, height];
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var c = lines[y][x];
                        map[0, x, y] = c == '#';
                    }
                }

                return new RecursiveMap(map);
            }

            public override string ToString()
            {
                var midDepth = _depth / 2;
                var result = new StringBuilder();
                for (var l = 0; l < _depth; l++)
                {
                    result.AppendLine($"Depth {l - midDepth}");
                    for (var y = 0; y < _height; y++)
                    {
                        var line = new StringBuilder();
                        for (var x = 0; x < _width; x++)
                        {
                            line.Append(x == _midWidth && y == _midHeight ? '?' : (ContainsBug(l, x, y) ? '#' : '.'));
                        }

                        result.AppendLine(line.ToString());
                    }

                    result.AppendLine();
                }

                return result.ToString();
            }
        }

        private async Task<string> ParseInput()
        {
            return await File.ReadAllTextAsync("24/input");
        }

        private async Task<long> Part1()
        {
            var map = Map.Parse(await ParseInput());

            var seen = new HashSet<Map>() {map};
            while (true)
            {
                map = map.Next();
                if (seen.Contains(map))
                {
                    return map.Biodiversity;
                }

                seen.Add(map);
                
            }
        }

        private async Task<long> Part2()
        {
            var map = RecursiveMap.Parse(await ParseInput());

            for (var i = 0; i < 200; i++)
            {
                map = map.Next();
            }
            
            return map.CountBugs();
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
