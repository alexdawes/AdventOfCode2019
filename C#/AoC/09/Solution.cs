using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AoC._09
{
    internal sealed class Solution : ISolution
    {
        public enum ParameterMode
        {
            Position = 0,
            Immediate = 1,
            Relative = 2,
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

            public static Instruction Parse(long value)
            {
                var opCode = (int)(value % 100);
                var modes = new List<ParameterMode>();
                var p = value / 100;
                while (p != 0)
                {
                    modes.Add((ParameterMode)(p % 10));
                    p = p / 10;
                }
                return new Instruction(opCode, modes);
            }
        }

        public sealed class Program
        {
            private readonly List<long> _buffer;
            private int _pointer;
            private int _offset;

            public Program(IList<long> buffer)
            {
                _buffer = buffer.ToList();
                _pointer = 0;
                _offset = 0;
            }

            public Program Clone()
            {
                var program = new Program(_buffer.ToList()) {_pointer = _pointer};
                return program;
            }

            private long GetReadParameter(Instruction instruction, int index)
            {
                switch (instruction.GetParameterMode(index))
                {
                    case ParameterMode.Position:
                        return Get(Get(_pointer + index + 1));
                    case ParameterMode.Immediate:
                        return Get(_pointer + index + 1);
                    case ParameterMode.Relative:
                        return Get(Get(_pointer + index + 1) + _offset);
                    default:
                        throw new InvalidOperationException();
                }
            }

            private long GetWriteParameter(Instruction instruction, int index)
            {
                switch (instruction.GetParameterMode(index))
                {
                    case ParameterMode.Position:
                        return Get(_pointer + index + 1);
                    case ParameterMode.Relative:
                        return Get(_pointer + index + 1) + _offset;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public void Set(long index, long value)
            {
                while (_buffer.Count <= index)
                {
                    _buffer.Add(0);
                }
                _buffer[(int)index] = value;
            }

            public long Get(long index)
            {
                if (index >= _buffer.Count)
                {
                    return 0;
                }
                return _buffer[(int)index];
            }
            
            private void Log(int count, string extra = "")
            {
                // Console.WriteLine($"[ {string.Join(" ", Enumerable.Range(0, count).Select(i => _buffer[_pointer + i]))} ] ({_pointer}) {extra}");
            }

            private async Task RunNext(List<long> input, List<long> output)
            {
                var instruction = Instruction.Parse(_buffer[_pointer]);
                switch (instruction.OpCode)
                {
                    case 1:
                        {
                            var (o1, o2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1),
                                               GetWriteParameter(instruction, 2));

                            Log(4, $"[{r}] = {o1} + {o2}");
                            Set(r, o1 + o2);
                            _pointer += 4;
                            break;
                        }
                        ;
                    case 2:
                        {
                            var (o1, o2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1),
                                           GetWriteParameter(instruction, 2));
                            Log(4, $"[{r}] = {o1} * {o2}");
                            Set(r, o1 * o2);
                            _pointer += 4;
                            break;
                        }
                    case 3:
                        {
                            while (input.Count == 0)
                            {
                                await Task.Delay(100);
                            }
                            var i = input[0];
                            input.RemoveAt(0);
                            var r = GetWriteParameter(instruction, 0);
                            Log(2, $"[{r}] = {i}");
                            Set(r, i);
                            _pointer += 2;
                            break;
                        }
                    case 4:
                        {
                            var o = GetReadParameter(instruction, 0);
                            Log(2, $"=> {o}");
                            output.Add(o);
                            _pointer += 2;
                            break;
                        }
                    case 5:
                        {
                            var (p1, p2) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1));
                            Log(3, $"{p1} != 0 ? p={p2} : p+=3");
                            if (p1 != 0)
                            {
                                _pointer = (int)p2;
                            }
                            else
                            {
                                _pointer += 3;
                            }

                            break;
                        }
                    case 6:
                        {
                            var (p1, p2) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1));
                            Log(3, $"{p1} == 0 ? p={p2} : p+=3");
                            if (p1 == 0)
                            {
                                _pointer = (int)p2;
                            }
                            else
                            {
                                _pointer += 3;
                            }
                            break;
                        }
                    case 7:
                        {
                            var (c1, c2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1), GetWriteParameter(instruction, 2));
                            Log(4, $"[{r}] = ({c1} < {c2} ? 1 : 0)");
                            Set(r, c1 < c2 ? 1 : 0);
                            _pointer += 4;
                            break;
                        }
                    case 8:
                        {
                            var (c1, c2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1), GetWriteParameter(instruction, 2));
                            Log(4, $"[{r}] = ({c1} == {c2} ? 1 : 0)");
                            Set(r, c1 == c2 ? 1 : 0);
                            _pointer += 4;
                            break;
                        }
                    case 9:
                        {
                            var o = GetReadParameter(instruction, 0);
                            Log(2, $"o += {o}");
                            _offset += (int)o;
                            _pointer += 2;
                            break;
                        }
                    case 99:
                        {
                            Log(1);
                            break;
                        }
                    default:
                        {
                            throw new InvalidOperationException($"Unrecognised OpCode: {instruction.OpCode}");
                        }
                }
            }

            public async Task RunToCompletion(List<long> input, List<long> output)
            {
                while (Get(_pointer) != 99)
                {
                    await RunNext(input, output);
                }
            }
        }

        private async Task<Program> ParseInput()
        {
            var instructions = (await File.ReadAllTextAsync("09/input"))
                               .Split(',')
                               .Select(long.Parse)
                               .ToList();
            return new Program(instructions);
        }
        
        private async Task<long> Part1()
        {
            var program = await ParseInput();
            var input = new List<long> {1};
            var output = new List<long>();
            await program.RunToCompletion(input, output);
            // Console.WriteLine($"[ {string.Join(" ", output)} ]");
            return output.Last();
        }

        private async Task<long> Part2()
        {
            var program = await ParseInput();
            var input = new List<long> { 2
            };
            var output = new List<long>();
            await program.RunToCompletion(input, output);
            // Console.WriteLine($"[ {string.Join(" ", output)} ]");
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
