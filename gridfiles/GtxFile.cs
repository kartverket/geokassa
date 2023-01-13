using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace gridfiles
{
    public class GtxFile : GridFile
    {
        private Matrix<double> _covNn = null;
        private Matrix<double> _covNnInv = null;
        private Matrix<double> _covNn_D_Inv = null;
        private Matrix<double> _covNn_D_InvSn = null;
        private Matrix<double> _a = null;
        private Matrix<double> _l = null;
        private Matrix<double> _signalNoise = null;
        private GridParam _gridParam;
        private GriFile _griFile = new GriFile();
        private CptFile _cptFile = new CptFile();

        public GtxFile()
        {
            _gridParam = new GridParam();
            _griFile = new GriFile(_gridParam);
        }

        public GtxFile(GridParam gridParam)
        {
            _gridParam = gridParam;
            _griFile = new GriFile(_gridParam);
        }

        public GtxFile(string inputFilename)
        {
            _gridParam = new GridParam();
            _griFile = new GriFile(inputFilename, _gridParam);
        }

        public GtxFile(string inputFilename, GridParam gridParam)
        {
            _gridParam = gridParam;
            _griFile = new GriFile(inputFilename, _gridParam);            
        }

        public CptFile CptFile
        {
            get => _cptFile;
            set => _cptFile = value;
        }
        
        public GriFile GriHeight
        {
            get => _griFile;
            set => _griFile = value;
        }     

        public List<CommonPoint> CommonPointList
        {
            get => _cptFile.CommonPointList;
            set => _cptFile.CommonPointList = value;
        }

        public List<float> Data
        {
            get => _griFile.Data;
            set => _griFile.Data = value;
        }
        
        public double DeltaLatitude
        {
            get => _gridParam.DeltaLatitude;
            set => _gridParam.DeltaLatitude = value;
        }

        public double DeltaLongitude
        {
            get => _gridParam.DeltaLongitude;
            set => _gridParam.DeltaLongitude = value;
        }

        public double LowerLeftLatitude
        {
            get => _gridParam.LowerLeftLatitude;
            set => _gridParam.LowerLeftLatitude = value;
        }

        public double LowerLeftLongitude
        {
            get => _gridParam.LowerLeftLongitude;
            set => _gridParam.LowerLeftLongitude = value;
        }

        public Int32 NRows
        {
            get => _gridParam.NRows;
            set => _gridParam.NRows = value;
        }
      
        public Int32 NColumns
        {
            get => _gridParam.NColumns;
            set => _gridParam.NColumns = value;
        } 

        public Matrix<double> A
        {
            get
            {
                if (_a == null)
                {
                    _a = Matrix<double>.Build.Dense(NumberOfPoints, 1);

                    for (var i = 0; i < NumberOfPoints; i++)
                        _a[i, 0] = 1d;
                }
                return _a;
            }
        }       

        public List<float> GridData
        {
            get
            {
                var data = new List<float>();
                var rand = new Random();

                for (var j = 0; j < NRows; j++)
                    for (var i = 0; i < NColumns; i++)
                        data.Add(100f * (float)rand.NextDouble());                              
                 
                return data;
            }
        }

        internal int NumberOfPoints => PointList.Count(x => !x.HasNullValues);
         
        public double PredictedHeight(double k, double c, double sn, double lat, double lon)
        {
            var covMN = CovMn(k, c, lat, lon);

            if (covMN.Enumerate().All(x => x == 0d))
                return -88.88880d;
           
            if (X == null)
                X = (A.Transpose() * CovNn_D_Inv(k, c, sn) * A).Inverse() * (A.Transpose() * CovNn_D_Inv(k, c, sn) * L);

            if (_signalNoise == null)
                _signalNoise = (L - A * X);

            var predictedHeight = X + covMN.Transpose() * CovNn_D_InvSn(k, c, sn);             
          
            return predictedHeight[0, 0];
        }
           
        public List<CommonPoint> ClosestCommonPointList(int numberOfPoints, double lat, double lon)
        { 
            return CommonPointList.OrderBy(x => x.GetDistance(lat, lon)).Take(numberOfPoints).ToList();
        }

        public Matrix<double> D(double sn)
        {         
            Matrix<double> d = Matrix<double>.Build.Dense(NumberOfPoints, NumberOfPoints);

            for (int i = 0; i < NumberOfPoints; i++)
                d[i, i] = sn * sn;

            return d;
        }

        public Matrix<double> CovNn_D_Inv(double k, double c, double sn)
        {
            if (_covNn_D_Inv != null)
                return _covNn_D_Inv;

            _covNn_D_Inv = (CovNn(k, c) + D(sn)).Inverse();

            return _covNn_D_Inv;
        }

        public Matrix<double> CovNn_D_InvSn(double k, double c, double sn)
        {
            if (_covNn_D_InvSn != null)
                return _covNn_D_InvSn;

            _covNn_D_InvSn = (CovNn(k, c) + D(sn)).Inverse() * _signalNoise;

            return _covNn_D_InvSn;
        }

        public Matrix<double> CovNn(double k, double c)
        {
            if (_covNn != null)
                return _covNn;

            _covNn = Matrix<double>.Build.Dense(NumberOfPoints, NumberOfPoints);
            
            foreach (var p1 in ValidPointList)
            {
                var index1 = ValidPointList.IndexOf(p1);
                foreach (var p2 in ValidPointList)
                {
                    var dist = p1.GetDistance(p2);
                    var v = k * Math.Exp(-(Math.PI / 2) * (dist / c));
                    var index2 = ValidPointList.IndexOf(p2);

                    _covNn[index1, index2] = v;
                }
            }
            return _covNn;
        }

        public Matrix<double> CovNnInv(double k, double c)
        {
            if (_covNnInv != null)
                return _covNnInv;

            _covNnInv = Matrix<double>.Build.Dense(NumberOfPoints, NumberOfPoints);
          
            foreach (var p1 in ValidPointList)
            {
                var index1 = ValidPointList.IndexOf(p1);
                foreach (var p2 in ValidPointList)
                {
                    var dist = p1.GetDistance(p2);
                    var v = k * Math.Exp(-(Math.PI / 2) * (dist / c));
                    var index2 = ValidPointList.IndexOf(p2);

                    _covNnInv[index1, index2] = v;
                }
            }        
            _covNnInv = _covNnInv.Inverse();
            return _covNnInv;
        }

        public Matrix<double> CovMn(double k, double c, double lat, double lon)
        {
            var covMN = Matrix<double>.Build.Dense(NumberOfPoints, 1);

            var tempPoint = new CommonPointXYZ()
            {
                Phi_SourceDeg = lat,
                Lambda_SourceDeg = lon,
                Phi_TargetDeg = lat,
                Lambda_TargetDeg = lon
            };

            // NOTE: Limit interpolation:
            // if (PointList.Min(x => x.GetDistance(lat, lon)) > 50000d)
            // return covMN;

            foreach (var p in ValidPointList)
            {
                var d = p.GetDistance(tempPoint);
                var v = k * Math.Exp(-(Math.PI / 2) * (d / c));
                var index = ValidPointList.IndexOf(p);
                covMN[index, 0] = v;
            }
            return covMN;
        }

        public Matrix<double> L
        {
            get
            {
                if (_l == null)
                {
                    _l = Matrix<double>.Build.Dense(NumberOfPoints, 1);

                    foreach (var p in ValidPointList)
                    {
                        var dH = p.H_Target - p.H_Source;
                        var index = ValidPointList.IndexOf(p);
                        _l[index, 0] = dH;
                    }
                }
                return _l;
            }           
        }

        public Matrix<double> X { get; set; }

        public override bool PopulatedGrid(double k, double c, double sn)
        {
            Data.Clear();

            var l = 0;

            for (var i = NRows - 1; i >= 0; i--)
            {
                var lat = LowerLeftLatitude + DeltaLatitude * i;
                for (var j = 0; j < NColumns; j++)
                {
                    var lon = LowerLeftLongitude + DeltaLongitude * j;
                    var h = PredictedHeight(k, c, sn, lat, lon);
                
                    Data.Add((float)h);
                    l++;
                }
                Console.Clear();
                Console.Write($"Processing vgrid...  { (int)(100 * l / (NRows * NColumns))} %");
            }
            return true;
        }
  
        public override bool GenerateGridFile(string outputFileName, bool isRandom = false)
        {
            OutputFileName = outputFileName;

            if (OutputFileName == "")
                return false;

            using (FileStream fs = new FileStream(OutputFileName, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(BitConverter.GetBytes(LowerLeftLatitude).Reverse().ToArray());
                    bw.Write(BitConverter.GetBytes(LowerLeftLongitude).Reverse().ToArray());
                    bw.Write(BitConverter.GetBytes(DeltaLatitude).Reverse().ToArray());
                    bw.Write(BitConverter.GetBytes(DeltaLongitude).Reverse().ToArray());
                    bw.Write(BitConverter.GetBytes(NRows).Reverse().ToArray());
                    bw.Write(BitConverter.GetBytes(NColumns).Reverse().ToArray());
                 
                    if (isRandom)
                        foreach (var v in GridData)
                            bw.Write(BitConverter.GetBytes(v).Reverse().ToArray());            
                    else
                    {
                        int c = 0; int r = NRows;

                        for (int i = 0; i < Data.Count(); i++)
                        {
                            int index = (r - 1) * NColumns + c;
                            c++;
                            if (c == NColumns)
                            {
                                r--;
                                c = 0;
                            }
                            var v = Data.ElementAtOrDefault(index);
                            bw.Write(BitConverter.GetBytes(v).Reverse().ToArray());
                        }
                    }
                    bw.Close();
                }
                fs.Close();

                return true;
            }
        }

        public bool ReadGtx(string gtxFilename)
        {
            Data.Clear();

            if (!File.Exists(gtxFilename))
                return false;           

            using (FileStream fs = new FileStream(gtxFilename, FileMode.Open))
            {
                if (fs.Length < 40)
                {
                    fs.Close();
                    return false;
                }

                using (BinaryReader br = new BinaryReader(fs))
                {
                    LowerLeftLatitude = BitConverter.ToDouble(br.ReadBytes(sizeof(double)).Reverse().ToArray(), 0);
                    LowerLeftLongitude = BitConverter.ToDouble(br.ReadBytes(sizeof(double)).Reverse().ToArray(), 0);
                    DeltaLatitude = BitConverter.ToDouble(br.ReadBytes(sizeof(double)).Reverse().ToArray(), 0);
                    DeltaLongitude = BitConverter.ToDouble(br.ReadBytes(sizeof(double)).Reverse().ToArray(), 0);
                    NRows = BitConverter.ToInt32(br.ReadBytes(sizeof(int)).Reverse().ToArray(), 0);
                    NColumns = BitConverter.ToInt32(br.ReadBytes(sizeof(int)).Reverse().ToArray(), 0);

                    var noOfBytes = br.BaseStream.Position;
                    var index = 0;

                    while (br.BaseStream.Position < fs.Length)
                    {
                        var h = BitConverter.ToSingle(br.ReadBytes(sizeof(int)).Reverse().ToArray(), 0);

                        if (h == 9999f)
                        {
                            // h = -88.8888f;                    
                            h = -32768;
                        }

                        // if (h != -88.8888f)
                        // if (h != -32768)
                        //   Console.Write($"Value: {h} Index: {index}");

                        if (index == NColumns)
                            index = 0;

                        Data.Insert(index++, h);

                        noOfBytes += 4;
                    }
                    br.Close();
                }
                fs.Close();
            }
            return true;
        }

        public bool ReadBin(string binFileName)
        {          
            var index = 0;
            var col = 0;

            Data.Clear();

            if (!File.Exists(binFileName))
                return false;

            try
            {
                using (FileStream fs = new FileStream(binFileName, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        if (br.BaseStream.Length < 64)
                        {
                            br.Close();
                            fs.Close();

                            return false;
                        }

                        var fortranCode = br.ReadInt32();

                        _gridParam.LowerLeftLatitude = br.ReadDouble();
                        var upperlat = br.ReadDouble();

                        _gridParam.LowerLeftLongitude = br.ReadDouble();
                        var upperlon = br.ReadDouble();

                        _gridParam.DeltaLatitude = br.ReadDouble();
                        _gridParam.DeltaLongitude = br.ReadDouble();

                        _gridParam.NColumns = (int)((upperlon - _gridParam.LowerLeftLongitude) / _gridParam.DeltaLongitude) + 1;
                        _gridParam.NRows = (int)((upperlat - _gridParam.LowerLeftLatitude) / _gridParam.DeltaLatitude) + 1;

                        var emptyBytes = br.ReadBytes(12);                     

                        while (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            var data = br.ReadSingle();

                            if (col == _gridParam.NColumns)
                            {
                                if (data == 0f)
                                    continue;
                                else
                                    col = 0;
                            }
                            
                            // Note: Max/min filter
                            if (data > 100f || data < -100f)
                                data = -88.8888f;

                            if (data == 9999f || data == 9999.999f)
                                data = -88.8888f;
 
                            col++;

                            Data.Insert(index++, data); 
                        }
                        br.Close();
                    }
                    fs.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override bool ReadSourceFromFile(string inputFile)
        {
            try
            {
                var reader = new StreamReader(File.OpenRead(inputFile));

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (values.Count() < 5)
                    {
                        reader.Close();
                        return false;
                    }

                    var name = values[0];
                    if (!double.TryParse(values[1], out double lon) ||
                        !double.TryParse(values[2], out double lat) ||
                        !double.TryParse(values[3], out double h) ||
                        !double.TryParse(values[4], out double epoch))
                        continue;
                    else
                    {
                        CommonPointXYZ cpPoint;

                        if (PointList.Any(p => p.PointName == name))
                        {
                            cpPoint = PointList.Find(p => p.PointName == name);
                            cpPoint.X_Source = lon;
                            cpPoint.Y_Source = lat;
                            cpPoint.Z_Source = h;
                            cpPoint.Epoch = epoch;
                        }
                        else
                        {
                            cpPoint = new CommonPointXYZ
                            {
                                PointName = name,
                                Lambda_SourceDeg = lon,
                                Phi_SourceDeg = lat,
                                H_Source = h,
                                Epoch = epoch
                            };
                            PointList.Add(cpPoint);
                        }
                    }
                }
                reader.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }

        public override bool ReadTargetFromFile(string inputFile)
        {
            try
            {
                var reader = new StreamReader(File.OpenRead(inputFile));

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (values.Count() < 5)
                    {
                        reader.Close();
                        return false;
                    }
                    var name = values[0];
                    if (!double.TryParse(values[1], out double lon) ||
                        !double.TryParse(values[2], out double lat) ||
                        !double.TryParse(values[3], out double h) ||
                        !double.TryParse(values[4], out double epoch))
                        continue;
                    else
                    {
                        CommonPointXYZ cpPoint;

                        if (PointList.Any(p => p.PointName == name))
                        {
                            cpPoint = PointList.Find(p => p.PointName == name);
                            cpPoint.Lambda_TargetDeg = lon;
                            cpPoint.Phi_TargetDeg = lat;
                            cpPoint.H_Target = h;
                            cpPoint.Epoch = epoch;
                        }
                        else
                        {
                            cpPoint = new CommonPointXYZ
                            {
                                PointName = name,
                                Lambda_TargetDeg = lon,
                                Phi_TargetDeg = lat,
                                H_Target = h,
                                Epoch = epoch
                            };
                            PointList.Add(cpPoint);
                        }
                    }
                }
                reader.Close();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override bool ClipGrid(double west_long, double south_lat, double east_long, double north_lat)
        {
            if (_griFile == null)
                return false;

            return _griFile.ClipGrid(west_long, south_lat, east_long, north_lat);            
        }

        public void CleanNullPoints()
        {
            PointList.RemoveAll(x => x.HasNullValues);
        }
    }
}
