using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class GriFile : GridFile
    {
        private GridParam _gridParam;
  
        public GriFile()
        {
        }

        public GriFile(GridParam gridParam)
        {
            _gridParam = gridParam;
        }

        public GriFile(string inputFilename, GridParam gridParam)
        {
            InputFileName = inputFilename;
            _gridParam = gridParam;
        }

        internal string InputFileName { get; set; } = "";       

        public virtual double LowerLeftLatitude
        {
            get => _gridParam.LowerLeftLatitude;
            set => _gridParam.LowerLeftLatitude = value;
        }

        public virtual double LowerLeftLongitude
        {
            get => _gridParam.LowerLeftLongitude;
            set => _gridParam.LowerLeftLongitude = value;
        }

        public double UpperLeftLatitude => LowerLeftLatitude + (NRows - 1) * DeltaLatitude;

        public double UpperLeftLongitude => LowerLeftLongitude;

        public virtual double DeltaLatitude
        {
            get => _gridParam.DeltaLatitude;
            set => _gridParam.DeltaLatitude = value;
        }

        public virtual double DeltaLongitude
        {
            get => _gridParam.DeltaLongitude;
            set => _gridParam.DeltaLongitude = value;
        }

        public virtual Int32 NRows
        {
            get => _gridParam.NRows;
            set => _gridParam.NRows = value;
        }  

        public virtual Int32 NColumns
        {
            get => _gridParam.NColumns;
            set => _gridParam.NColumns = value;
        }

        public bool IsEmpty => !Data.Any();

        internal List<float> Data { get; set; } = new List<float>();
        
        public bool ReadGridFile()
        {
            try
            {
                if (!File.Exists(InputFileName))
                    return false;

                Data.Clear();

                using (var fileStream = new FileStream(InputFileName, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        string line;
                        bool headerIsFound = true;

                        while ((line = streamReader.ReadLine()) != null)
                        {
                            string[] linearray = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (linearray.Count() == 6 && headerIsFound)
                            {
                                var upperRightLatitude = 0d;
                                var upperRightLongitude = 0d;
                                var lowerLeftLatitude = 0d;
                                var lowerLeftLongitude = 0d;
                                var deltaLatitude = 0d;
                                var deltaLongitude = 0d;
                                headerIsFound = false;
                              
                                if (!double.TryParse(linearray[0], out lowerLeftLatitude))
                                    return false;                               

                                if (!double.TryParse(linearray[1], out upperRightLatitude))
                                    return false;
                               
                                if (!double.TryParse(linearray[2], out lowerLeftLongitude))
                                    return false;                               

                                if (!double.TryParse(linearray[3], out upperRightLongitude))
                                    return false;

                                if (!double.TryParse(linearray[4], out deltaLatitude))
                                    return false;

                                if (!double.TryParse(linearray[5], out deltaLongitude))
                                    return false;

                                LowerLeftLatitude = lowerLeftLatitude;
                                LowerLeftLongitude = lowerLeftLongitude;
                                DeltaLatitude = deltaLatitude;
                                DeltaLongitude = deltaLongitude;

                                NRows = (int)(Math.Round((upperRightLatitude - LowerLeftLatitude) / DeltaLatitude) + 1);
                                NColumns = (int)(Math.Round((upperRightLongitude - LowerLeftLongitude) / DeltaLongitude) + 1);

                                DeltaLatitude = (upperRightLatitude - LowerLeftLatitude) / (NRows - 1);
                                DeltaLongitude = (upperRightLongitude - LowerLeftLongitude) / (NColumns - 1);                                
 
                                continue;
                            }
                            foreach (var value in linearray)
                            {                                
                                var v = 0f;
                                
                                if (!float.TryParse(value, out v))
                                    return false;

                                Data.Add(v);
                            }
                        }
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
                return true;
            }
            catch
            {
                Data.Clear();
                return false;
            }
        }

        public override bool ClipGrid(double west_long, double south_lat, double east_long, double north_lat)
        {
            if (IsEmpty)
                return false;
            
            var newData  = new List<float>();
            var newLowerLeftLatitude = Double.MaxValue;
            var newLowerLeftLongitude = Double.MaxValue;

            for (Int32 i = 0; i < NRows; i++)
            {
                var lat = UpperLeftLatitude - i * DeltaLatitude;

                if (lat < south_lat || lat > north_lat)
                    continue;

                newLowerLeftLatitude = lat < newLowerLeftLatitude ? lat : newLowerLeftLatitude;

                for (Int32 j = 0; j < NColumns; j++)
                {
                    var lon = UpperLeftLongitude + j * DeltaLongitude;                    
                    
                    if (lon < west_long || lon > east_long)
                        continue;

                    newLowerLeftLongitude = lon < newLowerLeftLongitude ? lon : newLowerLeftLongitude;

                    var index = i * NColumns + j;
                    var v = Data.ElementAt(index);
                    
                    newData.Add(v);

                    // Console.Write($"{index} {v:F2} ");
                }
                // Console.WriteLine();
            }
            if (newData.Count() > 0)
            {
                Data = newData;

                NColumns = (Int32)((east_long - west_long) / DeltaLongitude);
                NRows = (Int32)((north_lat - south_lat) / DeltaLatitude);

                LowerLeftLatitude = newLowerLeftLatitude;
                LowerLeftLongitude = newLowerLeftLongitude;
            }
            return true;
        }
    }
}
