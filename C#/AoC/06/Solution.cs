using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._06
{
    internal sealed class Solution : ISolution
    {
        public sealed class Graph
        {
            private readonly IReadOnlyCollection<string> _nodes;
            private readonly IReadOnlyDictionary<string, string> _edges;

            public Graph(IReadOnlyCollection<string> nodes, IReadOnlyCollection<(string From, string To)> edges)
            {
                _nodes = nodes;
                _edges = edges.ToDictionary(e => e.From, e => e.To);
            }

            public int GetDirectEdgesCount() => _edges.Count;

            public int GetIndirectEdgesCount()
            {
                Dictionary<string, int> orders = new Dictionary<string, int>();
                var com = _nodes.Single(n => !_edges.ContainsKey(n));
                orders[com] = 0;
                var current = new List<string> {com};
                var count = 0;
                while (orders.Count != _nodes.Count)
                {
                    var nodes = _nodes.Where(n => _edges.ContainsKey(n) && current.Contains(_edges[n])).ToList();
                    foreach (var node in nodes)
                    {
                        orders[node] = count + 1;
                    }

                    count++;
                    current = nodes;
                }

                return orders.Sum(o => Math.Max(0, o.Value - 1));
            }

            public static Graph Parse(IEnumerable<string> inputs)
            {
                var edges = inputs.Select(i =>
                {
                    var split = i.Split(")");
                    return (From: split[1], To: split[0]);
                }).ToList();
                var nodes = new HashSet<string>(edges.SelectMany(e => new[] {e.From, e.To})).ToList();
                return new Graph(nodes, edges);
            }
            
            public int GetPathLength(string from, string to)
            {
                var fromToCom = new List<string> {from};
                var current = from;
                while (_edges.ContainsKey(current))
                {
                    current = _edges[current];
                    fromToCom.Add(current);
                }

                current = to;
                var count = 0;
                while (_edges.ContainsKey(current))
                {
                    current = _edges[current];
                    count++;
                    if (fromToCom.Contains(current))
                    {
                        return fromToCom.IndexOf(current) + count - 2;
                    }
                }

                return -1;
            }
        }


        private async Task<Graph> ParseInput()
        {
            return Graph.Parse(await File.ReadAllLinesAsync("06/input"));
        }
        public async Task<int> Part1()
        {
            var graph = await ParseInput();
            return graph.GetDirectEdgesCount() + graph.GetIndirectEdgesCount();
        }

        public async Task<int> Part2()
        {
            var graph = await ParseInput();
            return graph.GetPathLength("YOU", "SAN");
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
