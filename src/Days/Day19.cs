using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 19)]
    public class Day19 : BaseDay
    {
        public override string PartOne(string input)
        {
            var beam = MapBeam(50, input);

            return beam.Sum(b => b.HasValue ? b.Value.end - b.Value.start + 1 : 0).ToString();
        }

        public override string PartTwo(string input)
        {
            var beam = MapBeam(2000, input);
            var result = FindShip(beam);

            return (result.X * 10000 + result.Y).ToString();
        }

        private bool CheckBeamCoords(int x, int y, IntCodeVM vm)
        {
            vm.Reset();
            vm.AddInput(x);
            vm.AddInput(y);

            return vm.Run()[0] > 0;
        }

        private List<(int start, int end)?> MapBeam(int rowCount, string program)
        {
            var vm = new IntCodeVM(program, true);

            var beam = new List<(int left, int right)?>(rowCount);
            beam.AddMany(null, rowCount);

            // these are hardcoded based on inspecting the puzzle input
            var left = 4;
            var right = 5;
            var startRow = 6;

            // Treat the first few rows special - they have either 0 or 1
            for (var y = 0; y < startRow; y++)
            {
                for (var x = 0; x <= right; x++)
                {
                    if (CheckBeamCoords(x, y, vm))
                    {
                        beam[y] = (x, x);
                    }
                }
            }
            
            for (var y = startRow; y < rowCount; y++)
            {
                if (!CheckBeamCoords(left, y, vm))
                {
                    left++;
                }

                if (CheckBeamCoords(right, y, vm))
                {
                    right++;
                }

                beam[y] = (left, right - 1);
            }

            return beam;
        }

        private Point FindShip(List<(int start, int end)?> beam)
        {
            var startRow = beam.SelectWithIndex()
                               .Where(r => r.item != null)
                               .Select(r => (r.index, r.item.Value.start, r.item.Value.end))
                               .First(r => (r.end - r.start + 1) >= 100)
                               .index;


            for (var y = startRow; y < beam.Count; y++)
            {
                for (var x = beam[y].Value.start; x <= beam[y].Value.end - 99; x++)
                {
                    if (IsShip(x, y, beam)) return new Point(x, y);
                }
            }

            throw new Exception("Ship not found");
        }

        private bool IsShip(int x, int y, List<(int start, int end)?> beam) => beam[y + 99].Value.start <= x;

        public class IntCodeVM
        {
            private readonly List<long> _instructions;
            private List<long> _memory;
            private Dictionary<long, long> _sparseMemory;
            private int _ip = 0;
            private List<long> _inputs = new List<long>();
            private List<long> _outputs = new List<long>();
            private long _relativeBase = 0;
            public Func<long> InputFunction { get; set; }
            public Action<long> OutputFunction { get; set; }
            private bool _halt = false;
            private bool _isSparseMemory;
            private int _memorySize;

            public IntCodeVM(string program, bool isSparseMemory = false, int memorySize = 1000000)
            {
                _instructions = program.Longs().ToList();
                _isSparseMemory = isSparseMemory;
                _memorySize = memorySize;
                Reset();
            }

            public void Reset()
            {
                if (_isSparseMemory)
                {
                    _sparseMemory = new Dictionary<long, long>();

                    for (var i = 0; i < _instructions.Count; i++)
                    {
                        _sparseMemory.Add((long)i, _instructions[i]);
                    }
                }
                else
                {
                    _memory = _instructions.Select(x => x).ToList();
                    _memory.AddMany(0, _memorySize);
                }

                _inputs = new List<long>();
                _outputs = new List<long>();

                _ip = 0;
            }

            public void AddInput(long input) => _inputs.Add(input);

            public void AddInputs(IEnumerable<long> inputs) => _inputs.AddRange(inputs);

            public void SetMemory(int address, long value)
            {
                if (_isSparseMemory)
                {
                    _sparseMemory.SafeSet(address, value);
                }
                else
                {
                    _memory[address] = value;
                }
            }

            public long GetMemory(int address)
            {
                if (_isSparseMemory)
                {
                    if (_sparseMemory.ContainsKey(address))
                    {
                        return _sparseMemory[address];
                    }

                    _sparseMemory.Add(address, 0);
                    return 0;
                }

                if (_memory.Count < (address - 1))
                {
                    var toAdd = (address - 1) - _memory.Count;
                    _memory.AddMany(0, toAdd);
                }

                return _memory[address];
            }

            public List<long> Run(params long[] inputs)
            {
                AddInputs(inputs);

                while (GetMemory(_ip) != 99 && !_halt)
                {
                    var (op, p1, p2, p3) = ParseOpCode(GetMemory(_ip));

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

                return _outputs;
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

                SetMemory(c, a + b);
                return _ip += 4;
            }

            private int Multiply(int p1, int p2, int p3)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = GetParameterAddress(3, p3);

                SetMemory(c, a * b);
                return _ip += 4;
            }

            private int Input(int p1)
            {
                var a = GetParameterAddress(1, p1);

                if (InputFunction != null)
                {
                    SetMemory(a, InputFunction());
                }
                else
                {
                    SetMemory(a, _inputs[0]);
                    _inputs.RemoveAt(0);
                }
                return _ip += 2;
            }

            private int Output(int p1)
            {
                var a = GetParameter(1, p1);

                if (OutputFunction != null)
                {
                    OutputFunction(a);
                }
                else
                {
                    _outputs.Add(a);
                }

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

                SetMemory(c, a < b ? 1 : 0);

                return _ip += 4;
            }

            private int EqualCheck(int p1, int p2, int p3)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = GetParameterAddress(3, p3);

                SetMemory(c, a == b ? 1 : 0);

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
                    0 => GetMemory((int)GetMemory(_ip + offset)),
                    1 => GetMemory(_ip + offset),
                    2 => GetMemory((int)GetMemory(_ip + offset) + (int)_relativeBase),
                    _ => throw new Exception("Invalid parameter mode")
                };
            }

            private int GetParameterAddress(int offset, int mode)
            {
                return mode switch
                {
                    0 => (int)GetMemory(_ip + offset),
                    2 => (int)(GetMemory(_ip + offset) + _relativeBase),
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
