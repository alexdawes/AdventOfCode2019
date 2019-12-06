using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace AoC._05
{
    internal sealed class Solution : ISolution
    {
        public enum ParameterMode
        {
            Position = 0,
            Immediate = 1
        }

        public struct Instruction
        {
            public int OpCode { get; }

            public List<ParameterMode> Modes { get; }

            public Instruction(int opCode, List<ParameterMode> modes)
            {
                OpCode = opCode;
                Modes = modes;
            }

            public ParameterMode GetParameterMode(int idx)
            {
                return Modes.Count > idx ? Modes[idx] : ParameterMode.Position;
            }

            public static Instruction Parse(int value)
            {
                var opCode = value % 100;
                var modes = new List<ParameterMode>();
                var p = value / 100;
                while (p != 0)
                {
                    modes.Add(p % 10 == 1 ? ParameterMode.Immediate : ParameterMode.Position);
                    p = p / 10;
                }
                return new Instruction(opCode, modes);
            }
        }

        public sealed class Program
        {
            private readonly int[] _buffer;
            private int _pointer;

            public Program(IList<int> buffer)
            {
                _buffer = buffer.ToArray();
                _pointer = 0;
            }

            private int GetReadParameter(Instruction instruction, int index)
            {
                switch (instruction.GetParameterMode(index))
                {
                    case ParameterMode.Position:
                        return _buffer[_buffer[_pointer + index + 1]];
                    case ParameterMode.Immediate:
                        return _buffer[_pointer + index + 1];
                    default:
                        throw new InvalidOperationException();
                }
            }

            private int GetWriteParameter(Instruction instruction, int index)
            {
                return _buffer[_pointer + index + 1];
            }

            public void Set(int index, int value)
            {
                _buffer[index] = value;
            }

            public int Get(int index)
            {
                return _buffer[index];
            }



            private void Log(int count)
            {
                // Console.WriteLine($"[ {string.Join(" ", Enumerable.Range(0, count).Select(i => _buffer[_pointer + i]))} ]");
            }

            private void RunNext(List<int> input, List<int> output)
            {
                var instruction = Instruction.Parse(_buffer[_pointer]);
                switch (instruction.OpCode)
                {
                    case 1:
                    {
                        Log(4);
                        var (o1, o2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1),
                                           GetWriteParameter(instruction, 2));
                        _buffer[r] = o1 + o2;
                        _pointer += 4;
                        break;
                    }
                        ;
                    case 2:
                    {
                        Log(4);
                            var (o1, o2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1),
                                           GetWriteParameter(instruction, 2));
                            _buffer[r] = o1 * o2;
                        _pointer += 4;
                        break;
                    }
                    case 3:
                    {
                        Log(2);
                            var i = input[0];
                        input.RemoveAt(0);
                        var r = _buffer[_pointer + 1];
                        _buffer[r] = i;
                        _pointer += 2;
                        break;
                    }
                    case 4:
                    {
                        Log(2);
                            var o = GetReadParameter(instruction, 0);
                        output.Add(o);
                        _pointer += 2;
                        break;
                    }
                    case 5:
                    {
                        Log(3);
                        var (p1, p2) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1));
                        if (p1 != 0)
                        {
                            _pointer = p2;
                        }
                        else
                        {
                            _pointer += 3;
                        }

                        break;
                    }
                    case 6:
                    {
                        Log(3);
                        var (p1, p2) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1));
                        if (p1 == 0)
                        {
                            _pointer = p2;
                        }
                        else
                        {
                            _pointer += 3;
                        }
                        break;
                    }
                    case 7:
                    {
                        Log(4);
                        var (c1, c2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1), GetWriteParameter(instruction, 2));
                        _buffer[r] = (c1 < c2 ? 1 : 0);
                        _pointer += 4;
                        break;
                    }
                    case 8:
                    {
                        Log(4);
                        var (c1, c2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1), GetWriteParameter(instruction, 2));
                        _buffer[r] = (c1 == c2 ? 1 : 0);
                        _pointer += 4;
                        break;
                    }
                    case 99:
                    {
                        break;
                    }
                    default:
                    {
                        throw new InvalidOperationException($"Unrecognised OpCode: {instruction.OpCode}");
                    }
                }
            }

            public void RunToCompletion(List<int> input, List<int> output)
            {
                while (_buffer[_pointer] != 99)
                {
                    RunNext(input, output);
                }
            }
        }

        private async Task<Program> ParseInput()
        {
            var instructions = (await File.ReadAllTextAsync("05/input"))
                   .Split(',')
                   .Select(int.Parse)
                   .ToList();
            return new Program(instructions);
        }

        private async Task<int> Part1()
        {
            var program = await ParseInput();
            var input = new List<int> {1};
            var output = new List<int>();
            program.RunToCompletion(input, output);
            return output.Last();
        }

        private async Task<int> Part2()
        {
            var program = await ParseInput();
            var input = new List<int> { 5 };
            var output = new List<int>();
            program.RunToCompletion(input, output);
            return output.Last();
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
