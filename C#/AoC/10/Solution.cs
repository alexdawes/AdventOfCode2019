using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._10
{
    public sealed class Solution : ISolution
    {
        private async Task<IReadOnlyCollection<(int X, int Y)>> ParseInput()
        {
            return (await File.ReadAllLinesAsync("10/input")).SelectMany((line, y) =>
            {
                return line.Select((c, x) => (Asteroid: c == '#', X: x))
                           .Where(t => t.Asteroid)
                           .Select(t => (X: t.X, Y: y));
            }).ToList();
        }

        private bool AreOnLine((int X, int Y) coord1, (int X, int Y) coord2, (int X, int Y) coord3)
        {
            (int x1, int y1) = coord1;
            (int x2, int y2) = coord2;
            (int x3, int y3) = coord3;
            return (x2 - x1) * (y3 - y1) == (x3 - x1) * (y2 - y1);
        }

        private int GetL1Norm((int X, int Y) coord1, (int X, int Y) coord2)
        {
            (int x1, int y1) = coord1;
            (int x2, int y2) = coord2;
            return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
        }

        private bool IsInBoundBox((int X, int Y) coord1, (int X, int Y) coord2, (int X, int Y) coord3)
        {
            (int x1, int y1) = coord1;
            (int x2, int y2) = coord2;
            (int x3, int y3) = coord3;
            return Math.Min(x1, x3) <= x2 && x2 <= Math.Max(x1, x3) && Math.Min(y1, y3) <= y2 && y2 <= Math.Max(y1, y3);
        }

        private bool AreInLine((int X, int Y) coord1, (int X, int Y) coord2, (int X, int Y) coord3)
        {
            return IsInBoundBox(coord1, coord2, coord3)
                   && GetL1Norm(coord1, coord2) < GetL1Norm(coord1, coord3)
                   && AreOnLine(coord1, coord2, coord3);
        }

        private int CountInSight(IReadOnlyCollection<(int X, int Y)> asteroids, (int X, int Y) coord)
        {
            return asteroids
                            .Except(new[] {coord})
                            .Count(a1 => !asteroids.Except(new[] {coord, a1})
                                                   .Any(a2 => AreInLine(coord, a2, a1)));
        }

        private double GetAngle((int X, int Y) coord1, (int X, int Y) coord2)
        {
            (int x1, int y1) = coord1;
            (int x2, int y2) = coord2;

            if (x1 == x2)
            {
                return y1 > y2 ? 0 : Math.PI;
            }

            if (y1 == y2)
            {
                return x2 > x1 ? Math.PI / 2 : 3 * Math.PI / 2;
            }

            if (x2 > x1 && y1 > y2)
            {
                return Math.Atan((double)(x2 - x1) / (y1 - y2));
            }

            if (x2 > x1 && y2 > y1)
            {
                return Math.PI / 2 + Math.Atan((double)(y2 - y1) / (x2 - x1));
            }

            if (x1 < x2 && y2 > y1)
            {
                return Math.PI + Math.Atan((double)(x1 - x2) / (y2 - y1));
            }

            return 3 * Math.PI / 2 + Math.Atan((double)(y1 - y2) / (x1 - x2));
        }

        private async Task<int> Part1()
        {
            var asteroids = await ParseInput();
            var counts = (await Task.WhenAll(asteroids.Select(async a =>
            {
                await Task.Yield();
                return CountInSight(asteroids, a);
            }))).Max();
            return counts;
        }

        private async Task<int> Part2()
        {
            var asteroids = await ParseInput();
            var counts = await Task.WhenAll(asteroids.Select(async a =>
            {
                await Task.Yield();
                return (Asteroid: a, Count: CountInSight(asteroids, a));
            }));
            var bestCount = counts.Max(c => c.Count);
            var station = counts.Single(c => c.Count == bestCount).Asteroid;
            
            List<List<(int X, int Y)>> groups = new List<List<(int X, int Y)>>();
            foreach (var asteroid in asteroids.Except(new[] {station}))
            {
                var matchingGroup = groups.SingleOrDefault(g =>
                {
                    var candidate = g.First();
                    return AreOnLine(station, asteroid, candidate) && !IsInBoundBox(asteroid, station, candidate);
                });
                if (matchingGroup != null)
                {
                    matchingGroup.Add(asteroid);
                }
                else
                {
                    groups.Add(new List<(int X, int Y)> { asteroid });
                }
            }

            var orderedGroups = groups.Select(g => g.OrderBy(asteroid => GetL1Norm(station, asteroid)).ToList())
                                      .OrderBy(g => GetAngle(station, g.First())).ToList();

            var destroyed = new List<(int X, int Y)>();
            var pointer = 0;
            while (orderedGroups.Any())
            {
                var group = orderedGroups[pointer];
                destroyed.Add(group[0]);
                group.RemoveAt(0);

                if (group.Any())
                {
                    pointer++;
                }
                else
                {
                    orderedGroups.RemoveAt(pointer);
                }

                if (pointer >= orderedGroups.Count)
                {
                    pointer = 0;
                }
            }

            var chosen = destroyed[199];

            return (chosen.X * 100) + chosen.Y;
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
