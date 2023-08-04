using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class CptFile
    {
        private List<CommonPoint> _commonPointList = new List<CommonPoint>();
        private const double Ro = Math.PI / 180;

        public CptFile()
        {            
        }

        public List<CommonPoint> CommonPointList
        {
            get => _commonPointList;
            set => _commonPointList = value;
        }         

        public bool ReadCptFile(string inputFileName)
        {
            if (!File.Exists(inputFileName))
                return false;

            _commonPointList.Clear();

            using (FileStream fs = new FileStream(inputFileName, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    if (br.BaseStream.Length < 404)
                    {
                        br.Close();
                        fs.Close();
                        return false;
                    }

                    var chars = br.ReadChars(128);
                    string filename = new string(chars).TrimEnd();

                    chars = br.ReadChars(128);
                    string copyright = new string(chars).TrimEnd();

                    chars = br.ReadChars(128);
                    string licence = new string(chars).TrimEnd();

                    var version = br.ReadDouble();
                    var epsgFromPoints = br.ReadInt32();
                    var epsgToPoints = br.ReadInt32();
                    var noOfPoints = br.ReadInt32();

                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        if ((br.BaseStream.Length - br.BaseStream.Position) % 68 != 0)
                        {
                            br.Close();
                            fs.Close();
                            return false;
                        }

                        var nameChars = br.ReadChars(8);
                        var name = new string(nameChars).TrimEnd('\0');

                        var cp = new CommonPoint()
                        {
                            Name = name,
                            Lon1 = br.ReadDouble() / Ro,
                            Lat1 = br.ReadDouble() / Ro,
                            H1 = br.ReadDouble(),
                            Lon2 = br.ReadDouble() / Ro,
                            Lat2 = br.ReadDouble() / Ro,
                            H2 = br.ReadDouble(),
                            Area = br.ReadInt32(),
                            Dist = br.ReadDouble()
                        };
                        CommonPointList.Add(cp);
                    }
                    br.Close();
                }
                fs.Close();
            }
            return true;
        }

        internal bool ReadCsvFile(string inputFileName)
        {
            try
            {
                var reader = new StreamReader(File.OpenRead(inputFileName));

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (values.Count() < 8)
                    {
                        reader.Close();
                        return false;
                    }
                    var name = values[0];
                    if (!double.TryParse(values[1], out double lon1 ) ||
                        !double.TryParse(values[2], out double lat1) ||
                        !double.TryParse(values[3], out double h1) ||
                        !double.TryParse(values[4], out double lon2) ||
                        !double.TryParse(values[5], out double lat2) ||
                        !double.TryParse(values[6], out double h2) || 
                        !int.TryParse(values[7], out int area))
                        continue;
                    else
                    {
                        var cp = new CommonPoint()
                        {
                            Name = name,
                            Lat1 = lat1,
                            Lon1 = lon1,
                            H1 = h1,
                            Lat2 = lat2,
                            Lon2 = lon2,
                            H2 = h2,
                            Area = area
                        };
                        _commonPointList.Add(cp);
                    }
                }
                reader.Close();

                if (_commonPointList.Count() == 0)
                    return false;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
            return true;
        }

        public bool ReadInputFile(string inputFileName)
        {
            try
            {
                if (!File.Exists(inputFileName))
                    return false;                 

                if (Path.GetExtension(inputFileName).ToLowerInvariant() == ".cpt")                
                    return ReadCptFile(inputFileName);                 
                else if (Path.GetExtension(inputFileName).ToLowerInvariant() == ".csv")               
                    return ReadCsvFile(inputFileName);
                
                return true;
            }
            catch 
            {               
                return false;
            }
        }
    }

    public class CommonPoint
    {
        private const double Ro = Math.PI / 180;

        internal CommonPoint()
        {
        }

        internal string Name { get; set; }
        internal double Lon1 { get; set; }
        internal double Lat1 { get; set; }
        internal double H1 { get; set; }
        internal double Lon2 { get; set; }
        internal double Lat2 { get; set; }
        internal double H2 { get; set; }
        internal Int32 Area { get; set; }
        internal double Dist { get; set; }

        internal double CosLat(double lat) => Math.Cos(lat * Ro);

        internal double GetDistance(double lat, double lon) => Math.Sqrt(Math.Pow((Lon1 - lon) * CosLat(lat), 2) + Math.Pow(Lat1 - lat, 2));
 
        internal double GetDistance(CommonPoint cp) => Math.Sqrt(Math.Pow((Lon1 - cp.Lon1) * CosLat(cp.Lat1), 2) + Math.Pow(Lat1 - cp.Lat1, 2));
    }
}
