using System;
using System.Threading.Tasks;

namespace AoC.Utils.IntCode
{
    public sealed class Computer
    {
        private int _pointer;
        private int _offset;
        private readonly Program _program;

        private TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>();

        public Computer(Program program)
        {
            _program = program;
        }

        public async Task WaitUntilInputRequired()
        {
            await _tcs.Task;
        }

        private long GetReadParameter(Instruction instruction, int index)
        {
            switch (instruction.GetParameterMode(index))
            {
                case ParameterMode.Position:
                    return _program.Get(_program.Get(_pointer + index + 1));
                case ParameterMode.Immediate:
                    return _program.Get(_pointer + index + 1);
                case ParameterMode.Relative:
                    return _program.Get(_program.Get(_pointer + index + 1) + _offset);
                default:
                    throw new InvalidOperationException();
            }
        }

        private long GetWriteParameter(Instruction instruction, int index)
        {
            switch (instruction.GetParameterMode(index))
            {
                case ParameterMode.Position:
                    return _program.Get(_pointer + index + 1);
                case ParameterMode.Relative:
                    return _program.Get(_pointer + index + 1) + _offset;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void Log(int count, string extra = "")
        {
            // Console.WriteLine($"[ {string.Join(" ", Enumerable.Range(0, count).Select(i => _buffer[_pointer + i]))} ] ({_pointer}) {extra}");
        }

        private async Task RunNext(IoStream input, IoStream output)
        {
            var instruction = Instruction.Parse(_program[_pointer]);
            switch (instruction.OpCode)
            {
                case 1:
                {
                    var (o1, o2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1),
                                       GetWriteParameter(instruction, 2));

                    Log(4, $"[{r}] = {o1} + {o2}");
                    _program.Set(r, o1 + o2);
                    _pointer += 4;
                    break;
                }
                    ;
                case 2:
                {
                    var (o1, o2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1),
                                       GetWriteParameter(instruction, 2));
                    Log(4, $"[{r}] = {o1} * {o2}");
                    _program.Set(r, o1 * o2);
                    _pointer += 4;
                    break;
                }
                case 3:
                {
                    var t = input.WaitNext();
                    if (!t.IsCompleted)
                    {
                        _tcs.SetResult(0);
                    }

                    var i = await t;
                    _tcs = new TaskCompletionSource<int>();
                    var r = GetWriteParameter(instruction, 0);
                    Log(2, $"[{r}] = {i}");
                    _program.Set(r, i);
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
                    var (c1, c2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1),
                                       GetWriteParameter(instruction, 2));
                    Log(4, $"[{r}] = ({c1} < {c2} ? 1 : 0)");
                    _program.Set(r, c1 < c2 ? 1 : 0);
                    _pointer += 4;
                    break;
                }
                case 8:
                {
                    var (c1, c2, r) = (GetReadParameter(instruction, 0), GetReadParameter(instruction, 1),
                                       GetWriteParameter(instruction, 2));
                    Log(4, $"[{r}] = ({c1} == {c2} ? 1 : 0)");
                    _program.Set(r, c1 == c2 ? 1 : 0);
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

        public async Task RunToCompletion(IoStream input, IoStream output)
        {
            while (_program.Get(_pointer) != 99)
            {
                await RunNext(input, output);
            }
        }
    }
}
