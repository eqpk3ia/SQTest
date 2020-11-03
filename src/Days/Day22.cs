using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AdventOfCode.Days
{
    [Day(2019, 22)]
    public class Day22 : BaseDay
    {
        public override string PartOne(string input)
        {
            var pos = 2019;
            var deckSize = 10007;

            pos = input.Lines().Aggregate(pos, (p, line) => Shuffle(line, p, deckSize));

            return pos.ToString();
        }

        public override string PartTwo(string input)
        {
            var deckSize = 119315717514047;
            var targetPos = 2020L;
            var shuffleCount = 101741582076661;

            BigInteger a = 1;
            BigInteger b = 0;

            (a, b) = input.Lines()
                          .Reverse()
                          .Aggregate((a, b), (constants, line) => ReverseShuffle(line, deckSize, constants.a, constants.b));

            (a, b) = RepeatShuffle(a, b, shuffleCount, deckSize);

            return ((a * targetPos + b) % deckSize).ToString();
        }

        private int Shuffle(string line, int pos, int deckSize)
        {
            if (line.StartsWith("deal with increment"))
            {
                return DealIncrement(pos, deckSize, int.Parse(line.Words().Last()));
            }

            if (line.StartsWith("deal into new stack"))
            {
                return DealNewStack(pos, deckSize);
            }

            if (line.StartsWith("cut"))
            {
                return CutDeck(pos, deckSize, int.Parse(line.Words().Last()));
            }

            throw new ArgumentException($"Unrecognized input: {line}");
        }

        private int CutDeck(int pos, int deckSize, int n)
        {
            return (pos - n + deckSize) % deckSize;
        }

        private int DealNewStack(int pos, int deckSize)
        {
            return deckSize - pos - 1;
        }

        private int DealIncrement(int pos, int deckSize, int n)
        {
            return (pos * n) % deckSize;
        }

        private (BigInteger a, BigInteger b) ReverseShuffle(string line, long deckSize, BigInteger a, BigInteger b)
        {
            if (line.StartsWith("deal with increment"))
            {
                return ReverseIncrement(long.Parse(line.Words().Last()), deckSize, a, b);
            }

            if (line.StartsWith("deal into new stack"))
            {
                return ReverseNewStack(deckSize, a, b);
            }

            if (line.StartsWith("cut"))
            {
                return ReverseCut(long.Parse(line.Words().Last()), deckSize, a, b);
            }

            throw new ArgumentException($"Unrecognized input: {line}");
        }

        private (BigInteger A, BigInteger B) ReverseCut(long n, long deckSize, BigInteger a, BigInteger b)
        {
            return (a, b + n);
        }

        private (BigInteger A, BigInteger B) ReverseNewStack(long deckSize, BigInteger a, BigInteger b)
        {
            return (-a, -b + deckSize - 1);
        }

        private (BigInteger A, BigInteger B) ReverseIncrement(long n, long deckSize, BigInteger a, BigInteger b)
        {
            var inverse = InverseMod(n, deckSize);

            return (a * inverse, b * inverse);
        }

        private BigInteger InverseMod(BigInteger n, BigInteger mod)
        {
            return BigInteger.ModPow(n, mod - 2, mod);
        }

        private (BigInteger A, BigInteger B) RepeatShuffle(BigInteger a, BigInteger b, BigInteger count, BigInteger deckSize)
        {
            var newA = BigInteger.ModPow(a, count, deckSize);
            var newB = b * (newA - 1) * InverseMod(a - 1, deckSize);

            return (newA, newB);
        }
    }
}
