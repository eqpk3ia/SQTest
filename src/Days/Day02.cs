using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 2)]
    public class Day02 : BaseDay
    {
        public override string PartOne(string input)
        {
            var vm = new IntCodeVM(input);

            vm.SetMemory(1, 12);
            vm.SetMemory(2, 2);

            return vm.Run().ToString();
        }

        public override string PartTwo(string input)
        {
            var vm = new IntCodeVM(input);

            for (var noun = 0; noun <= 99; noun++)
            {
                for (var verb = 0; verb <= 99; verb++)
                {
                    vm.SetMemory(1, noun);
                    vm.SetMemory(2, verb);

                    if (vm.Run() == 19690720)
                    {
                        return (100 * noun + verb).ToString();
                    }

                    vm.Reset();
                }
            }

            throw new Exception();
        }

        public class IntCodeVM
        {
            private readonly List<int> _instructions;
            private List<int> _memory;
            private int _ip = 0;

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

            public int Run()
            {
                while (_memory[_ip] != 99)
                {
                    var op = _memory[_ip];
                    var a = _memory[_memory[_ip + 1]];
                    var b = _memory[_memory[_ip + 2]];
                    var c = _memory[_ip + 3];

                    switch (op)
                    {
                        case 1:
                            _memory[c] = a + b;
                            break;
                        case 2:
                            _memory[c] = a * b;
                            break;
                        default:
                            throw new Exception($"Invalid op code [{op}]");
                    }

                    _ip += 4;
                }

                return _memory[0];
            }
        }
    }
}
