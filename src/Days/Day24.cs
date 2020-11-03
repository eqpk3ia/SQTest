using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 24)]
    public class Day24 : BaseDay
    {
        public override string PartOne(string input)
        {
            var grid = input.CreateCharGrid();

            var diversity = GetBiodiversityRating(grid);

            var seen = new HashSet<int>();

            while (!seen.Contains(diversity))
            {
                seen.Add(diversity);

                grid = NextMinute(grid);
                diversity = GetBiodiversityRating(grid);
            }

            return diversity.ToString();
        }

        private char[,] NextMinute(char[,] grid)
        {
            var result = grid.Clone(c => c);

            foreach (var p in grid.GetPoints())
            {
                var bugCount = grid.GetNeighbors(p.X, p.Y, false).Count(c => c == '#');

                if (grid[p.X, p.Y] == '#')
                {
                    result[p.X, p.Y] = bugCount == 1 ? '#' : '.';
                }

                if (grid[p.X, p.Y] == '.')
                {
                    result[p.X, p.Y] = bugCount == 1 || bugCount == 2 ? '#' : '.';
                }
            }

            return result;
        }

        private int GetBiodiversityRating(char[,] grid)
        {
            return (int)grid.GetPoints('#').Sum(p => Math.Pow(2, (p.X) + (p.Y * 5)));
        }

        public override string PartTwo(string input)
        {
            var map = new Dictionary<int, HashSet<Point>>();

            map.Add(0, new HashSet<Point>());

            input.CreateCharGrid()
                 .GetPoints('#')
                 .ForEach(p => map[0].Add(p));

            foreach (var layer in map)
            {
                Log($"Layer {layer.Key}");
                foreach (var bug in layer.Value)
                {
                    Log($"BUG: {bug.X}, {bug.Y}");
                }
            }

            foreach (var m in Enumerable.Range(1, 200))
            {
                var newMap = new Dictionary<int, HashSet<Point>>();

                foreach (var layer in map)
                {
                    if (!newMap.ContainsKey(layer.Key))
                    {
                        newMap.Add(layer.Key, new HashSet<Point>());
                    }

                    foreach (var bug in layer.Value)
                    {
                        var adj = GetAdjacentPoints(bug, layer.Key);

                        foreach (var (l, p) in adj)
                        {
                            if (IsBug(map, l, p))
                            {
                                if (!newMap.ContainsKey(l))
                                {
                                    newMap.Add(l, new HashSet<Point>());
                                }

                                if (!newMap[l].Contains(p))
                                {
                                    newMap[l].Add(p);
                                }
                            }
                        }
                    }
                }

                map = newMap;
            }

            foreach (var layer in map)
            {
                Log($"Layer {layer.Key}");
                foreach (var bug in layer.Value)
                {
                    Log($"BUG: {bug.X}, {bug.Y}");
                }
            }

            return map.Sum(l => l.Value.Count).ToString();
        }

        private bool IsBug(Dictionary<int, HashSet<Point>> map, int layer, Point point)
        {
            var adj = GetAdjacentPoints(point, layer);
            var bugCount = adj.Count(a => map.ContainsKey(a.layer) && map[a.layer].Contains(a.point));

            if (map.ContainsKey(layer) && map[layer].Contains(point))
            {
                return bugCount == 1;
            }

            return bugCount == 1 || bugCount == 2;
        }

        private List<(int layer, Point point)> GetAdjacentPoints(Point bug, int layer)
        {
            var neighbors = bug.GetNeighbors(false).ToList();
            var result = new List<(int layer, Point p)>();

            foreach (var n in neighbors)
            {
                if (n.X == 2 && n.Y == 2)
                {
                    if (bug.Y == 1)
                    {
                        result.Add((layer + 1, new Point(0, 0)));
                        result.Add((layer + 1, new Point(1, 0)));
                        result.Add((layer + 1, new Point(2, 0)));
                        result.Add((layer + 1, new Point(3, 0)));
                        result.Add((layer + 1, new Point(4, 0)));
                    }

                    if (bug.X == 1)
                    {
                        result.Add((layer + 1, new Point(0, 0)));
                        result.Add((layer + 1, new Point(0, 1)));
                        result.Add((layer + 1, new Point(0, 2)));
                        result.Add((layer + 1, new Point(0, 3)));
                        result.Add((layer + 1, new Point(0, 4)));
                    }

                    if (bug.X == 3)
                    {
                        result.Add((layer + 1, new Point(4, 0)));
                        result.Add((layer + 1, new Point(4, 1)));
                        result.Add((layer + 1, new Point(4, 2)));
                        result.Add((layer + 1, new Point(4, 3)));
                        result.Add((layer + 1, new Point(4, 4)));
                    }

                    if (bug.Y == 3)
                    {
                        result.Add((layer + 1, new Point(0, 4)));
                        result.Add((layer + 1, new Point(1, 4)));
                        result.Add((layer + 1, new Point(2, 4)));
                        result.Add((layer + 1, new Point(3, 4)));
                        result.Add((layer + 1, new Point(4, 4)));
                    }
                }

                if (n.Y < 0)
                {
                    result.Add((layer - 1, new Point(2, 1)));
                }

                if (n.Y > 4)
                {
                    result.Add((layer - 1, new Point(2, 3)));
                }

                if (n.X > 4)
                {
                    result.Add((layer - 1, new Point(3, 2)));
                }

                if (n.X < 0)
                {
                    result.Add((layer - 1, new Point(1, 2)));
                }

                if (n.X >= 0 && n.X <= 4 && n.Y >= 0 && n.Y <= 4 && !(n.X == 2 && n.Y == 2))
                {
                    result.Add((layer, n));
                }
            }

            return result;
        }
    }
}
