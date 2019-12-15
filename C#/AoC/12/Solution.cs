using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC._12
{
    public sealed class Solution : ISolution
    {
        public struct Vector
        {
            public int X { get; }

            public int Y { get; }

            public int Z { get; }

            public Vector(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public static implicit operator (int X, int Y, int Z)(Vector vector)
            {
                return (vector.X, vector.Y, vector.Z);
            }

            public static implicit operator Vector((int X, int Y, int Z) vector)
            {
                return new Vector(vector.X, vector.Y, vector.Z);
            }

            public static Vector operator +(Vector left, Vector right)
            {
                return new Vector(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
            }

            public static Vector operator -(Vector left, Vector right)
            {
                return new Vector(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
            }

            public int L1()
            {
                return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
            }

            public override bool Equals(object obj)
            {
                return (obj is Vector a && Equals(a)) || (obj?.GetType() == typeof((int X, int Y, int Z)) && Equals((Vector)obj));
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = X;
                    hashCode = (hashCode * 397) ^ Y;
                    hashCode = (hashCode * 397) ^ Z;
                    return hashCode;
                }
            }

            public bool Equals(Vector v)
            {
                return X == v.X && Y == v.Y && Z == v.Z;
            }

            public static Vector Parse(string s)
            {
                var regex = new Regex(@"<x=(?<x>\-?\d+), y=(?<y>\-?\d+), z=(?<z>\-?\d+)>");
                var result = regex.Match(s);
                if (result.Success)
                {
                    var x = int.Parse(result.Groups["x"].Value);
                    var y = int.Parse(result.Groups["y"].Value);
                    var z = int.Parse(result.Groups["z"].Value);
                    return (x, y, z);
                }
                throw new InvalidOperationException($"{s} is not a valid {nameof(Vector)}");
            }

            public override string ToString()
            {
                return $"<x={X}, y={Y}, z={Z}>";
            }
        }

        public sealed class Planet
        {
            public Vector Position { get; private set; }

            public Vector Velocity { get; private set; }

            public Planet(Vector position, Vector velocity)
            {
                Position = position;
                Velocity = velocity;
            }

            public int PotentialEnergy => Position.L1();
            public int KineticEnergy => Velocity.L1();
            public int TotalEnergy => PotentialEnergy * KineticEnergy;

            public void Accelerate(IEnumerable<Planet> planets)
            {
                foreach (var planet in planets)
                {
                    Accelerate(planet);
                }
            }

            private void Accelerate(Planet planet)
            {
                var dX = planet.Position.X.CompareTo(Position.X);
                var dY = planet.Position.Y.CompareTo(Position.Y);
                var dZ = planet.Position.Z.CompareTo(Position.Z);
                Velocity += (dX, dY, dZ);
            }

            public void Step()
            {
                Position += Velocity;
            }

            public override string ToString()
            {
                return $"pos={Position}, vel={Velocity}";
            }
        }

        private async Task<IReadOnlyCollection<Planet>> ParseInput()
        {
            return (await File.ReadAllLinesAsync("12/input"))
                   .Select(line => new Planet(Vector.Parse(line), (0, 0, 0))).ToList();
        }

        private async Task Iterate(IReadOnlyCollection<Planet> planets)
        {
            await Task.WhenAll(planets.Select(async planet =>
            {
                await Task.Yield();
                planet.Accelerate(planets.Except(new[] {planet}));
            }));
            await Task.WhenAll(planets.Select(async planet =>
            {
                await Task.Yield();
                planet.Step();
            }));
        }

        private async Task<int> Part1()
        {
            var planets = await ParseInput();
            for (var i = 0; i < 1000; i++)
            {
                await Iterate(planets);
                // Console.WriteLine($"After {i+1} steps:\n{string.Join("\n", planets)}\n");
            }

            return planets.Sum(p => p.TotalEnergy);
        }

        private static long GCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while(true)
            {
                long remainder = a % b;
                if (remainder == 0) return b;
                a = b;
                b = remainder;
            };
        }

        public static long LCM(long a, long b)
        {
            return a * b / GCD(a, b);
        }
        private async Task<long> Part2()
        {
            var planets = (await ParseInput()).ToList();

            var (pl1Init, pl2Init, pl3Init, pl4Init) = (planets[0], planets[1], planets[2], planets[3]);
            var (p1Init, p2Init, p3Init, p4Init) = (pl1Init.Position, pl2Init.Position, pl3Init.Position, pl4Init.Position);
            var (v1Init, v2Init, v3Init, v4Init) = (pl1Init.Velocity, pl2Init.Velocity, pl3Init.Velocity, pl4Init.Velocity);
            var xInit = (p1Init.X, v1Init.X, p2Init.X, v2Init.X, p3Init.X, v3Init.X, p4Init.X, v4Init.X);
            var yInit = (p1Init.Y, v1Init.Y, p2Init.Y, v2Init.Y, p3Init.Y, v3Init.Y, p4Init.Y, v4Init.Y);
            var zInit = (p1Init.Z, v1Init.Z, p2Init.Z, v2Init.Z, p3Init.Z, v3Init.Z, p4Init.Z, v4Init.Z);
            
            var count = 0;
            var (xCycle, yCycle, zCycle) = (-1, -1, -1);

            while (xCycle == -1 || yCycle == -1 || zCycle == -1)
            {
                count++;
                await Iterate(planets);

                var (pl1, pl2, pl3, pl4) = (planets[0], planets[1], planets[2], planets[3]);
                var (p1, p2, p3, p4) = (pl1.Position, pl2.Position, pl3.Position, pl4.Position);
                var (v1, v2, v3, v4) = (pl1.Velocity, pl2.Velocity, pl3.Velocity, pl4.Velocity);
                
                if (xCycle == -1)
                {
                    var xTuple = (p1.X, v1.X, p2.X, v2.X, p3.X, v3.X, p4.X, v4.X);
                    if (xInit.Equals(xTuple))
                    {
                        xCycle = count;
                    }
                }

                if (yCycle == -1)
                {
                    var yTuple = (p1.Y, v1.Y, p2.Y, v2.Y, p3.Y, v3.Y, p4.Y, v4.Y);
                    if (yInit.Equals(yTuple))
                    {
                        yCycle = count;
                    }
                }

                if (zCycle == -1)
                {
                    var zTuple = (p1.Z, v1.Z, p2.Z, v2.Z, p3.Z, v3.Z, p4.Z, v4.Z);
                    if (zInit.Equals(zTuple))
                    {
                        zCycle = count;
                    }
                }
            }

            return LCM(LCM(xCycle, yCycle), zCycle);
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
