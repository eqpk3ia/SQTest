using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 11)]
    public class Day11 : BaseDay
    {
        private Dictionary<Point, bool> _panels = new Dictionary<Point, bool>();
        private Point _pos = new Point(0, 0);
        private Direction _dir = Direction.Up;
        private IntCodeVM _vm;

        public override string PartOne(string input)
        {
            _vm = new IntCodeVM(input)
            {
                InputFunction = GetInput,
                OutputFunction = PaintPanel
            };

            _vm.Run();

            return _panels.Count.ToString();
        }

        public override string PartTwo(string input)
        {
            _vm = new IntCodeVM(input)
            {
                InputFunction = GetInput,
                OutputFunction = PaintPanel
            };

            _panels.Add(_pos, true);

            _vm.Run();

            var width = _panels.Max(p => p.Key.X) - _panels.Min(p => p.Key.X) + 1;
            var height = _panels.Max(p => p.Key.Y) - _panels.Min(p => p.Key.Y) + 1;

            ImageHelper.CreateBitmap(width, height, @"C:\AdventOfCode\Day11.bmp", GetPixel);

            return @"C:\AdventOfCode\Day11.bmp";
        }

        private long GetInput()
        {
            if (_panels.ContainsKey(_pos))
            {
                return _panels[_pos] ? 1 : 0;
            }

            return 0;
        }

        private void PaintPanel(long color)
        {
            _panels.SafeSet(_pos, color == 1);

            _vm.OutputFunction = MoveRobot;
        }

        private void MoveRobot(long turn)
        {
            _dir = turn == 0 ? _dir.TurnLeft() : _dir.TurnRight();
            _pos = _pos.Move(_dir);

            _vm.OutputFunction = PaintPanel;
        }

        private Color GetPixel(int x, int y)
        {
            var panelX = _panels.Min(p => p.Key.X) + x;
            var panelY = _panels.Max(p => p.Key.Y) - y;

            var panelPos = new Point(panelX, panelY);

            if (_panels.ContainsKey(panelPos))
            {
                return _panels[panelPos] ? Color.White : Color.Black;
            }

            return Color.Black;
        }

        public class IntCodeVM
        {
            private readonly List<long> _instructions;
            private List<long> _memory;
            private int _ip = 0;
            private List<long> _inputs = new List<long>();
            private long _relativeBase = 0;
            public List<long> Outputs { get; set; } = new List<long>();
            public Func<long> InputFunction { get; set; }
            public Action<long> OutputFunction { get; set; }


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

            public IEnumerable<long> Run(params long[] inputs)
            {
                AddInputs(inputs);

                while (_memory[_ip] != 99)
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

                return Outputs;
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
