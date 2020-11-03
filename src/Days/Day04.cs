using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 4)]
    public class Day04 : BaseDay
    {
        private static readonly int _start = 138241;
        private static readonly int _count = 674034 - 138241 + 1;
        private static readonly List<string> _passwords = Enumerable.Range(_start, _count).Select(x => x.ToString()).ToList();

        public override string PartOne(string input)
        {
            return _passwords.Count(x => CheckAdjacent(x) && CheckIncreasingDigits(x)).ToString();
        }

        public override string PartTwo(string input)
        {
            return _passwords.Count(x => CheckTwoAdjacent(x) && CheckIncreasingDigits(x)).ToString();
        }

        private bool CheckAdjacent(string pwd) => pwd.GroupBy(x => x).Any(g => g.Count() >= 2);

        private bool CheckTwoAdjacent(string pwd) => pwd.GroupBy(x => x).Any(g => g.Count() == 2);

        private bool CheckIncreasingDigits(string pwd) => pwd.OrderBy(c => c).SequenceEqual(pwd);
    }
}
