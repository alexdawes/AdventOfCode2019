using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._13
{

    internal sealed class Solution : ISolution
    {
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
            private readonly IntCode.Program _program;
            private readonly IntCode.Computer _computer;
            private Task _runTask;
            private readonly object _lock = new object();

            public Game(IDictionary<(long x, long y), TileType> initialTiles)
            {
                Tiles = initialTiles;
            }

            public Game(IntCode.Program program)
            {
                _program = program;
                _computer = new IntCode.Computer(_program);
            }

            public Task Start()
            {
                _computer.Start();
                _runTask = _computer.WaitUntilCompleted();
                var t = Task.Run(async () =>
                {
                    while (!_runTask.IsCompleted || _computer.Output.Any())
                    {
                        var x = await _computer.Output.Read();
                        var y = await _computer.Output.Read();
                        var z = await _computer.Output.Read();
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
                await _computer.WaitUntilInputRequired();
                //Console.Clear();
                //Console.WriteLine(this);
                //await Task.Delay(20);
                switch (mode)
                {
                    case Mode.Manual:
                        switch (Console.ReadKey().Key)
                        {
                            case ConsoleKey.LeftArrow:
                                await _computer.Input.Write(-1);
                                break;
                            case ConsoleKey.RightArrow:
                                await _computer.Input.Write(1);
                                break;
                            default:
                                await _computer.Input.Write(0);
                                break;
                        }

                        break;
                    case Mode.Automatic:
                        var bat = FindBat();
                        var ball = FindBall();
                        await _computer.Input.Write(ball.X.CompareTo(bat.X));
                        break;
                    case Mode.DoNothing:
                        await Task.Delay(20);
                        await _computer.Input.Write(0);
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
        
        private async Task<int> Part1()
        {
            var program = await IntCode.Program.Load("13/input");
            var game = new Game(program);
            await game.Start();
            return game.Tiles.Count(kvp => kvp.Value == TileType.Block);
        }

        private async Task<long> Part2()
        {
            var program = await IntCode.Program.Load("13/input");
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
