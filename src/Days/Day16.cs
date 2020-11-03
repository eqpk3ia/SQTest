using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 16)]
    public class Day16 : BaseDay
    {
        private int[] _signal;

        public override string PartOne(string input)
        {
            _signal = input.Trim().Select(x => int.Parse(x.ToString())).ToArray();

            for (var p = 0; p < 100; p++)
            {
                _signal = ProcessPhase(_signal, false);
            }

            return $"{_signal[0]}{_signal[1]}{_signal[2]}{_signal[3]}{_signal[4]}{_signal[5]}{_signal[6]}{_signal[7]}";
        }

        public override string PartTwo(string input)
        {
            var signalRepeat = 10000;
            var baseSignal = input.Trim().Select(x => int.Parse(x.ToString())).ToArray();
            _signal = new int[baseSignal.Length * signalRepeat];

            for (var i = 0; i < signalRepeat; i++)
            {
                baseSignal.CopyTo(_signal, i * baseSignal.Length);
            }

            var messageLocation = int.Parse(string.Concat(input.Take(7)));

            for (var p = 0; p < 100; p++)
            {
                _signal = ProcessPhase(_signal, true);
            }

            var result = string.Concat(_signal.Skip(messageLocation).Take(8).Select(x => x.ToString()));

            return result;
        }

        private int[] ProcessPhase(int[] signal, bool skipEarlyPositions)
        {
            var result = new int[signal.Length];
            var sum = 0;
            var pos = (int)(signal.Length / 2);

            for (var i = signal.Length - 1; i >= pos; i--)
            {
                sum += signal[i];
                result[i] = sum % 10;
            }

            pos--;

            if (!skipEarlyPositions)
            {
                for (var i = 0; i <= pos; i++)
                {
                    result[i] = TransformElement(signal, i + 1);
                }
            }

            return result;
        }

        private int TransformElement(int[] signal, int position)
        {
            var sum = 0;
            var multiplier = -1;
            var stop = 0;

            while (stop < signal.Length)
            {
                multiplier += 2;
                stop = Math.Min((position * (multiplier + 1)) - 1, signal.Length);

                for (var i = (position * multiplier) - 1; i < stop; i++)
                {
                    sum += signal[i];
                }

                multiplier += 2;
                stop = Math.Min((position * (multiplier + 1)) - 1, signal.Length);

                for (var i = (position * multiplier) - 1; i < stop; i++)
                {
                    sum -= signal[i];
                }
            }

            return Math.Abs(sum) % 10;
        }
    }
}
