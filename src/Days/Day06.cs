using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 6)]
    public class Day06 : BaseDay
    {
        public override string PartOne(string input)
        {
            var planets = BuildTree(input);
            
            var children = planets.Children.AsEnumerable();
            var result = 0;
            var layer = 1;

            while (children?.Count() > 0)
            {
                result += children.Count() * layer;
                layer++;
                children = children.GetAllChildren();
            }

            return result.ToString();
        }

        private Tree<string> BuildTree(string input)
        {
            var lines = input.Lines().ToList();
            var planets = new Dictionary<string, Tree<string>>();

            foreach (var line in lines)
            {
                var left = line.Split(')').First();
                var right = line.Split(')').Last();

                if (!planets.ContainsKey(left))
                { 
                    planets.Add(left, new Tree<string>(left));
                }

                if (!planets.ContainsKey(right))
                {
                    planets.Add(right, new Tree<string>(right));
                }

                planets[right].Parent = planets[left];
                planets[left].Children.AddLast(planets[right]);
            }

            return planets["COM"];
        }

        public override string PartTwo(string input)
        {
            var planets = BuildTree(input);

            var startPlanet = planets.Single(p => p.Data == "YOU").Parent;
            var targetPlanet = planets.Single(p => p.Data == "SAN").Parent;

            return startPlanet.CalcDistance(targetPlanet).ToString();
        }
    }
}
