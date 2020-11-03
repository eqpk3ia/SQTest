using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 23)]
    public class Day23 : BaseDay
    {
        public override string PartOne(string input)
        {
            var vms = InitializeVMs(input).ToList();

            while (true)
            {
                var nat = RunNetwork(vms);

                if (nat.HasValue)
                {
                    return nat.Value.y.ToString();
                }
            }
        }

        public override string PartTwo(string input)
        {
            var vms = InitializeVMs(input).ToList();
            (long x, long y)? previousNat = null;

            while (true)
            {
                var nat = RunNetwork(vms);

                if (IsNetworkIdle(vms, nat))
                {
                    vms[0].AddInput(nat.Value.x);
                    vms[0].AddInput(nat.Value.y);

                    if (previousNat.HasValue && previousNat.Value.y == nat.Value.y)
                    {
                        return nat.Value.y.ToString();
                    }

                    previousNat = nat;
                }
            }
        }

        private bool IsNetworkIdle(List<IntCodeVM> vms, (long x, long y)? nat) => !vms.Any(vm => vm.Inputs.Any()) && nat.HasValue;

        private (long x, long y)? RunNetwork(List<IntCodeVM> vms)
        {
            (long x, long y)? nat = null;

            foreach (var vm in vms)
            {
                vm.Outputs.Clear();
                var outputs = vm.Run();

                for (var i = 0; i < outputs.Count; i += 3)
                {
                    var address = (int)outputs[i];
                    var x = outputs[i + 1];
                    var y = outputs[i + 2];

                    if (address == 255)
                    {
                        nat = (x, y);
                    }
                    else
                    {
                        vms[address].AddInput(x);
                        vms[address].AddInput(y);
                    }
                }
            }

            return nat;
        }

        private IEnumerable<IntCodeVM> InitializeVMs(string input)
        {
            foreach (var i in Enumerable.Range(0, 50))
            {
                var vm = new IntCodeVM(input);
                vm.AddInput(i);

                yield return vm;
            }
        }

        public class IntCodeVM
        {
            private readonly List<long> _instructions;
            private List<long> _memory;
            private Dictionary<long, long> _sparseMemory;
            private int _ip = 0;
            public readonly List<long> Inputs = new List<long>();
            public readonly List<long> Outputs = new List<long>();
            private long _relativeBase = 0;
            public Func<IntCodeVM, long> InputFunction { get; set; }
            public Action<IntCodeVM, long> OutputFunction { get; set; }
            private bool _halted = false;
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

                Inputs.Clear();
                Outputs.Clear();

                _ip = 0;
            }

            public void AddInput(long input) => Inputs.Add(input);

            public void AddInputs(IEnumerable<long> inputs) => Inputs.AddRange(inputs);

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
                _halted = false;

                while (GetMemory(_ip) != 99 && !_halted)
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

                return Outputs;
            }

            public void Halt()
            {
                _halted = true;
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
                    SetMemory(a, InputFunction(this));
                }
                else
                {
                    if (Inputs.Any())
                    {
                        SetMemory(a, Inputs[0]);
                        Inputs.RemoveAt(0);
                    }
                    else
                    {
                        SetMemory(a, -1);
                        Halt();
                    }
                }
                return _ip += 2;
            }

            private int Output(int p1)
            {
                var a = GetParameter(1, p1);

                if (OutputFunction != null)
                {
                    OutputFunction(this, a);
                }
                else
                {
                    Outputs.Add(a);
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
