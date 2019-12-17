using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IntCode = AoC.Utils.IntCode;

namespace AoC._15
{
    internal sealed class Solution : ISolution
    {
        public enum Direction
        {
            North = 1,
            South = 2,
            West = 3,
            East = 4
        }

        public static readonly Direction[] Directions = new[]
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        public enum TileType
        {
            Unknown = 0,
            Empty = 1,
            Wall = 2,
            Target = 3
        }

        public enum Response
        {
            Blocked = 0,
            Success = 1,
            SuccessAndFinished = 2
        }

        public static (int X, int Y) GetNext((int X, int Y) current, Direction direction)
        {
            var (x, y) = current;
            switch (direction)
            {
                case Direction.North: return (x, y - 1);
                case Direction.East: return (x + 1, y);
                case Direction.South: return (x, y + 1);
                case Direction.West: return (x - 1, y);
                default: throw new InvalidOperationException($"Invalid {nameof(Direction)}: {direction}");
            }
        }

        public static Direction GetOpposite(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return Direction.South;
                case Direction.East: return Direction.West;
                case Direction.South: return Direction.North;
                case Direction.West: return Direction.East;
                default: throw new InvalidOperationException($"Invalid {nameof(Direction)}: {direction}");
            }
        }
        
        public sealed class Map
        {
            public IDictionary<(int X, int Y), TileType> Tiles { get; }

            public Map(IDictionary<(int X, int Y), TileType> initial)
            {
                Tiles = initial;
            }

            public TileType GetType((int X, int Y) coord)
            {
                return Tiles.TryGetValue(coord, out TileType result) ? result : TileType.Unknown;
            }

            public void SetType((int X, int Y) coord, TileType type)
            {
                Tiles[coord] = type;
            }

            public List<Direction> GetPath((int X, int Y) start, (int X, int Y) end)
            {
                var explored = new HashSet<(int X, int Y)> {start};
                var toProcess = new HashSet<(int X, int Y)> {start};
                var paths = new Dictionary<(int X, int Y), List<Direction>> {{ start, new List<Direction>() }};

                while (!explored.Contains(end))
                {
                    var toProcessNext = new HashSet<(int X, int Y)>();
                    foreach (var coord in toProcess)
                    {
                        foreach (var direction in Directions)
                        {
                            var next = GetNext(coord, direction);
                            if (!explored.Contains(next))
                            {
                                var type = GetType(next);
                                if (type == TileType.Empty || type == TileType.Target)
                                {
                                    toProcessNext.Add(next);
                                    explored.Add(next);
                                    paths[next] = paths[coord].Concat(new[] {direction}).ToList();
                                }
                            }
                        }
                    }

                    toProcess = toProcessNext;
                }

                return paths[end];
            }
        }

        public sealed class Robot
        {
            public (int X, int Y) Position { get; private set; }

            public Robot((int X, int Y) position)
            {
                Position = position;
            }

            public void Move(Direction direction)
            {
                Position = GetNext(Position, direction);
            }
        }

        public sealed class Ship
        {
            private readonly IntCode.Program _program;
            public IntCode.Computer Computer { get; }
            public Robot Robot { get; }

            public Ship(IntCode.Program program)
            {
                _program = program;
                Computer = new IntCode.Computer(_program);
                Computer.Start();
                Robot = new Robot((0,0));
            }

            public async Task<Response> MoveRobot(Direction direction)
            {
                await Computer.Input.Write((long)direction);
                var result = (Response)await Computer.Output.Read();
                if (result == Response.Success || result == Response.SuccessAndFinished)
                {
                    Robot.Move(direction);
                }

                return result;
            }
        }

        private void Draw(Map map, Robot robot)
        {
            var (x1, x2, y1, y2) = (map.Tiles.Keys.Min(k => k.X), map.Tiles.Keys.Max(k => k.X),
                                    map.Tiles.Keys.Min(k => k.Y), map.Tiles.Keys.Max(k => k.Y));

            var result = new StringBuilder();
            for (var y = y1 - 2; y <= y2 + 2; y++)
            {
                var line = new StringBuilder();
                for (var x = x1 - 2; x <= x2 + 2; x++)
                {
                    if ((x, y).Equals(robot.Position))
                    {
                        line.Append("O");
                    }
                    else
                    {
                        var type = map.GetType((x, y));
                        switch (type)
                        {
                            case TileType.Wall:
                                line.Append("#");
                                break;
                            case TileType.Empty:
                                line.Append(".");
                                break;
                            default:
                                line.Append(" ");
                                break;
                        }
                    }
                }

                result.AppendLine(line.ToString());
            }

            Thread.Sleep(10);
            Console.Clear();
            Console.WriteLine($"\n{result}\n");
        }
        
        private async Task<long> Part1()
        {
            var program = await IntCode.Program.Load("15/input");
            var ship = new Ship(program);
            var map = new Map(new Dictionary<(int X, int Y), TileType> {{ ship.Robot.Position, TileType.Empty }});

            try
            {
                var explored = new HashSet<(int X, int Y)> {(0, 0)};
                var distances = new Dictionary<(int X, int Y), int> {{(0, 0), 0}};

                Draw(map, ship.Robot);
                while (true)
                {
                    var maxDistance = distances.Values.Max();
                    var nextToProcess = distances.Keys.Where(k => distances[k] == maxDistance).ToList();
                    foreach (var coord in nextToProcess)
                    {
                        var path = map.GetPath(ship.Robot.Position, coord);
                        foreach (var direction in path)
                        {
                            await ship.MoveRobot(direction);
                        }

                        foreach (var direction in Directions)
                        {
                            var next = GetNext(coord, direction);
                            if (!explored.Contains(next))
                            {
                                var response = await ship.MoveRobot(direction);
                                if (response == Response.SuccessAndFinished)
                                {
                                    Draw(map, ship.Robot);
                                    return maxDistance + 1;
                                }
                                else if (response == Response.Success)
                                {
                                    Draw(map, ship.Robot);
                                    await ship.MoveRobot(GetOpposite(direction));
                                    Draw(map, ship.Robot);
                                    distances[next] = maxDistance + 1;
                                    explored.Add(next);
                                    map.SetType(next, TileType.Empty);
                                }
                                else if (response == Response.Blocked)
                                {
                                    map.SetType(next, TileType.Wall);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                ship.Computer.Stop();
            }
        }

        private async Task<int> Part2()
        {
            var program = await IntCode.Program.Load("15/input");
            
            var ship = new Ship(program);
            var map = new Map(new Dictionary<(int X, int Y), TileType> { { ship.Robot.Position, TileType.Empty } });

            try
            {
                var explored = new HashSet<(int X, int Y)> {(0, 0)};
                var distances = new Dictionary<(int X, int Y), int> {{(0, 0), 0}};


                var previousExploredCount = 0;
                //Draw(map, ship.Robot);
                while (explored.Count != previousExploredCount)
                {
                    previousExploredCount = explored.Count;
                    var maxDistance = distances.Values.Max();
                    var nextToProcess = distances.Keys.Where(k => distances[k] == maxDistance).ToList();
                    foreach (var coord in nextToProcess)
                    {
                        var path = map.GetPath(ship.Robot.Position, coord);
                        foreach (var direction in path)
                        {
                            await ship.MoveRobot(direction);
                        }

                        foreach (var direction in Directions)
                        {
                            var next = GetNext(coord, direction);
                            if (!explored.Contains(next))
                            {
                                var response = await ship.MoveRobot(direction);
                                if (response == Response.SuccessAndFinished)
                                {
                                    //Draw(map, ship.Robot);
                                    await ship.MoveRobot(GetOpposite(direction));
                                    //Draw(map, ship.Robot);
                                    distances[next] = maxDistance + 1;
                                    explored.Add(next);
                                    map.SetType(next, TileType.Target);
                                }
                                else if (response == Response.Success)
                                {
                                    //Draw(map, ship.Robot);
                                    await ship.MoveRobot(GetOpposite(direction));
                                    //Draw(map, ship.Robot);
                                    distances[next] = maxDistance + 1;
                                    explored.Add(next);
                                    map.SetType(next, TileType.Empty);
                                }
                                else if (response == Response.Blocked)
                                {
                                    map.SetType(next, TileType.Wall);
                                }
                            }
                        }
                    }

                }
            }
            finally
            {
                ship.Computer.Stop();
            }

            var target = map.Tiles.First(kvp => kvp.Value == TileType.Target).Key;
            var others = map.Tiles.Where(kvp => kvp.Value == TileType.Empty).Select(kvp => kvp.Key).ToList();

            return others.Max(o => map.GetPath(o, target).Count);

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
