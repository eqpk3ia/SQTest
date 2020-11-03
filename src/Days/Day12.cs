using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 12)]
    public class Day12 : BaseDay
    {
        private List<Moon> _moons;
        private List<List<Moon>> _combos;

        public override string PartOne(string input)
        {
            _moons = input.Lines().Select(x => new Moon(x)).ToList();
            _combos = GetMoonCombos();

            for (var i = 0; i < 1000; i++)
            {
                ProcessGravity();
            }

            return _moons.Sum(m => m.GetTotalEnergy()).ToString();
        }

        public override string PartTwo(string input)
        {
            var seen = new HashSet<(long, long, long, long, long, long, long, long)>[3];
            var steps = new long[3];

            seen[0] = new HashSet<(long, long, long, long, long, long, long, long)>();
            seen[1] = new HashSet<(long, long, long, long, long, long, long, long)>();
            seen[2] = new HashSet<(long, long, long, long, long, long, long, long)>();

            _moons = input.Lines().Select(x => new Moon(x)).ToList();
            _combos = GetMoonCombos();

            var foundX = false;
            var foundY = false;
            var foundZ = false;

            while (!foundX || !foundY || !foundZ)
            {
                if (!foundX)
                {
                    if (!seen[0].Contains(GetAllX()))
                    {
                        seen[0].Add(GetAllX());
                        steps[0]++;
                    }
                    else
                    {
                        foundX = true;
                    }
                }

                if (!foundY)
                {
                    if (!seen[1].Contains(GetAllY()))
                    {
                        seen[1].Add(GetAllY());
                        steps[1]++;
                    }
                    else
                    {
                        foundY = true;
                    }
                }

                if (!foundZ)
                {
                    if (!seen[2].Contains(GetAllZ()))
                    {
                        seen[2].Add(GetAllZ());
                        steps[2]++;
                    }
                    else
                    {
                        foundZ = true;
                    }
                }

                ProcessGravity();
            }

            return steps.LeastCommonMultiple().ToString();
        }

        private List<List<Moon>> GetMoonCombos()
        {
            return new List<List<Moon>>
            {
                new List<Moon>() { _moons[0], _moons[1] },
                new List<Moon>() { _moons[0], _moons[2] },
                new List<Moon>() { _moons[0], _moons[3] },
                new List<Moon>() { _moons[1], _moons[2] },
                new List<Moon>() { _moons[1], _moons[3] },
                new List<Moon>() { _moons[2], _moons[3] }
            };
        }

        private void ProcessGravity()
        {
            UpdateVelocity();

            foreach (var m in _moons)
            {
                m.Position += m.Velocity;
            }
        }

        private void UpdateVelocity()
        {
            foreach (var c in _combos)
            {
                if (c[0].Position.X > c[1].Position.X)
                {
                    c[0].Velocity.X--;
                    c[1].Velocity.X++;
                }

                if (c[0].Position.X < c[1].Position.X)
                {
                    c[0].Velocity.X++;
                    c[1].Velocity.X--;
                }

                if (c[0].Position.Y > c[1].Position.Y)
                {
                    c[0].Velocity.Y--;
                    c[1].Velocity.Y++;
                }

                if (c[0].Position.Y < c[1].Position.Y)
                {
                    c[0].Velocity.Y++;
                    c[1].Velocity.Y--;
                }

                if (c[0].Position.Z > c[1].Position.Z)
                {
                    c[0].Velocity.Z--;
                    c[1].Velocity.Z++;
                }

                if (c[0].Position.Z < c[1].Position.Z)
                {
                    c[0].Velocity.Z++;
                    c[1].Velocity.Z--;
                }
            }
        }

        private (long, long, long, long, long, long, long, long) GetAllX()
        {
            return (_moons[0].Position.X,
                    _moons[0].Velocity.X,
                    _moons[1].Position.X,
                    _moons[1].Velocity.X,
                    _moons[2].Position.X,
                    _moons[2].Velocity.X,
                    _moons[3].Position.X,
                    _moons[3].Velocity.X);
        }

        private (long, long, long, long, long, long, long, long) GetAllY()
        {
            return (_moons[0].Position.Y,
                    _moons[0].Velocity.Y,
                    _moons[1].Position.Y,
                    _moons[1].Velocity.Y,
                    _moons[2].Position.Y,
                    _moons[2].Velocity.Y,
                    _moons[3].Position.Y,
                    _moons[3].Velocity.Y);
        }

        private (long, long, long, long, long, long, long, long) GetAllZ()
        {
            return (_moons[0].Position.Z,
                    _moons[0].Velocity.Z,
                    _moons[1].Position.Z,
                    _moons[1].Velocity.Z,
                    _moons[2].Position.Z,
                    _moons[2].Velocity.Z,
                    _moons[3].Position.Z,
                    _moons[3].Velocity.Z);
        }

        private class Moon
        {
            public Point3D Position { get; set; }
            public Point3D Velocity { get; set; }

            public Moon(string input)
            {
                var a = input.Shave(1).Split(',', StringSplitOptions.RemoveEmptyEntries);

                Position = new Point3D();
                Velocity = new Point3D();

                Position.X = int.Parse(a[0].Trim().ShaveLeft(2));
                Position.Y = int.Parse(a[1].Trim().ShaveLeft(2));
                Position.Z = int.Parse(a[2].Trim().ShaveLeft(2));
            }

            public long GetTotalEnergy()
            {
                var potential = Math.Abs(Position.X) + Math.Abs(Position.Y) + Math.Abs(Position.Z);
                var kinetic = Math.Abs(Velocity.X) + Math.Abs(Velocity.Y) + Math.Abs(Velocity.Z);

                return potential * kinetic;
            }
        }
    }
}
