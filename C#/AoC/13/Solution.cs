using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AoC._13
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
            
            private TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>();

            public async Task WaitUntilInputRequired()
            {
                await _tcs.Task;
            }

            public Program(IList<long> buffer)
            {
                _buffer = buffer.ToList();
                _pointer = 0;
                _offset = 0;
            }

            public Program Clone()
            {
                var program = new Program(_buffer.ToList()) { _pointer = _pointer };
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

            private async Task RunNext(AsyncQueue<long> input, AsyncQueue<long> output)
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
                            var t = input.WaitNext();
                            if (!t.IsCompleted)
                            {
                                _tcs.SetResult(0);
                            }

                            var i = await t;
                            _tcs = new TaskCompletionSource<int>();
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

            public async Task RunToCompletion(AsyncQueue<long> input, AsyncQueue<long> output)
            {
                while (Get(_pointer) != 99)
                {
                    await RunNext(input, output);
                }
            }
        }

        public sealed class AsyncQueue<T> : IEnumerable<T>
        {
            private readonly Queue<T> _queue;
            private TaskCompletionSource<int> _tcs;

            public AsyncQueue(IEnumerable<T> items)
            {
                _queue = new Queue<T>(items);
                _tcs = new TaskCompletionSource<int>();
            }

            public AsyncQueue() : this(new T[] { })
            {
            }

            public void Add(T item)
            {
                _queue.Enqueue(item);
                var tcs = _tcs;
                _tcs = new TaskCompletionSource<int>();
                tcs.SetResult(0);
            }

            public async Task<T> WaitNext()
            {
                var waitTask = _tcs.Task;
                while (true)
                {
                    if (_queue.TryDequeue(out T result))
                    {
                        return result;
                    }
                    else
                    {
                        await waitTask;
                        waitTask = _tcs.Task;
                    }
                }
            }

            public IEnumerable<T> AsEnumerable() => _queue.ToList().AsEnumerable();

            public IEnumerator<T> GetEnumerator()
            {
                return AsEnumerable().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public enum TileType
        {
            Empty = 0, 
            Wall = 1,
            Block = 2,
            HorizontalPaddle = 3,
            Ball = 4
        }

        public enum Mode
        {
            Manual,
            Automatic,
            DoNothing
        }

        public sealed class Game
        {
            private readonly Program _program;
            private readonly AsyncQueue<long> _input;
            private readonly AsyncQueue<long> _output;
            private Task _runTask;
            private readonly object _lock = new object();

            public Game(IDictionary<(long x, long y), TileType> initialTiles)
            {
                Tiles = initialTiles;
            }

            public Game(Program program)
            {
                _program = program;
                _input = new AsyncQueue<long>();
                _output = new AsyncQueue<long>();
            }

            public Task Start()
            {
                _runTask = _program.RunToCompletion(_input, _output);
                var t = Task.Run(async () =>
                {
                    while (!_runTask.IsCompleted || _output.Any())
                    {
                        var x = await _output.WaitNext();
                        var y = await _output.WaitNext();
                        var z = await _output.WaitNext();
                        Set(x, y, z);
                    }
                });
                return Task.WhenAll(t, _runTask);
            }

            public void TurnOnEasyMode()
            {
                for (var i = 1362; i <= 1397; i++)
                {
                    _program.Set(i, 1);
                }
            }
            
            public async Task PlayStep(Mode mode)
            {
                await _program.WaitUntilInputRequired();
                //Console.Clear();
                //Console.WriteLine(this);
                //await Task.Delay(20);
                switch (mode)
                {
                    case Mode.Manual:
                        switch (Console.ReadKey().Key)
                        {
                            case ConsoleKey.LeftArrow:
                                _input.Add(-1);
                                break;
                            case ConsoleKey.RightArrow:
                                _input.Add(1);
                                break;
                            default:
                                _input.Add(0);
                                break;
                        }

                        break;
                    case Mode.Automatic:
                        var bat = FindBat();
                        var ball = FindBall();
                        _input.Add(ball.X.CompareTo(bat.X));
                        break;
                    case Mode.DoNothing:
                        await Task.Delay(20);
                        _input.Add(0);
                        break;
                }
            }

            public async Task Play(Mode mode)
            {
                while (!_runTask.IsCompleted)
                {
                    await PlayStep(mode);
                }
            }

            public void Set(long x, long y, long value)
            {
                if (x == -1 && y == 0)
                {
                    Score = value;
                }
                else
                {
                    lock (_lock)
                    {
                        Tiles[(x, y)] = (TileType)value;
                    }
                }
            }

            public IDictionary<(long X, long Y), TileType> Tiles { get; } = new Dictionary<(long X, long Y), TileType>();

            public long Score { get; private set; }

            public (long X, long Y) FindBat()
            {
                lock (_lock)
                {
                    return Tiles.ToList().First(kvp => kvp.Value == TileType.HorizontalPaddle).Key;
                }
            }

            public (long X, long Y) FindBall()
            {
                lock (_lock)
                {
                    return Tiles.ToList().First(kvp => kvp.Value == TileType.Ball).Key;
                }
            }

            public override string ToString()
            {
                var (x1, x2, y1, y2) = (Tiles.Keys.Min(k => k.X), Tiles.Keys.Max(k => k.X), Tiles.Keys.Min(k => k.Y), Tiles.Keys.Max(k => k.Y));
                var s = new StringBuilder();
                s.AppendLine();
                s.AppendLine($"Score: {Score}");
                s.AppendLine();
                for (var y = y1; y <= y2; y++)
                {
                    var row = new StringBuilder();
                    for (var x = x1; x <= x2; x++)
                    {
                        if (!Tiles.ContainsKey((x, y)) || Tiles[(x, y)] == TileType.Empty)
                        {
                            row.Append(' ');
                        }
                        else if (Tiles[(x, y)] == TileType.Ball)
                        {
                            row.Append('O');
                        }
                        else if (Tiles[(x, y)] == TileType.Block)
                        {
                            row.Append('X');
                        }
                        else if (Tiles[(x, y)] == TileType.Wall)
                        {
                            row.Append('#');
                        }
                        else if (Tiles[(x, y)] == TileType.HorizontalPaddle)
                        {
                            row.Append('-');
                        }
                        else
                        {
                            row.Append(' ');
                        }
                    }

                    s.AppendLine(row.ToString());
                }

                s.AppendLine();

                return s.ToString();
            }
        }
        
        private async Task<Program> ParseInput()
        {
            var instructions = (await File.ReadAllTextAsync("13/input"))
                               .Split(',')
                               .Select(long.Parse)
                               .ToList();
            return new Program(instructions);
        }


        private async Task<int> Part1()
        {
            var program = await ParseInput();
            var game = new Game(program);
            await game.Start();
            return game.Tiles.Count(kvp => kvp.Value == TileType.Block);
        }

        private async Task<long> Part2()
        {
            var program = await ParseInput();
            program.Set(0, 2);
            var game = new Game(program);
            // game.TurnOnEasyMode();
            var t = game.Start();
            await game.Play(Mode.Automatic);
            return game.Score;
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
