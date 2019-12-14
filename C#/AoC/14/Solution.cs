using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC._14
{
    public sealed class Solution : ISolution
    {
        private const string FUEL = "FUEL";
        private const string ORE = "ORE";

        public sealed class Item
        {
            public string Element { get; set; }

            public long Quantity { get; set; }

            public Item(string element, long quantity)
            {
                Element = element;
                Quantity = quantity;
            }

            public static Item Parse(string s)
            {
                var match = new Regex(@"(?<quantity>\d+) (?<element>\w+)").Match(s);
                return new Item(match.Groups["element"].Value, long.Parse(match.Groups["quantity"].Value));
            }

            public override string ToString()
            {
                return $"{Quantity} {Element}";
            }
        }

        public sealed class Reaction
        {
            public IReadOnlyCollection<Item> Inputs { get; }

            public Item Output { get; }

            public Reaction(IReadOnlyCollection<Item> inputs,
                            Item output)
            {
                Inputs = inputs;
                Output = output;
            }

            public static Reaction Parse(string s)
            {
                var split = s.Split("=>");
                var output = Item.Parse(split[1].Trim());
                var inputs = split[0].Split(",").Select(ss => Item.Parse(ss.Trim())).ToList();
                return new Reaction(inputs, output);
            }

            public override string ToString()
            {
                return $"{string.Join(", ", Inputs)} => {Output}";
            }
        }

        private async Task<IReadOnlyCollection<Reaction>> ParseInput()
        {
            return (await File.ReadAllLinesAsync("14/input"))
                   .Select(Reaction.Parse)
                   .ToList();
        }

        public async Task<long> Part1()
        {
            var reactions = (await ParseInput()).ToDictionary(r => r.Output.Element);
            
            var elements = reactions.Values.SelectMany(r => r.Inputs.Select(i => i.Element).Concat(new[] {r.Output.Element}))
                                    .Distinct().ToList();
            var stock = elements.ToDictionary(e => e, e => 0L);
            var required = elements.ToDictionary(e => e, e => e == FUEL ? 1L : 0L);

            while (required.Any(kvp => kvp.Key != ORE && kvp.Value > 0))
            {
                var next = required.First(
                    kvp => kvp.Key != ORE &&
                           kvp.Value > 0 &&
                           !required.Any(
                               kvp2 => kvp2.Key != ORE &&
                                       kvp2.Value > 0 &&
                                       reactions[kvp2.Key].Inputs.Select(i => i.Element)
                                                .Contains(kvp.Key)));

                var reaction = reactions[next.Key];
                var quantity = (long)Math.Ceiling((decimal)next.Value / reaction.Output.Quantity);
                if (quantity * reaction.Output.Quantity > next.Value)
                {
                    stock[next.Key] += quantity * reaction.Output.Quantity - next.Value;
                }
                foreach (var input in reaction.Inputs)
                {
                    if (stock[input.Element] >= input.Quantity * quantity)
                    {
                        stock[input.Element] -= input.Quantity * quantity;
                    }
                    else
                    {
                        required[input.Element] += input.Quantity * quantity - stock[input.Element];
                        stock[input.Element] = 0;
                    }
                }

                required[next.Key] = 0;
            }


            return required[ORE];
        }


        private async Task<long> Part2()
        {
            var reactions = (await ParseInput()).ToDictionary(r => r.Output.Element);

            var elements = reactions
                           .Values.SelectMany(r => r.Inputs.Select(i => i.Element).Concat(new[] {r.Output.Element}))
                           .Distinct().ToList();
            var stock = elements.ToDictionary(e => e, e => e == ORE ? 1000000000000L : 0L);

            var count = 0L;
            var step = 1000000L;
            while (true)
            {
                var stockClone = stock.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var required = elements.ToDictionary(e => e, e => e == FUEL ? step : 0L);
                while (required.Any(kvp => kvp.Key != ORE && kvp.Value > 0))
                {
                    var next = required.First(
                        kvp => kvp.Key != ORE &&
                               kvp.Value > 0 &&
                               !required.Any(
                                   kvp2 => kvp2.Key != ORE &&
                                           kvp2.Value > 0 &&
                                           reactions[kvp2.Key].Inputs.Select(i => i.Element)
                                                              .Contains(kvp.Key)));

                    var reaction = reactions[next.Key];
                    var quantity = (long)Math.Ceiling((decimal)next.Value / reaction.Output.Quantity);
                    if (quantity * reaction.Output.Quantity > next.Value)
                    {
                        stockClone[next.Key] += quantity * reaction.Output.Quantity - next.Value;
                    }

                    foreach (var input in reaction.Inputs)
                    {
                        if (stockClone[input.Element] >= input.Quantity * quantity)
                        {
                            stockClone[input.Element] -= input.Quantity * quantity;
                        }
                        else
                        {
                            required[input.Element] += input.Quantity * quantity - stockClone[input.Element];
                            stockClone[input.Element] = 0;
                        }
                    }

                    required[next.Key] = 0;
                }

                if (stockClone[ORE] >= required[ORE])
                {
                    stockClone[ORE] -= required[ORE];
                    count+=step;
                    stock = stockClone;
                }
                else if (step > 1)
                {
                    step /= 10;
                }
                else
                {
                    break;
                }
            }

            return count;
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
