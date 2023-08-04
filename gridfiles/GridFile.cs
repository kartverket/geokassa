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
            bin = 0,
            ct2 = 1,
            gtx = 2,
            tiff = 3
        }

        public enum Direction
        {
            fwd = 0,
            inv = 1
        }
   
        private CommonPointList _list = null;

        public string OutputFileName { get; set; } = "";

        public CommonPointList CommonPointList
        {
            get => _list = _list ?? new CommonPointList();
            set => _list = value;
        }

        public List<CommonPointXYZ> PointList
        {
            get
            {
                if (_list == null)
                    _list = new CommonPointList();

                return _list.PointList;
            }                
            set => _list.PointList = value;
        }

        public List<CommonPointXYZ> ValidPointList => PointList.Where(o => !o.HasNullValues).ToList();

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

        public virtual bool ClipGrid(double west_long, double south_lat, double east_long, double north_lat)
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

                p.X_Source = 3172870.7154;
                p.Y_Source = 604208.28140;
                p.Z_Source = 5481574.23150;
                Console.WriteLine($"X: {p.X_Source} Y: {p.Y_Source} Z: {p.Z_Source} Lat: {p.Phi_SourceDeg} Lon: {p.Lambda_SourceDeg} H: {p.H_Source }");

                p.Phi_SourceDeg = 59.6603328269847;
                p.Lambda_SourceDeg = 10.7817180373767;
                p.H_Source = 133.463056743145;
                Console.WriteLine($"X: {p.X_Source} Y: {p.Y_Source} Z: {p.Z_Source} Lat: {p.Phi_SourceDeg} Lon: {p.Lambda_SourceDeg} H: {p.H_Source }");

                var lat1 = p.Phi_SourceDeg;
                var lon1 = p.Lambda_SourceDeg;
                var height1 = p.H_Source;
                Console.WriteLine($"X: {p.X_Source} Y: {p.Y_Source} Z: {p.Z_Source} Lat: {lat1} Lon: {lon1} H: {height1}");

                var x = p.X_Source;
                var y = p.Y_Source;
                var z = p.Z_Source;
                Console.WriteLine($"X: {x} Y: {y} Z: {z} Lat: {lat1} Lon: {lon1} H: {height1}");

                p.H_Source -= 100d;
                Console.WriteLine($"X: {p.X_Source} Y: {p.Y_Source} Z: {p.Z_Source} Lat: {p.Phi_SourceDeg} Lon: {p.Lambda_SourceDeg} H: {p.H_Source }");

                p.Phi_SourceDeg -= 1d;
                Console.WriteLine($"X: {p.X_Source} Y: {p.Y_Source} Z: {p.Z_Source} Lat: {p.Phi_SourceDeg} Lon: {p.Lambda_SourceDeg} H: {p.H_Source }");

                p.Lambda_SourceDeg -= 1d;
                Console.WriteLine($"X: {p.X_Source} Y: {p.Y_Source} Z: {p.Z_Source} Lat: {p.Phi_SourceDeg} Lon: {p.Lambda_SourceDeg} H: {p.H_Source }");
            }
            return trueOrFalse;
        }
    }
}
