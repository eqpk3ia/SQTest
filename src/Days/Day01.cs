using System;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 1)]
    public class Day01 : BaseDay
    {
        public override string PartOne(string input)
        {
            return input.Doubles()
                        .Sum(x => CalcFuel(x))
                        .ToString();
        }

        public override string PartTwo(string input)
        {
            return input.Doubles()
                        .Sum(m => RecurseFuel(CalcFuel(m)))
                        .ToString();
        }

        private double CalcFuel(double m) => Math.Floor(m / 3) - 2;

        private double RecurseFuel(double m) => m > 0 ? m + RecurseFuel(CalcFuel(m)) : 0;
    }
}
