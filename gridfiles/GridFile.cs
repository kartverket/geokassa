using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class GridFile
    {
        public string OutputFileName { get; set; } = "";

        public virtual bool GenerateGridFile(string outputFileName, bool isRandom = false)
        {
            return true;
        }

        public virtual bool PopulatedGrid(double k, double c, double sn)
        {
            return true;
        }

        public virtual bool ReadSourceFromFile(string inputFileName)
        {
            return true;
        }

        public virtual bool ReadTargetFromFile(string inputFileName)
        {
            return true;
        }
    }

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
