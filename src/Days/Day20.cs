using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 20)]
    public class Day20 : BaseDay
    {
        public override string PartOne(string input)
        {
            var map = input.CreateCharGrid();

            var portalPoints = GetPortalPoints(map).ToList();
            var paths = GetPaths(map, portalPoints);

            var startPos = portalPoints.Single(p => p.name == "AA").pos;
            var endPos = portalPoints.Single(p => p.name == "ZZ").pos;

            var result = FindBestPath(paths, startPos, endPos);

            return result.ToString();
        }

        public override string PartTwo(string input)
        {
            var map = input.CreateCharGrid();

            var portalPoints = GetPortalPoints(map).ToList();
            var paths = GetPaths(map, portalPoints);

            var startPos = portalPoints.Single(p => p.name == "AA").pos;
            var endPos = portalPoints.Single(p => p.name == "ZZ").pos;

            var result = FindBestRecursivePath(paths, startPos, endPos);

            return result.ToString();
        }

        private int FindBestPath(Dictionary<Point, Dictionary<Point, (int steps, int layer)>> paths, Point start, Point end)
        {
            var q = new SimplePriorityQueue<(Point pos, int steps), int>();

            q.Enqueue((start, 0), 0);

            var seen = new HashSet<Point>();

            while (q.Any())
            {
                var (pos, steps) = q.Dequeue();

                if (pos == end)
                {
                    return steps;
                }

                if (!seen.Contains(pos))
                {
                    seen.Add(pos);

                    foreach (var path in paths[pos])
                    {
                        q.Enqueue((path.Key, steps + path.Value.steps), steps + path.Value.steps);
                    }
                }
            }

            throw new Exception("Path not found");
        }

        private int FindBestRecursivePath(Dictionary<Point, Dictionary<Point, (int steps, int layer)>> paths, Point start, Point end)
        {
            var q = new SimplePriorityQueue<(Point pos, int steps, int layer), int>();

            q.Enqueue((start, 0, 0), 0);

            var seen = new HashSet<(Point pos, int layer)>();

            while (q.Any())
            {
                var (pos, steps, layer) = q.Dequeue();

                if (pos == end && layer == 0)
                {
                    return steps;
                }

                if (!seen.Contains((pos, layer)))
                {
                    seen.Add((pos, layer));

                    foreach (var path in paths[pos])
                    {
                        if (layer > 0 || (layer == 0 && path.Value.layer >= 0))
                        {
                            q.Enqueue((path.Key, steps + path.Value.steps, layer + path.Value.layer), steps + path.Value.steps);
                        }
                    }
                }
            }

            throw new Exception("Path not found");
        }

        private Dictionary<Point, Dictionary<Point, (int steps, int layer)>> GetPaths(char[,] map, List<(string name, Point pos, int layer)> portalPoints)
        {
            var points = new HashSet<Point>(portalPoints.Select(p => p.pos));
            var result = new Dictionary<Point, Dictionary<Point, (int steps, int layer)>>();
            
            foreach (var p in points)
            {
                var paths = map.FindShortestPaths(c => c == '.', p)
                               .Where(x => points.Contains(x.Key) && x.Key != p)
                               .ToList();

                var destinations = new Dictionary<Point, (int steps, int layer)>();
                paths.ForEach(x => destinations.Add(x.Key, (x.Value, 0)));
                
                var (name, pos, layer) = portalPoints.Single(x => x.pos == p);
                var otherPortal = portalPoints.FirstOrDefault(x => x.name == name && x.pos != p);

                if (otherPortal != default)
                {
                    destinations.Add(otherPortal.pos, (1, layer));
                }

                result.Add(p, destinations);
            }

            return result;
        }

        private IEnumerable<(string name, Point pos, int layer)> GetPortalPoints(char[,] map)
        {
            for (var x = 2; x <= map.GetUpperBound(0) - 2; x++)
            {
                if (map[x, 2] == '.')
                {
                    yield return ($"{map[x, 0]}{map[x, 1]}", new Point(x, 2), -1);
                }

                if (map[x, map.GetUpperBound(1) - 2] == '.')
                {
                    yield return ($"{map[x, map.GetUpperBound(1) - 1]}{map[x, map.GetUpperBound(1)]}", new Point(x, map.GetUpperBound(1) - 2), -1);
                }
            }

            for (var y = 2; y <= map.GetUpperBound(1) - 2; y++)
            {
                if (map[2, y] == '.')
                {
                    yield return ($"{map[0, y]}{map[1, y]}", new Point(2, y), -1);
                }

                if (map[map.GetUpperBound(0) - 2, y] == '.')
                {
                    yield return ($"{map[map.GetUpperBound(0) - 1, y]}{map[map.GetUpperBound(0), y]}", new Point(map.GetUpperBound(0) - 2, y), -1);
                }
            }

            // inner corners
            // 36, 36
            // 96, 36
            // 36, 90
            // 96, 90

            for (var x = 36; x <= 96; x++)
            {
                if (map[x, 36] == '.')
                {
                    yield return ($"{map[x, 37]}{map[x, 38]}", new Point(x, 36), 1);
                }

                if (map[x, 90] == '.')
                {
                    yield return ($"{map[x, 88]}{map[x, 89]}", new Point(x, 90), 1);
                }
            }

            for (var y = 36; y <= 90; y++)
            {
                if (map[36, y] == '.')
                {
                    yield return ($"{map[37, y]}{map[38, y]}", new Point(36, y), 1);
                }

                if (map[96, y] == '.')
                {
                    yield return ($"{map[94, y]}{map[95, y]}", new Point(96, y), 1);
                }
            }
        }
    }
}
