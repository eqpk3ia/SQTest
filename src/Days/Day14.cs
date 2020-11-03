using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 14)]
    public class Day14 : BaseDay
    {
        private Dictionary<string, Reaction> _reactions;
        private Dictionary<string, long> _chemicals = new Dictionary<string, long>();
        private readonly long _startOre = 1000000000000;

        public override string PartOne(string input)
        {
            InitializeData(input);
            MakeFuel(1);

            return (_startOre - _chemicals["ORE"]).ToString();
        }

        public override string PartTwo(string input)
        {
            InitializeData(input);

            var batchSize = 10000000;

            while (batchSize > 1)
            {
                Log($"Making FUEL in batches of {batchSize}...");
                while (MakeFuel(batchSize)) { };

                Log("Converting back to ORE...");
                ReverseReactions();

                batchSize /= 100;
            }

            Log($"Making FUEL in batches of 1...");
            while (MakeFuel(1)) { };

            return _chemicals["FUEL"].ToString();
        }

        private void InitializeData(string input)
        {
            _reactions = new Dictionary<string, Reaction>();
            input.Lines().Select(x => ParseReaction(x)).ForEach(x => _reactions.Add(x.Output, x));

            foreach (var r in _reactions)
            {
                _chemicals.Add(r.Key, 0);
            }

            _chemicals.Add("ORE", _startOre);
        }

        private void PerformReaction(Reaction reaction, long count)
        {
            _chemicals[reaction.Output] += reaction.Quantity * count;

            foreach (var (quantity, input) in reaction.Inputs)
            {
                _chemicals[input] -= quantity * count;
            }
        }

        private Reaction ParseReaction(string input)
        {
            var left = input.Split("=>")[0];
            var right = input.Split("=>")[1];

            var result = new Reaction
            {
                Output = right.Words().ElementAt(1),
                Quantity = long.Parse(right.Words().ElementAt(0))
            };

            var inputs = left.Words().ToList();

            for (var i = 0; i < inputs.Count; i += 2)
            {
                result.Inputs.Add((long.Parse(inputs[i]), inputs[i + 1]));
            }

            return result;
        }

        private bool MakeFuel(long batchSize)
        {
            while (true)
            {
                var reaction = FindReactionToPerform();

                if (reaction == null)
                {
                    return false;
                }

                var count = GetMaxReactions(reaction, batchSize);
                PerformReaction(reaction, count);

                if (reaction.Output == "FUEL")
                {
                    return true;
                }
            }
        }

        private Reaction FindReactionToPerform()
        {
            var useful = _reactions.Where(r => r.Key == "FUEL").Select(r => r.Value).ToList();

            while (useful.Any())
            {
                var result = useful.FirstOrDefault(u => u.Inputs.All(i => _chemicals[i.input] >= i.quantity));

                if (result != null)
                {
                    return result;
                }

                useful = useful.SelectMany(x => x.Inputs).Where(x => _chemicals[x.input] < x.quantity && x.input != "ORE").Select(x => _reactions[x.input]).ToList();
            }

            return null;
        }

        private void ReverseReactions()
        {
            var repeat = true;
            var chems = _chemicals.Where(x => x.Key != "FUEL" && x.Key != "ORE").Select(c => c.Key).ToList();

            while (repeat)
            {
                repeat = false;

                foreach (var c in chems)
                {
                    if (_chemicals[c] >= _reactions[c].Quantity)
                    {
                        var count = _chemicals[c] / _reactions[c].Quantity;
                        PerformReaction(_reactions[c], -count);
                        repeat = true;
                    }
                }
            }
        }

        private long GetMaxReactions(Reaction reaction, long max)
        {
            var result = max;

            foreach (var (quantity, input) in reaction.Inputs)
            {
                var count = _chemicals[input] / quantity;

                if (count < result)
                {
                    result = count;
                }
            }

            return result;
        }

        private class Reaction
        {
            public List<(long quantity, string input)> Inputs { get; set; } = new List<(long quantity, string input)>();
            public string Output { get; set; }
            public long Quantity { get; set; }
        }
    }
}
