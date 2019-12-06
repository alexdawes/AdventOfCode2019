using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AoC._02
{
    internal sealed class Solution : ISolution
    {
        public sealed class Program
        {
            private readonly int[] _buffer;
            private int _pointer;

            public Program(IList<int> buffer)
            {
                _buffer = buffer.ToArray();
                _pointer = 0;
            }

            public void Set(int index, int value)
            {
                _buffer[index] = value;
            }

            public int Get(int index)
            {
                return _buffer[index];
            }

            private void RunNext()
            {
                var opCode = _buffer[_pointer];
                switch (opCode)
                {
                    case 1:
                    {
                        var (o1, o2, r) = (_buffer[_pointer + 1], _buffer[_pointer + 2], _buffer[_pointer + 3]);
                        _buffer[r] = _buffer[o1] + _buffer[o2];
                        _pointer += 4;
                        break;
                    }
                    case 2:
                    {
                        var (o1, o2, r) = (_buffer[_pointer + 1], _buffer[_pointer + 2], _buffer[_pointer + 3]);
                        _buffer[r] = _buffer[o1] * _buffer[o2];
                        _pointer += 4;
                            break;
                    }
                    case 99:
                    {
                        break;
                    }
                    default:
                    {
                        throw new InvalidOperationException($"Unrecognised OpCode: {opCode}");
                    }
                }
            }

            public void RunToCompletion()
            {
                while (_buffer[_pointer] != 99)
                {
                    RunNext();
                }
            }
        }

        private async Task<Program> ParseInput()
        {
            var instructions = (await File.ReadAllTextAsync("02/input"))
                   .Split(',')
                   .Select(int.Parse)
                   .ToList();
            return new Program(instructions);
        }

        private async Task<int> Part1()
        {
            var program = await ParseInput();
            program.Set(1, 12);
            program.Set(2, 2);
            program.RunToCompletion();
            return program.Get(0);
        }

        private async Task<int> Part2()
        {
            var expected = 19690720;
            for (var noun = 0; noun < 100; noun++)
            {
                for (var verb = 0; verb < 100; verb++)
                {
                    var program = await ParseInput();
                    program.Set(1, noun);
                    program.Set(2, verb);
                    program.RunToCompletion();
                    var actual = program.Get(0);
                    if (actual == expected)
                    {
                        return 100 * noun + verb;
                    }
                }
            }

            throw new Exception("No answer found.");
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
