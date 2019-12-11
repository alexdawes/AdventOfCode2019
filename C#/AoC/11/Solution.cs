using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC._11
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
                            var i = await input.WaitNext();
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

            public AsyncQueue() : this (new T[] {})
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

        private async Task<Program> ParseInput()
        {
            var instructions = (await File.ReadAllTextAsync("11/input"))
                               .Split(',')
                               .Select(long.Parse)
                               .ToList();
            return new Program(instructions);
        }

        public enum Color
        {
            Black = 0,
            White = 1
        }

        public enum Direction
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }

        public static Direction TurnLeft(Direction direction)
        {
            return (Direction)(((int)direction + 3) % 4);
        }
        public static Direction TurnRight(Direction direction)
        {
            return (Direction)(((int)direction + 1) % 4);
        }

        public static (int X, int Y) Move((int X, int Y) coord, Direction direction)
        {
            var (x, y) = coord;
            switch (direction)
            {
                case Direction.Up: return (x, y - 1);
                case Direction.Down: return (x, y + 1);
                case Direction.Left: return (x - 1, y);
                case Direction.Right: return (x + 1, y);
                default: return (x, y);
            }
        }

        public enum Rotation
        {
            Clockwise = 1,
            Anticlockwise = 0
        }

        public static Direction Turn(Direction direction, Rotation rotation)
        {
            switch (rotation)
            {
                case Rotation.Clockwise: return TurnRight(direction);
                case Rotation.Anticlockwise: return TurnLeft(direction);
                default: return direction;
            }
        }

        public sealed class Canvas
        {
            private readonly Dictionary<(int X, int Y), List<Color>> _paintLayers = new Dictionary<(int X, int Y), List<Color>>();
            private ((int X, int Y) Position, Direction Direction) _robot = ((0, 0), Direction.Up);
            
            public void Iterate(Color color, Rotation rotation)
            {
                // Console.WriteLine($"Current: {_robot.Position} {_robot.Direction}, Painting: {color}, Turning: {rotation}");
                Paint(color);
                TurnRobot(rotation);
                StepRobot();
            }
            
            private void Paint(Color color)
            {
                if (!_paintLayers.ContainsKey(_robot.Position))
                {
                    _paintLayers[_robot.Position] = new List<Color>();
                }

                _paintLayers[_robot.Position].Add(color);
            }

            private void TurnRobot(Rotation rotation)
            {
                _robot.Direction = Turn(_robot.Direction, rotation);
            }

            private void StepRobot()
            {
                _robot.Position = Move(_robot.Position, _robot.Direction);
            }

            public int GetPaintedPanelsCount()
            {
                return _paintLayers.Keys.Count;
            }

            public Color GetColor((int X, int Y) position)
            {
                return _paintLayers.TryGetValue(position, out List<Color> colors) ? colors.Last() : Color.Black;
            }

            public Color GetCurrentColor()
            {
                return GetColor(_robot.Position);
            }

            public override string ToString()
            {
                var (x1, x2, y1, y2) = (_paintLayers.Keys.Select(k => k.X).Min(),
                                        _paintLayers.Keys.Select(k => k.X).Max(),
                                        _paintLayers.Keys.Select(k => k.Y).Min(),
                                        _paintLayers.Keys.Select(k => k.Y).Max());

                var result = new StringBuilder();
                for (var y = y1; y <= y2; y++)
                {
                    var line = new StringBuilder();
                    for (var x = x1; x <= x2; x++)
                    {
                        line.Append(GetColor((x, y)) == Color.Black ? ' ' : 'X');
                    }

                    result.AppendLine(line.ToString());
                }

                return result.ToString();
            }
        }

        private async Task<int> Part1()
        {
            var program = await ParseInput();
            var canvas = new Canvas();
            var input = new AsyncQueue<long> { (int)Color.Black };
            var output = new AsyncQueue<long>();
            var task = program.RunToCompletion(input, output);
            while (true)
            {
                var colorToPaint = (Color)(await output.WaitNext());
                var rotation = (Rotation)(await output.WaitNext());
                canvas.Iterate(colorToPaint, rotation);
                input.Add((int)canvas.GetCurrentColor());
                if (task.IsCompleted)
                {
                    break;
                }
            }

            return canvas.GetPaintedPanelsCount();
        }


        private async Task<string> Part2()
        {
            var program = await ParseInput();
            var canvas = new Canvas();
            var input = new AsyncQueue<long> { (int)Color.White };
            var output = new AsyncQueue<long>();
            var task = program.RunToCompletion(input, output);
            while (true)
            {
                var colorToPaint = (Color)(await output.WaitNext());
                var rotation = (Rotation)(await output.WaitNext());
                canvas.Iterate(colorToPaint, rotation);
                input.Add((int)canvas.GetCurrentColor());
                if (task.IsCompleted)
                {
                    break;
                }
            }

            return canvas.ToString();
        }

        public async Task Run()
        {
            var part1 = await Part1();
            Console.WriteLine($"Part 1: {part1}");

            var part2 = await Part2();
            Console.WriteLine($"Part 2:\n{part2}");
        }
    }
}
