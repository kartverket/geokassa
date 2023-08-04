using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class VelocityFile : GridParam
    {   
        private List<VelocityPoint> _list = null;

        public VelocityFile()
        {
        }
         
        internal string OutputFileName { get; set; } = "";

        public override double LowerLeftLatitude
        {
            get
            {
                if (base.LowerLeftLatitude == 0d)
                    LowerLeftLatitude = GridData.Min(x => x.Lat);

                return base.LowerLeftLatitude;
            }   
            set => base.LowerLeftLatitude = value;
        }

        public override double LowerLeftLongitude
        {
            get
            {
                if (base.LowerLeftLongitude == 0d)
                    LowerLeftLongitude = GridData.Min(x => x.Lon);

                return base.LowerLeftLongitude;
            }
            set => base.LowerLeftLongitude = value;
        }

        /*
        public override double UpperLeftLatitude
        { 
            get => 0d;
           // set;
        }

        public override double UpperLeftLongitude
        {
            get => 0d;
           // set;
        }

        public override double UpperRightLatitude
        {
            get => 0d;
           // set;
        }

        public override double UpperRightLongitude
        {
            get => 0d;
           // set;
        }*/

        public override double DeltaLatitude
        {
            get
            {
                if (base.DeltaLatitude == 0)                
                    base.DeltaLatitude = GridData.GroupBy(p => new { p.Lat }).Max(x => x.Key.Lat) - GridData.GroupBy(p => new { p.Lat }).SkipLast(1).Max(x => x.Key.Lat);
                
                return base.DeltaLatitude;
            }
            set => base.DeltaLatitude = value;
        }

        public override double DeltaLongitude
        {
            get
            {
                if (base.DeltaLongitude == 0)                
                    base.DeltaLongitude = GridData.GroupBy(p => new { p.Lon }).Max(x => x.Key.Lon) - GridData.GroupBy(p => new { p.Lon }).SkipLast(1).Max(x => x.Key.Lon);
                
                return base.DeltaLongitude;
            }
            set => base.DeltaLongitude = value;
        }
        

        public override Int32 NRows
        { 
            get
            {
                if (base.NRows == 0)                
                    base.NRows = (Int32)GridData.GroupBy(p => new { p.Lat }).Count();
                
                return base.NRows;
            } 
            set => base.NRows = value;
        }

        public override Int32 NColumns
        {
            get
            {
                if (base.NColumns == 0)
                    base.NColumns = (Int32)GridData.GroupBy(p => new { p.Lon }).Count();
               
                return base.NColumns;
            }
            set => base.NColumns = value;
        }

        public List<VelocityPoint> GridData
        {
            get => _list = _list ?? new List<VelocityPoint>();
            set => _list = value;
        }

        public bool ReadVelocityFile(string inputFileName)
        {
            if (!File.Exists(inputFileName))
                return false;

            var reader = new StreamReader(File.OpenRead(inputFileName));
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                var values = line.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Count() != 5)
                {
                    continue;
                    reader.Close();
                    return false;
                }

                if (!double.TryParse(values[0], out double lat))
                    return false;

                if (!double.TryParse(values[1], out double lon))
                    return false;

                if (!double.TryParse(values[2], out double vE))
                    return false;

                if (!double.TryParse(values[3], out double vN))
                    return false;

                if (!double.TryParse(values[4], out double vUp))
                    return false;

                var vPoint = new VelocityPoint()
                {
                    Lat = lat,
                    Lon = lon,
                    EastVelocity = vE,
                    NorthVelocity = vN,
                    UpVelocity = vUp
                };
                GridData.Add(vPoint);
            }
            return true;
        }
    }
}
