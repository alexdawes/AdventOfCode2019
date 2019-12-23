using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC._22
{
    public sealed class Solution : ISolution
    {
        public interface IDeal
        {
            int[] Deal(int[] deck);
        }

        public sealed class DealIntoNewStack : IDeal
        {
            public static Regex Regex = new Regex(@"deal into new stack");

            public int[] Deal(int[] deck)
            {
                var result = new int[deck.Length];
                for (var i = 0; i < deck.Length; i++)
                {
                    result[deck.Length - i - 1] = deck[i];
                }

                return result;
            }
        }

        public sealed class Cut : IDeal
        {
            public static Regex Regex = new Regex(@"cut (?<value>-?\d+)");
            public readonly int Value;

            public Cut(int value)
            {
                Value = value;
            }

            public int[] Deal(int[] deck)
            {
                var result = new int[deck.Length];
                for (var i = 0; i < deck.Length; i++)
                {
                    var index = (i - Value) % deck.Length;
                    while (index < 0)
                    {
                        index += deck.Length;
                    }
                    result[index] = deck[i];
                }

                return result;
            }
        }

        public sealed class DealWithIncrement : IDeal
        {
            public static Regex Regex = new Regex(@"deal with increment (?<value>\d+)");
            public readonly int Value;

            public DealWithIncrement(int value)
            {
                Value = value;
            }

            public int[] Deal(int[] deck)
            {
                var result = new int[deck.Length];
                for (var i = 0; i < deck.Length; i++)
                {
                    var index = (Value * i) % deck.Length;
                    while (index < 0)
                    {
                        index += deck.Length;
                    }
                    result[index] = deck[i];
                }

                return result;
            }
        }

        public static int[] GetDeck() => Enumerable.Range(0, 10007).ToArray();

        public static IDeal Parse(string instruction)
        {
            if (DealIntoNewStack.Regex.IsMatch(instruction))
            {
                return new DealIntoNewStack();
            }

            if (Cut.Regex.IsMatch(instruction))
            {
                var match = Cut.Regex.Match(instruction);
                return new Cut(int.Parse(match.Groups["value"].Value));
            }

            if (DealWithIncrement.Regex.IsMatch(instruction))
            {
                var match = DealWithIncrement.Regex.Match(instruction);
                return new DealWithIncrement(int.Parse(match.Groups["value"].Value));
            }

            throw new InvalidOperationException($"'{instruction}' is not a recognised Deal.");
        }

        public static async Task<IList<IDeal>> ParseInput()
        {
            return (await File.ReadAllLinesAsync("22/input")).Select(Parse).ToList();
        }

        public async Task<int> Part1()
        {
            var deck = GetDeck();
            var deals = await ParseInput();

            foreach (var deal in deals)
            {
                deck = deal.Deal(deck);
            }

            return deck.ToList().IndexOf(2019);
        }

        public static long ModInverse(long value, long deckLength)
        {
            return (long)BigInteger.ModPow(value, deckLength - 2, deckLength);
        }

        public async Task<long> Part2()
        {
            var deals = await ParseInput();
            var repetitions = 101741582076661L;
            var deckLength = 119315717514047L;
            var position = 2020;
            
            var (offset, increment) = (0L, 1L);
            foreach (var deal in deals)
            {
                if (deal is DealIntoNewStack)
                {
                    increment *= -1;
                    offset += increment;
                }
                else if (deal is Cut c)
                {
                    offset += (long)(BigInteger.Multiply(increment, c.Value) % deckLength);
                }
                else if (deal is DealWithIncrement dwi)
                {
                    increment = (long)(BigInteger.Multiply(increment, ModInverse(dwi.Value, deckLength)) % deckLength);
                }

                (offset, increment) = (offset % deckLength, increment % deckLength);
                while (offset < 0) { offset += deckLength; }
                while (increment < 0) { increment += deckLength; }
            }

            var totalIncrement = (long)BigInteger.ModPow(increment, repetitions, deckLength);
            var totalOffset = (long)(BigInteger.Multiply(BigInteger.Multiply(offset, 1 - BigInteger.ModPow(increment, repetitions, deckLength)), ModInverse(1 - increment, deckLength)) % deckLength);

            return (long)(BigInteger.Add(BigInteger.Multiply(totalIncrement, position), totalOffset) % deckLength);
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
