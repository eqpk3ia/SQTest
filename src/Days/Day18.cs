using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 18)]
    public class Day18 : BaseDay
    {
        private Dictionary<Point, Dictionary<Point, (int distance, HashSet<Point> doors, HashSet<Point> keys)>> _pathsByStart;
        private readonly Dictionary<Point, char> _keys = new Dictionary<Point, char>();
        private readonly Dictionary<Point, char> _doors = new Dictionary<Point, char>();
        private char[,] _map;

        public override string PartOne(string input)
        {
            _map = input.CreateCharGrid();

            var startPos = GetStartPos(_map)[0];
            PreProcessMap(_map);

            var result = FindPath(startPos, "abcdefghijklmnopqrstuvwxyz");

            return result.ToString();
        }

        public override string PartTwo(string input)
        {
            _map = input.CreateCharGrid();

            _map[39, 39] = '@';
            _map[39, 40] = '#';
            _map[39, 41] = '@';
            _map[40, 39] = '#';
            _map[40, 40] = '#';
            _map[40, 41] = '#';
            _map[41, 39] = '@';
            _map[41, 40] = '#';
            _map[41, 41] = '@';

            var startPos = GetStartPos(_map);
            var keyMap = PreProcessMap(_map);
            _pathsByStart = GetPaths(_map, keyMap, startPos);

            startPos.ForEach(p => _keys.Add(p, '@'));

            var result = FindPath(startPos, "abcdefghijklmnopqrstuvwxyz");

            return result.ToString();
        }

        private Dictionary<Point, Dictionary<Point, (int distance, HashSet<Point> doors, HashSet<Point> keys)>> GetPaths(char[,] map, Dictionary<Point, Point> keyMap, Point[] startPos)
        {
            var keyPoints = keyMap.Select(k => k.Key).Concat(startPos).ToList();

            return GetPaths(map, keyMap, keyPoints);
        }

        private Dictionary<Point, Dictionary<Point, (int distance, HashSet<Point> doors, HashSet<Point> keys)>> GetPaths(char[,] map, Dictionary<Point, Point> keyMap, List<Point> keyPoints)
        {
            var result = new Dictionary<Point, Dictionary<Point, (int distance, HashSet<Point> doors, HashSet<Point> keys)>>();
            Log(map.GetString());

            foreach (var combo in keyPoints.GetCombinations(2))
            {
                var path = GetShortestPath(map, combo.First(), combo.Last(), new HashSet<Point>());

                if (path != null)
                {
                    var doors = path.Where(p => keyMap.Any(k => k.Value == p)).Select(p => keyMap.Single(k => k.Value == p).Key).ToList();
                    var keys = path.Where(p => keyMap.Any(k => k.Key == p)).Where(p => p != combo.First() && p != combo.Last()).ToList();
                    var dict = new Dictionary<Point, (int distance, HashSet<Point> doors, HashSet<Point> keys)>();

                    if (!result.ContainsKey(combo.First()))
                    {
                        result.Add(combo.First(), new Dictionary<Point, (int distance, HashSet<Point> doors, HashSet<Point> keys)>());
                    }

                    result[combo.First()].Add(combo.Last(), (path.Count - 1, new HashSet<Point>(doors), new HashSet<Point>(keys)));
                }
            }

            return result;
        }

        private List<Point> GetShortestPath(char[,] map, Point a, Point b, HashSet<Point> visited)
        {
            var result = new List<Point>() { a };

            if (a == b)
            {
                return result;
            }

            var neighbors = map.GetNeighborPoints(a.X, a.Y).Where(c => c.c == '.' && !visited.Contains(c.point)).ToList();

            visited.Add(a);
            var paths = neighbors.Select(n => GetShortestPath(map, n.point, b, visited)).Where(p => p != null).ToList();
            visited.Remove(a);

            if (paths.Any())
            {
                result.AddRange(paths.WithMin(p => p.Count));
                return result;
            }

            return null;
        }

        private Dictionary<Point, Point> PreProcessMap(char[,] map)
        {
            var result = new Dictionary<Point, Point>();

            var keys = map.GetPoints().Where(p => map[p.X, p.Y] >= 'a' && map[p.X, p.Y] <= 'z').ToList();

            foreach (var k in keys)
            {
                var door = map.GetPoints().Single(p => map[p.X, p.Y] == (char)(map[k.X, k.Y] - 32));
                result.Add(k, door);

                _keys.Add(k, map[k.X, k.Y]);
                _doors.Add(door, map[k.X, k.Y]);

                map[k.X, k.Y] = '.';
                map[door.X, door.Y] = '.';
            }

            return result;
        }

        public int FindPath(Point startPos, string keys)
        {
            var q = new Queue<(int steps, Point pos, string keysLeft)>();
            var seen = new HashSet<(Point pos, string keysLeft)>();

            q.Enqueue((0, startPos, keys));

            while (q.Any())
            {
                var state = q.Dequeue();

                var seenKey = (state.pos, state.keysLeft);

                if (!seen.Contains(seenKey))
                {
                    if (!state.keysLeft.Any())
                    {
                        return state.steps;
                    }

                    seen.Add(seenKey);

                    q.Enqueue(ProcessState(state));
                }
            }

            throw new Exception("No path found");
        }

        public int FindPath(Point[] startPos, string keys)
        {
            var q = new SimplePriorityQueue<(int steps, Point[] pos, string keysLeft), int>();
            var seen = new HashSet<(Point a, Point b, Point c, Point d, string keysLeft)>();

            q.Enqueue((0, startPos, keys), 0);

            while (q.Any())
            {
                var state = q.Dequeue();

                var seenKey = (state.pos[0], state.pos[1], state.pos[2], state.pos[3], state.keysLeft);

                if (!seen.Contains(seenKey))
                {
                    if (!state.keysLeft.Any())
                    {
                        return state.steps;
                    }

                    seen.Add(seenKey);

                    var newStates = ProcessState(state);
                    newStates.ForEach(s => q.Enqueue(s, s.steps));
                }
            }

            throw new Exception("No path found");
        }

        private IEnumerable<(int steps, Point pos, string keysLeft)> ProcessState((int steps, Point pos, string keysLeft) state)
        {
            var (steps, pos, keysLeft) = state;

            var paths = pos.GetNeighbors(false).Where(p => _map[p.X, p.Y] == '.').ToList();

            foreach (var path in paths)
            {
                if (_doors.ContainsKey(path) && keysLeft.Contains(_doors[path]))
                {
                    continue;
                }

                var newKeysLeft = keysLeft;

                if (_keys.ContainsKey(path) && keysLeft.Contains(_keys[path]))
                {
                    newKeysLeft = keysLeft.Remove(keysLeft.IndexOf(_keys[path]), 1);
                }

                yield return (steps + 1, path, newKeysLeft);
            }
        }

        private IEnumerable<(int steps, Point[] pos, string keysLeft)> ProcessState((int steps, Point[] pos, string keysLeft) state)
        {
            var (steps, pos, keysLeft) = state;

            for (var i = 0; i < 4; i++)
            {
                var paths = _pathsByStart[pos[i]].Where(p => keysLeft.Contains(_keys[p.Key]))
                                    .Where(p => !p.Value.doors.Any(d => keysLeft.Contains(_keys[d])))
                                    .ToList();

                foreach (var path in paths)
                {
                    var newKeysLeft = keysLeft.Remove(keysLeft.IndexOf(_keys[path.Key]), 1);
                    path.Value.keys.ForEach(k => 
                    { 
                        if (newKeysLeft.Contains(_keys[k])) newKeysLeft = newKeysLeft.Remove(newKeysLeft.IndexOf(_keys[k]), 1); 
                    });

                    var newPos = new Point[4] { pos[0], pos[1], pos[2], pos[3] };
                    newPos[i] = path.Key;
                    yield return (steps + path.Value.distance, newPos, newKeysLeft);
                }
            }
        }

        private Point[] GetStartPos(char[,] map)
        {
            var result = map.GetPoints().Where(p => map[p.X, p.Y] == '@').ToList();

            result.ForEach(r => map[r.X, r.Y] = '.');

            return result.ToArray();
        }
    }
}
