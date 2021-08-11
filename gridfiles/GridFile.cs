using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class GridFile
    {
        public enum GridType
        {
            ct2 = 0,
            gtx = 1,
            tiff = 2
        }
        /*
         * public enum TiffOutputTypeshort
        {
            hoffset = TiffOutputType.HORIZONTAL_OFFSET,
            geoid = TiffOutputType.VERTICAL_OFFSET_GEOGRAPHIC_TO_VERTICAL,
            vsep = TiffOutputType.VERTICAL_OFFSET_VERTICAL_TO_VERTICAL,
            goffset = TiffOutputType.GEOCENTRIC_TRANSLATION,
            vel = TiffOutputType.VELOCITY,
            deform = TiffOutputType.DEFORMATION_MODEL
        }

         */
        
        private List<CommonPointXYZ> _list;

        public string OutputFileName { get; set; } = "";

        public List<CommonPointXYZ> PointList
        {
            get => _list = _list ?? new List<CommonPointXYZ>();
            set => _list = value;
        }

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

        // NOTE: Tests geocentric<>geodetic transformation
        public bool TestTransformationsCommonPointXYZ()
        {
            bool trueOrFalse = false;
            if (trueOrFalse)
            {
                var p = new CommonPointXYZ();

                p.X1 = 3172870.7154;
                p.Y1 = 604208.28140;
                p.Z1 = 5481574.23150;
                Console.WriteLine($"X: {p.X1} Y: {p.Y1} Z: {p.Z1} Lat: {p.Phi1Deg} Lon: {p.Lambda1Deg} H: {p.H1 }");

                p.Phi1Deg = 59.6603328269847;
                p.Lambda1Deg = 10.7817180373767;
                p.H1 = 133.463056743145;
                Console.WriteLine($"X: {p.X1} Y: {p.Y1} Z: {p.Z1} Lat: {p.Phi1Deg} Lon: {p.Lambda1Deg} H: {p.H1 }");

                var lat1 = p.Phi1Deg;
                var lon1 = p.Lambda1Deg;
                var height1 = p.H1;
                Console.WriteLine($"X: {p.X1} Y: {p.Y1} Z: {p.Z1} Lat: {lat1} Lon: {lon1} H: {height1}");

                var x = p.X1;
                var y = p.Y1;
                var z = p.Z1;
                Console.WriteLine($"X: {x} Y: {y} Z: {z} Lat: {lat1} Lon: {lon1} H: {height1}");

                p.H1 -= 100d;
                Console.WriteLine($"X: {p.X1} Y: {p.Y1} Z: {p.Z1} Lat: {p.Phi1Deg} Lon: {p.Lambda1Deg} H: {p.H1 }");

                p.Phi1Deg -= 1d;
                Console.WriteLine($"X: {p.X1} Y: {p.Y1} Z: {p.Z1} Lat: {p.Phi1Deg} Lon: {p.Lambda1Deg} H: {p.H1 }");

                p.Lambda1Deg -= 1d;
                Console.WriteLine($"X: {p.X1} Y: {p.Y1} Z: {p.Z1} Lat: {p.Phi1Deg} Lon: {p.Lambda1Deg} H: {p.H1 }");
            }
            return trueOrFalse;
        }

        public void SpeedTestSimdjson()
        {
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
