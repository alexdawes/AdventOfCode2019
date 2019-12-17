using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._11
{
    internal sealed class Solution : ISolution
    {
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

        private async Task<long> Part1()
        {
            var program = await IntCode.Program.Load("11/input");
            var computer = new IntCode.Computer(program);
            var canvas = new Canvas();
            await computer.Input.Write((int)Color.Black);
            computer.Start();
            var task = computer.WaitUntilCompleted();
            while (true)
            {
                var colorToPaint = (Color)(await computer.Output.Read());
                var rotation = (Rotation)(await computer.Output.Read());
                canvas.Iterate(colorToPaint, rotation);
                await computer.Input.Write((int)canvas.GetCurrentColor());
                if (task.IsCompleted)
                {
                    break;
                }
            }

            return canvas.GetPaintedPanelsCount();
        }
        
        private async Task<string> Part2()
        {
            var program = await IntCode.Program.Load("11/input");
            var computer = new IntCode.Computer(program);
            var canvas = new Canvas();
            await computer.Input.Write((int)Color.White);
            computer.Start();
            var task = computer.WaitUntilCompleted();
            while (true)
            {
                var colorToPaint = (Color)(await computer.Output.Read());
                var rotation = (Rotation)(await computer.Output.Read());
                canvas.Iterate(colorToPaint, rotation);
                await computer.Input.Write((int)canvas.GetCurrentColor());
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
