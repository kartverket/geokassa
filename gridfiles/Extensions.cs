using System;
using System.Collections.Generic;
using System.Text;

namespace gridfiles
{
    public static class DoubleEx
    {
        public static double GetPrecision(this double value, int fractionRound)
        {
            double factor = Math.Pow(10, fractionRound);
            return Math.Truncate(value * factor) / factor;
        }

        public static double ToArcSec(this double value)
        {
            return value * 648000d / Math.PI;
        }

        public static double ToPpm(this double value)
        {
            return 1000000d * (value - 1d);
        }
    }
}
