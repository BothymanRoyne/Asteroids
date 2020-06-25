using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroids
{
    internal static class Ext
    {
        public static float NextDoubleRange(this System.Random random, float minNumber, float maxNumber) => (float)((random.NextDouble() * (maxNumber - minNumber)) + minNumber);
        public static int NextRange(this System.Random random, int minNumber, int maxNumber) => (random.Next() * (maxNumber - minNumber)) + minNumber;
        public static PointF RandomLocationInCanvas(this System.Windows.Forms.Form f) => new PointF(f.ClientSize.Width, f.ClientSize.Height);
        public static int Clamp(this int n, int min, int max) => (n >= min) ? (n <= max) ? n : max : min;
        //public static Color RandomColor(this Color c) => Color.FromArgb(Asteroid.RNG.Next(256), Asteroid.RNG.Next(256), Asteroid.RNG.Next(256));

        public static float LimitToRange(this float value, float inclusiveMinimum, float inclusiveMaximum)
        {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        }
    }
}
