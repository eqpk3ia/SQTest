using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 9)]
    public class Day09 : BaseDay
    {
        public override string PartOne(string input)
        {
            var vm = new IntCodeVM(input);
            var output = vm.Run(1);

            return output.Last().ToString();
        }

        public override string PartTwo(string input)
        {
            var vm = new IntCodeVM(input);
            var output = vm.Run(2);

            return output.Last().ToString();
        }

        public class IntCodeVM
        {
            private readonly List<long> _instructions;
            private List<long> _memory;
            private int _ip = 0;
            private List<long> _inputs = new List<long>();
            private long _relativeBase = 0;
            public List<long> Outputs { get; set; } = new List<long>();
            public bool Halted { get; set; }

            public IntCodeVM OutputVM { get; set; }

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

                Halted = true;

                if (OutputVM != null && !OutputVM.Halted)
                {
                    OutputVM.Run();
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

                if (_inputs.Any())
                {
                    _memory[a] = _inputs.First();
                    _inputs.RemoveAt(0);
                    return _ip += 2;
                }

                OutputVM.Run();
                return _ip;
            }

            private int Output(int p1)
            {
                var a = GetParameter(1, p1);

                Outputs.Add(a);

                if (OutputVM != null)
                {
                    OutputVM.AddInput(a);
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
