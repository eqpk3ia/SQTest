using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 10)]
    public class Day10 : BaseDay
    {
        private char[,] _grid;

        public override string PartOne(string input)
        {
            _grid = input.CreateCharGrid();

            var station = _grid.GetPoints(x => _grid[x.X, x.Y] == '#').WithMax(x => CountVisibleAsteroids(x));

            return CountVisibleAsteroids(station).ToString();
        }

        public override string PartTwo(string input)
        {
            _grid = input.CreateCharGrid();

            var station = _grid.GetPoints(x => _grid[x.X, x.Y] == '#').WithMax(x => CountVisibleAsteroids(x));

            var vaporized = 0;

            while (true)
            {
                var visible = _grid.GetPoints().Where(p => _grid[p.X, p.Y] == '#' && IsVisible(p, station)).ToList();
                var visibleE = visible.Where(v => v.X >= station.X).OrderBy(v => v.CalcSlope(station));
                var visibleW = visible.Where(v => v.X < station.X).OrderBy(v => v.CalcSlope(station));

                var toVapor = visibleE.Concat(visibleW);

                foreach (var v in toVapor)
                {
                    _grid[v.X, v.Y] = '*';
                    vaporized++;

                    if (vaporized == 200)
                    {
                        return (v.X * 100 + v.Y).ToString();
                    }
                }
            }
        }

        private int CountVisibleAsteroids(Point from) => _grid.GetPoints().Count(p => _grid[p.X, p.Y] == '#' && IsVisible(p, from));

        private bool IsVisible(Point p, Point from)
        {
            if (p == from)
            {
                return false;
            }

            var slope = p.CalcSlope(from);
            var distance = p.CalcDistance(from);

            return !_grid.GetPoints().Any(x => _grid[x.X, x.Y] == '#' &&
                                        x.CalcSlope(from) == slope &&
                                        x.CalcDistance(from) < distance &&
                                        !IsBetween(from, x, p));
        }

        private bool IsBetween(Point station, Point a, Point b)
        {
            if (b.X > station.X && a.X > station.X)
            {
                return false;
            }

            if (b.X < station.X && a.X < station.X)
            {
                return false;
            }

            if (b.X == station.X)
            {
                if (b.Y > station.Y && a.Y > station.Y)
                {
                    return false;
                }

                if (b.Y < station.Y && a.Y < station.Y)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
