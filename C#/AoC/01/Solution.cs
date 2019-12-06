using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._01
{
    public sealed class Solution : ISolution
    {
        public async Task Run()
        {
            var part1 = await Part1();
            Console.WriteLine($"Part 1: {part1}");

            var part2 = await Part2();
            Console.WriteLine($"Part 2: {part2}");
        }

        private sealed class Module
        {
            public int Mass { get; }

            public Module(int mass)
            {
                Mass = mass;
            }
        }
        
        private async Task<int> Part1()
        {
            var modules = await ParseInput();
            return modules.Select(GetFuelMass).Sum();
        }

        private async Task<int> Part2()
        {
            var modules = await ParseInput();
            return modules.Select(GetTotalFuelMass).Sum();
        }

        private int GetFuelMass(Module module)
        {
            return GetFuelMass(module.Mass);
        }

        private int GetFuelMass(int mass)
        {
            return Math.Max((int)Math.Floor(((float)mass / 3)) - 2, 0);
        }

        private int GetTotalFuelMass(Module module)
        {
            var total = 0;
            var fuel = GetFuelMass(module);
            while (fuel != 0)
            {
                total += fuel;
                fuel = GetFuelMass(fuel);
            }

            return total;
        }

        private async Task<List<Module>> ParseInput()
        {
            return (await File.ReadAllTextAsync("01/input"))
                   .Split('\n')
                   .Select(int.Parse)
                   .Select(mass => new Module(mass))
                   .ToList();
        }
    }
}
