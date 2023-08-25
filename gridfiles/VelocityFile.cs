using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class VelocityFile
    {
        private VelocityGrid _velocityGrid = null;
        private GridParam _gridParam;       

        public VelocityFile(GridParam gridParam)
        {
            _gridParam = gridParam;
        }

        internal string OutputFileName { get; set; } = "";

        public double LowerLeftLatitude
        {
            get
            {
                if (_gridParam.LowerLeftLatitude == 0d)
                    LowerLeftLatitude = GridData.VelocityGridData.Min(x => x.Lat);

                return _gridParam.LowerLeftLatitude;
            }   
            set => _gridParam.LowerLeftLatitude = value;
        }

        public double LowerLeftLongitude
        {
            get
            {
                if (_gridParam.LowerLeftLongitude == 0d)
                    LowerLeftLongitude = GridData.VelocityGridData.Min(x => x.Lon);

                return _gridParam.LowerLeftLongitude;
            }
            set => _gridParam.LowerLeftLongitude = value;
        }

        public double LowerRightLatitude
        {
            get => _gridParam.LowerRightLatitude;
        }

        public double LowerRightLongitude
        {
            get => _gridParam.LowerRightLongitude;
        }

        public double UpperLeftLatitude
        { 
            get => _gridParam.UpperLeftLatitude;                         
        }

        public double UpperLeftLongitude
        {
            get => _gridParam.UpperLeftLongitude;
        }

        public double UpperRightLatitude
        {
            get => _gridParam.UpperRightLatitude;
        }
 
        public double UpperRightLongitude
        {
            get => _gridParam.UpperRightLongitude;           
        }

        public double DeltaLatitude
        {
            get
            {
                if (_gridParam.DeltaLatitude == 0d)
                    _gridParam.DeltaLatitude = GridData.VelocityGridData.GroupBy(p => new { p.Lat }).Max(x => x.Key.Lat) - GridData.VelocityGridData.GroupBy(p => new { p.Lat }).SkipLast(1).Max(x => x.Key.Lat);
                
                return _gridParam.DeltaLatitude;
            }
            set => _gridParam.DeltaLatitude = value;
        }

        public double DeltaLongitude
        {
            get
            {
                if (_gridParam.DeltaLongitude == 0d)
                    _gridParam.DeltaLongitude = GridData.VelocityGridData.GroupBy(p => new { p.Lon }).Max(x => x.Key.Lon) - GridData.VelocityGridData.GroupBy(p => new { p.Lon }).SkipLast(1).Max(x => x.Key.Lon);
                
                return _gridParam.DeltaLongitude;
            }
            set => _gridParam.DeltaLongitude = value;
        }
        
        public Int32 NRows
        { 
            get
            {
                if (_gridParam.NRows == 0)
                    _gridParam.NRows = (Int32)GridData.VelocityGridData.GroupBy(p => new { p.Lat }).Count();
                
                return _gridParam.NRows;
            } 
            set => _gridParam.NRows = value;
        }

        public Int32 NColumns
        {
            get
            {
                if (_gridParam.NColumns == 0)
                    _gridParam.NColumns = (Int32)GridData.VelocityGridData.GroupBy(p => new { p.Lon }).Count();
               
                return _gridParam.NColumns;
            }
            set => _gridParam.NColumns = value;
        }

        public VelocityGrid GridData
        {
            get => _velocityGrid = _velocityGrid ?? new VelocityGrid(_gridParam);
            set => _velocityGrid = value;
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
                    continue;

                if (!double.TryParse(values[0], out double lat))
                    return false;

                if (!double.TryParse(values[1], out double lon))
                    return false;

                if (!float.TryParse(values[2], out float vE))
                    return false;

                if (!float.TryParse(values[3], out float vN))
                    return false;

                if (!float.TryParse(values[4], out float vUp))
                    return false;

                var vPoint = new VelocityPoint()
                {
                    Lat = lat,
                    Lon = lon,
                    EastVelocity = vE,
                    NorthVelocity = vN,
                    UpVelocity = vUp
                };
                GridData.VelocityGridData.Add(vPoint);
            }
            return true;
        }
    }
}
