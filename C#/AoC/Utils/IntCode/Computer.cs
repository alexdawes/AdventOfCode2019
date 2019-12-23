using System;
using System.Threading;
using System.Threading.Tasks;

namespace AoC.Utils.IntCode
{
    public sealed class Computer
    {
        private int _pointer;
        private int _offset;
        private readonly Program _program;
        private Task _runTask;
        private readonly object _lock = new object();
        private CancellationTokenSource _cts;

        public IoStream Input { get; set; }
        public IoStream Output { get; set; }

        private TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>();

        public Computer(Program program)
        {
            _program = program;
            Input = new IoStream();
            Output = new IoStream();
        }

        public async Task WaitUntilInputRequired(CancellationToken ct = default(CancellationToken))
        {
            var cts = new CancellationTokenSource();
            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token).Token;
            var cancellationTokenTask = Task.Delay(-1, token);

            if (await Task.WhenAny(_tcs.Task, cancellationTokenTask) == cancellationTokenTask)
            {
                throw new TaskCanceledException();
            }
            else
            {
                cts.Cancel();
            }
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

        private async Task RunNext()
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
                    var t = Input.Read(_cts.Token);

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
                    await Output.Write(o);
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

        public void Start()
        {
            lock (_lock)
            {
                if (_runTask == null)
                {
                    _cts = new CancellationTokenSource();
                    _runTask = RunToCompletion();
                }
            }
        }

        public async Task WaitUntilCompleted(CancellationToken ct = default(CancellationToken))
        {
            Task t;
            lock (_lock)
            {
                t = _runTask ?? Task.CompletedTask;
            }


            var cts = new CancellationTokenSource();
            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token).Token;
            var cancellationTokenTask = Task.Delay(-1, token);

            if (await Task.WhenAny(t, cancellationTokenTask) == cancellationTokenTask)
            {
                throw new TaskCanceledException();
            }
            else
            {
                cts.Cancel();
            }
        }

        public void Stop()
        {
            CancellationTokenSource cts = _cts;

            if (cts != null)
            {
                lock (_lock)
                {
                    cts.Cancel();
                    cts.Token.WaitHandle.WaitOne();
                    _runTask = null;
                    _cts = null;
                }
            }
        }


        private async Task RunToCompletion()
        {
            while (!_cts.Token.IsCancellationRequested && _program.Get(_pointer) != 99)
            {
                await RunNext();
            }
        }
    }
}
