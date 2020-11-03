using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 5)]
    public class Day05 : BaseDay
    {
        public override string PartOne(string input)
        {
            var vm = new IntCodeVM(input);
            return vm.Run(1).Last().ToString();
        }

        public override string PartTwo(string input)
        {
            var vm = new IntCodeVM(input);
            return vm.Run(5).Last().ToString();
        }

        public class IntCodeVM
        {
            private readonly List<int> _instructions;
            private List<int> _memory;
            private int _ip = 0;
            private List<int> _inputs;
            private List<int> _outputs = new List<int>();

            public IntCodeVM(string program)
            {
                _instructions = program.Integers().ToList();
                _memory = _instructions.Select(x => x).ToList();
            }

            public void Reset()
            {
                _memory = _instructions.Select(x => x).ToList();
                _ip = 0;
            }

            public void SetMemory(int address, int value) => _memory[address] = value;

            public IEnumerable<int> Run(params int[] inputs)
            {
                _inputs = inputs.ToList();

                while (_memory[_ip] != 99)
                {
                    var (op, p1, p2) = ParseOpCode(_memory[_ip]);
                    
                    _ = op switch
                    {
                        1 => Add(p1, p2),
                        2 => Multiply(p1, p2),
                        3 => Input(),
                        4 => Output(p1),
                        5 => JumpIfNotZero(p1, p2),
                        6 => JumpIfZero(p1, p2),
                        7 => LessThan(p1, p2),
                        8 => EqualCheck(p1, p2),
                        _ => throw new Exception($"Invalid op code [{op}]")
                    };
                }

                return _outputs;
            }

            private int Add(int p1, int p2)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = _memory[_ip + 3];

                _memory[c] = a + b;
                return _ip += 4;
            }

            private int Multiply(int p1, int p2)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = _memory[_ip + 3];

                _memory[c] = a * b;
                return _ip += 4;
            }

            private int Input()
            {
                var a = _memory[_ip + 1];

                _memory[a] = _inputs.First();
                _inputs.RemoveAt(0);
                return _ip += 2;
            }

            private int Output(int p1)
            {
                var a = GetParameter(1, p1);

                _outputs.Add(a);
                return _ip += 2;
            }

            private int JumpIfNotZero(int p1, int p2)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);

                _ip = a != 0 ? b : _ip + 3;
                return _ip;
            }

            private int JumpIfZero(int p1, int p2)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);

                _ip = a == 0 ? b : _ip + 3;
                return _ip;
            }

            private int LessThan(int p1, int p2)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = _memory[_ip + 3];

                _memory[c] = a < b ? 1 : 0;

                return _ip += 4;
            }

            private int EqualCheck(int p1, int p2)
            {
                var a = GetParameter(1, p1);
                var b = GetParameter(2, p2);
                var c = _memory[_ip + 3];

                _memory[c] = a == b ? 1 : 0;

                return _ip += 4;
            }

            private int GetParameter(int offset, int mode) => mode == 0 ? _memory[_memory[_ip + offset]] : _memory[_ip + offset];

            private (int op, int p1, int p2) ParseOpCode(int input)
            {
                var op = input % 100;
                var p1 = input % 1000 / 100;
                var p2 = input % 10000 / 1000;

                return (op, p1, p2);
            }
        }
    }
}
