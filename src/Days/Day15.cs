using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 15)]
    public class Day15 : BaseDay
    {
        private IntCodeVM _vm;
        private Point _bot = new Point(0, 0);
        private HashSet<Point> _walls = new HashSet<Point>();
        private HashSet<Point> _open = new HashSet<Point>();
        private Point? _oxygen = null;
        private Point _next;
        private List<Direction> _currentPath;

        public override string PartOne(string input)
        {
            ExploreMap(input);
            return FindShortestPath(new Point(0, 0), _oxygen.Value).Count.ToString();
        }

        public override string PartTwo(string input)
        {
            ExploreMap(input);

            var oxygens = new HashSet<Point>();
            var minutes = 0;

            oxygens.Add(_oxygen.Value);

            while (oxygens.Count < _open.Count)
            {
                var newOxygens = new HashSet<Point>();

                foreach (var o in oxygens)
                {
                    newOxygens.AddRange(o.GetNeighbors(false).Where(n => _open.Contains(n)).ToList());
                }

                oxygens.AddRange(newOxygens);
                minutes++;
            }

            return minutes.ToString();
        }

        private void ExploreMap(string input)
        {
            _vm = new IntCodeVM(input)
            {
                InputFunction = BotInput,
                OutputFunction = BotOutput
            };

            _open.Add(_bot);

            _vm.Run();
        }

        private string PrintMap()
        {
            var minX = Math.Min(_walls.Min(x => x.X), _open.Min(x => x.X));
            var minY = Math.Min(_walls.Min(x => x.Y), _open.Min(x => x.Y));
            var maxX = Math.Max(_walls.Max(x => x.X), _open.Max(x => x.X));
            var maxY = Math.Max(_walls.Max(x => x.Y), _open.Max(x => x.Y));

            var width = maxX - minX + 1;
            var height = maxY - minY + 1;

            var grid = new char[width, height];

            grid.Replace(default(char), '_');

            foreach (var w in _walls)
            {
                grid[w.X - minX, w.Y - minY] = '#';
            }

            foreach (var s in _open)
            {
                grid[s.X - minX, s.Y - minY] = '.';
            }

            grid[_oxygen.Value.X - minX, _oxygen.Value.Y - minY] = 'O';
            grid[-minX, -minY] = 'B';

            return grid.GetString();
        }

        private void BotOutput(long output)
        {
            if (output == 0)
            {
                _walls.Add(_next);
                _currentPath = null;
            }

            if (output == 1)
            {
                _bot = _next;
                _open.Add(_bot);
            }

            if (output == 2)
            {
                _bot = _next;
                _oxygen = _next;
                _open.Add(_bot);
            }
        }

        private long BotInput()
        {
            if (_currentPath == null || _currentPath.Count == 0)
            {
                _currentPath = FindPathToExplore();

                if (_currentPath == null)
                {
                    _vm.Halt();
                    return 0;
                }
            }

            _next = _bot.Move(_currentPath[0]);
            var result = DirectionToNumber(_currentPath[0]);
            _currentPath.RemoveAt(0);

            return result;
        }

        private long DirectionToNumber(Direction dir)
        {
            return dir switch
            {
                Direction.Up => 1,
                Direction.Down => 2,
                Direction.Left => 3,
                Direction.Right => 4,
                _ => throw new ArgumentException()
            };
        }

        private List<Direction> FindShortestPath(Point start, Point end)
        {
            var seen = new HashSet<Point>();
            var paths = new List<(List<Direction> path, Point location)>();

            paths.Add((new List<Direction>(), start));
            seen.Add(start);

            while (!seen.Any(s => s == end))
            {
                var oldPaths = paths.Select(p => p).ToList();
                paths = new List<(List<Direction> path, Point location)>();

                foreach (var path in oldPaths)
                {
                    foreach (Direction move in Enum.GetValues(typeof(Direction)))
                    {
                        var location = path.location.Move(move);

                        if (!_walls.Contains(location) && !seen.Contains(location))
                        {
                            var newPath = path.path.Select(p => p).ToList();
                            newPath.Add(move);

                            paths.Add((newPath, location));
                            seen.Add(location);
                        }
                    }
                }
            }

            return paths.First(p => p.location == end).path;
        }

        private List<Direction> FindPathToExplore()
        {
            var seen = new HashSet<Point>();
            var paths = new List<(List<Direction> path, Point location)>();

            paths.Add((new List<Direction>(), _bot));

            while (!seen.Any(s => !_open.Contains(s)) && paths.Count > 0)
            {
                paths = IncrementPaths(paths, seen);
            }

            return paths.FirstOrDefault(p => !_open.Contains(p.location)).path;
        }

        private List<(List<Direction> path, Point location)> IncrementPaths(List<(List<Direction> path, Point location)> paths, HashSet<Point> seen)
        {
            var newPaths = new List<(List<Direction> path, Point location)>();

            foreach (var path in paths)
            {
                foreach (Direction move in Enum.GetValues(typeof(Direction)))
                {
                    var location = path.location.Move(move);

                    if (!_walls.Contains(location) && !seen.Contains(location))
                    {
                        var newPath = path.path.Select(p => p).ToList();
                        newPath.Add(move);

                        newPaths.Add((newPath, location));
                        seen.Add(location);
                    }
                }
            }

            return newPaths;
        }

        public class IntCodeVM
        {
            private readonly List<long> _instructions;
            private List<long> _memory;
            private int _ip = 0;
            private List<long> _inputs = new List<long>();
            private long _relativeBase = 0;
            public Func<long> InputFunction { get; set; }
            public Action<long> OutputFunction { get; set; }
            private bool _halt = false;

            public IntCodeVM(string program)
            {
                _instructions = program.Longs().ToList();
                Reset();
            }

            public void Reset()
            {
                _memory = _instructions.Select(x => x).ToList();
                _memory.AddMany(0, 1000000);
                _ip = 0;
            }

            public void AddInput(long input) => _inputs.Add(input);

            public void AddInputs(IEnumerable<long> inputs) => _inputs.AddRange(inputs);

            public void SetMemory(int address, long value) => _memory[address] = value;

            public long GetMemory(int address)
            {
                if (_memory.Count < (address - 1))
                {
                    var toAdd = (address - 1) - _memory.Count;
                    _memory.AddMany(0, toAdd);
                }

                return _memory[address];
            }

            public void Run(params long[] inputs)
            {
                AddInputs(inputs);

                while (_memory[_ip] != 99 && !_halt)
                {
                    var (op, p1, p2, p3) = ParseOpCode(_memory[_ip]);

                    _ = op switch
                    {
                        1 => Add(p1, p2, p3),
                        2 => Multiply(p1, p2, p3),
                        3 => Input(p1),
                        4 => Output(p1),
                        5 => JumpIfNotZero(p1, p2),
                        6 => JumpIfZero(p1, p2),
                        7 => LessThan(p1, p2, p3),
                        8 => EqualCheck(p1, p2, p3),
                        9 => AdjustRelativeBase(p1),
                        _ => throw new Exception($"Invalid op code [{op}]")
                    };
                }
            }

            public void Halt()
            {
                _halt = true;
            }

            private int Add(int p1, int p2, int p3)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = GetParameterAddress(3, p3);

                _memory[c] = a + b;
                return _ip += 4;
            }

            private int Multiply(int p1, int p2, int p3)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = GetParameterAddress(3, p3);

                _memory[c] = a * b;
                return _ip += 4;
            }

            private int Input(int p1)
            {
                var a = GetParameterAddress(1, p1);

                _memory[a] = InputFunction();
                return _ip += 2;
            }

            private int Output(int p1)
            {
                var a = GetParameter(1, p1);

                OutputFunction(a);

                return _ip += 2;
            }

            private int JumpIfNotZero(int p1, int p2)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);

                _ip = a != 0 ? (int)b : _ip + 3;
                return _ip;
            }

            private int JumpIfZero(int p1, int p2)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);

                _ip = a == 0 ? (int)b : _ip + 3;
                return _ip;
            }

            private int LessThan(int p1, int p2, int p3)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = GetParameterAddress(3, p3);

                _memory[c] = a < b ? 1 : 0;

                return _ip += 4;
            }

            private int EqualCheck(int p1, int p2, int p3)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = GetParameterAddress(3, p3);

                _memory[c] = a == b ? 1 : 0;

                return _ip += 4;
            }

            private int AdjustRelativeBase(int p1)
            {
                var a = GetParameter(1, p1);

                _relativeBase += a;

                return _ip += 2;
            }

            private long GetParameter(int offset, int mode)
            {
                return mode switch
                {
                    0 => _memory[(int)_memory[_ip + offset]],
                    1 => _memory[_ip + offset],
                    2 => _memory[(int)(_memory[_ip + offset] + _relativeBase)],
                    _ => throw new Exception("Invalid parameter mode")
                };
            }

            private int GetParameterAddress(int offset, int mode)
            {
                return mode switch
                {
                    0 => (int)_memory[_ip + offset],
                    2 => (int)(_memory[_ip + offset] + _relativeBase),
                    _ => throw new Exception("Invalid parameter mode")
                };
            }

            private (int op, int p1, int p2, int p3) ParseOpCode(long input)
            {
                var op = input % 100;
                var p1 = input % 1000 / 100;
                var p2 = input % 10000 / 1000;
                var p3 = input % 100000 / 10000;

                return ((int)op, (int)p1, (int)p2, (int)p3);
            }
        }
    }
}
