using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace AoC._07
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

            public Program Clone()
            {
                var program = new Program(_buffer.ToList()) {_pointer = _pointer};
                return program;
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

            private async Task RunNext(List<int> input, List<int> output)
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
                            while (input.Count == 0)
                            {
                                await Task.Delay(100);
                            }
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

            public async Task RunToCompletion(List<int> input, List<int> output)
            {
                while (_buffer[_pointer] != 99)
                {
                    await RunNext(input, output);
                }
            }
        }

        private async Task<Program> ParseInput()
        {
            var instructions = (await File.ReadAllTextAsync("07/input"))
                               .Split(',')
                               .Select(int.Parse)
                               .ToList();
            return new Program(instructions);
        }

        private IEnumerable<List<int>> GetPermutations(params int[] choices)
        {
            if (!choices.Any())
            {
                return new [] {new List<int>()};
            }

            return choices.SelectMany((c, cIdx) =>
            {
                return GetPermutations(choices.Where((_, idx) => idx != cIdx).ToArray())
                    .Select(perm => new[] {c}.Concat(perm).ToList());
            }).ToList();
        }
        
        private async Task<int> RunForPermutation(Program program, List<int> permutation)
        {
            var (a, b, c, d, e) = (permutation[0], permutation[1], permutation[2], permutation[3], permutation[4]);
            await Task.Yield();

            var programA = program.Clone();
            var programB = program.Clone();
            var programC = program.Clone();
            var programD = program.Clone();
            var programE = program.Clone();

            var streamA = new List<int> { a, 0 };
            var streamB = new List<int> {b};
            var streamC = new List<int> {c};
            var streamD = new List<int> {d};
            var streamE = new List<int> {e};

            await Task.WhenAll(new[]
            {
                programA.RunToCompletion(streamA, streamB),
                programB.RunToCompletion(streamB, streamC),
                programC.RunToCompletion(streamC, streamD),
                programD.RunToCompletion(streamD, streamE),
                programE.RunToCompletion(streamE, streamA)
            });

            return streamA.Last();
        }
        
        private async Task<int> Part1()
        {
            var permutations = GetPermutations(0, 1, 2, 3, 4);
            var program = await ParseInput();

            var results = await Task.WhenAll(permutations.Select(async permutation => await RunForPermutation(program, permutation)));
            return results.Max();
        }

        private async Task<int> Part2()
        {
            var permutations = GetPermutations(5, 6, 7, 8, 9);
            var program = await ParseInput();

            var results = await Task.WhenAll(permutations.Select(async permutation => await RunForPermutation(program, permutation)));
            return results.Max();
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
