using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AdventOfCode.Days
{
    [Day(2019, 8)]
    public class Day08 : BaseDay
    {
        private int _width = 25;
        private int _height = 6;

        public override string PartOne(string input)
        {
            var layers = GetLayers(input).ToList();

            var target = layers.Select(l => l.ToList()).WithMin(x => x.Count(y => y == 0));

            return (target.Count(x => x == 1) * target.Count(x => x == 2)).ToString();
        }

        private IEnumerable<int[,]> GetLayers(string input)
        {
            var layerCount = input.Length / (_width * _height);
            var pos = 0;

            for (var l = 0; l < layerCount; l++)
            {
                var layer = new int[_width, _height];

                for (var y = 0; y < _height; y++)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        layer[x, y] = int.Parse(input[pos++].ToString());
                    }
                }

                yield return layer;
            }
        }

        public override string PartTwo(string input)
        {
            var layers = GetLayers(input).ToList();
            ImageHelper.CreateBitmap(_width, _height, @"C:\AdventOfCode\Day8.bmp", (x, y) => GetPixel(layers, x, y));

            return @"C:\AdventOfCode\Day8.bmp";
        }

        private Color GetPixel(IEnumerable<int[,]> layers, int x, int y)
        {
            foreach (var layer in layers)
            {
                if (layer[x, y] == 0)
                {
                    return Color.Black;
                }

                if (layer[x, y] == 1)
                {
                    return Color.White;
                }
            }

            throw new ArgumentException("All pixels were transparent");
        }
    }
}
