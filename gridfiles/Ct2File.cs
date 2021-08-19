using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GeoJSON.Net;
using MathNet.Numerics.LinearAlgebra;
using NetTopologySuite.Geometries;

namespace gridfiles
{
    public class Ct2File : GridFile
    {
        private Matrix<double> _covNn = null;
        private Matrix<double> _covNnInv = null;
        private Matrix<double> _covNn_D_Inv = null;
        private Matrix<double> _covNn_D_InvSn = null;
        private Matrix<double> _a = null;
        private Matrix<double> _signalNoise = Matrix<double>.Build.Dense(1, 1);
        private GriFile _griNorth;
        private GriFile _griEast;
        private CptFile _cptFile = new CptFile();
        private GridParam _gridParam;
        private List<Polygon> _polygonList = new List<Polygon>();
        private const double Ro = Math.PI / 180;
        private char[] _id = new char[80];

        public Ct2File()
        {
            _gridParam = new GridParam();
            _griNorth = new GriFile(_gridParam);
            _griEast = new GriFile(_gridParam);
        }

        public Ct2File(GridParam gridParam)
        {
            _gridParam = gridParam;
            _griNorth = new GriFile(_gridParam);
            _griEast = new GriFile(_gridParam);
        } 

        public Ct2File(string fileNameN, string fileNameE)
        {
            _gridParam = new GridParam();
            _griNorth = new GriFile(fileNameN,  _gridParam);
            _griEast = new GriFile(fileNameE, _gridParam);             
        }

        public Ct2File(string fileNameN, string fileNameE, GridParam gridParam)
        {
            _gridParam = gridParam;
            _griNorth = new GriFile(fileNameN, _gridParam);
            _griEast = new GriFile(fileNameE, _gridParam);
        }

        public CptFile CptFile
        {
            get => _cptFile;
            set => _cptFile = value;
        }
        
        public List<CommonPoint> CommonPointList
        {
            get => _cptFile.CommonPointList;
            set => _cptFile.CommonPointList = value;
        }

        public List<float> GridData { get; set; } = new List<float>();

        public List<Tuple<string, double, double>> System1PointList { get; set; } = new List<Tuple<string, double, double>>();

        public List<Tuple<string, double, double>> System2PointList { get; set; } = new List<Tuple<string, double, double>>();

        public GriFile GriNorth
        {
            get => _griNorth;
            set => _griNorth = value;
        }

        public GriFile GriEast
        {
            get => _griEast;
            set => _griEast = value;
        }        

        public double FalseLon { get; set; }

        public double FalseLat { get; set; }

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

        public double UpperLeftLatitude => _gridParam.UpperLeftLatitude;

        public double UpperLeftLongitude => _gridParam.UpperLeftLongitude;

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

        public double C0 { get; set; } = 0d;

        public double Sn { get; set; } = 0d;

        public double Cl { get; set; } = 0d;

        public double Apar { get; set; } = 1d;

        public double Bpar { get; set; } = 0d;

        public double Tx { get; set; } = 0d;

        public double Ty { get; set; } = 0d;

        public string Description
        {
            get => new string(_id);
            set
            {
                _id = value.ToCharArray();
                Array.Resize(ref _id, 80);
            }
        }

        internal int NumberOfPoints => PointList.Count();

        internal bool HelmertIsComputed => Apar != 1d || Bpar != 0d || Tx != 0d || Ty != 0d;
        
        internal double CosLat(double lat)
        {
            return Math.Cos(lat * Ro);
        }

        internal double MeanLat
        {
            get
            {
                if (PointList.Count() == 0)
                    return 0d;

                return PointList.Sum(x => x.Phi1Deg) / PointList.Count();
            }
        }

        public Matrix<double> A
        {
            get
            {
                if (_a == null)
                {
                    _a = Matrix<double>.Build.Dense(2 * NumberOfPoints, 4);

                    foreach (var p in PointList)
                    {
                        var index = PointList.IndexOf(p);

                        _a[index * 2 + 0, 0] = p.Lambda1Deg * CosLat(MeanLat);
                        _a[index * 2 + 0, 1] = p.Phi1Deg;
                        _a[index * 2 + 0, 2] = 1d;
                        _a[index * 2 + 0, 3] = 0d;

                        _a[index * 2 + 1, 0] = p.Phi1Deg;
                        _a[index * 2 + 1, 1] = -p.Lambda1Deg * CosLat(MeanLat);
                        _a[index * 2 + 1, 2] = 0d;
                        _a[index * 2 + 1, 3] = 1d;
                    }
                }
                return _a;
            }
        }
       
        public Matrix<double> L
        {
            get
            {
                var l = Matrix<double>.Build.Dense(2 * NumberOfPoints, 1);                

                var r = Matrix<double>.Build.Dense(2, 2);
                r[0, 0] = Apar; r[0, 1] = Bpar;
                r[1, 0] = -Bpar; r[1, 1] = Apar;

                var t = Matrix<double>.Build.Dense(2, 1);
                t[0, 0] = Tx;
                t[1, 0] = Ty;

                foreach (var point in PointList.Where(x => !x.HasNullValues))
                {
                    var index = PointList.IndexOf(point);

                    var p1 = Matrix<double>.Build.Dense(2, 1);
                    p1[0, 0] = point.Lambda1Deg * CosLat(MeanLat);
                    p1[1, 0] = point.Phi1Deg;

                    var p = t + r * p1;

                    l[index * 2 + 0, 0] = point.Lambda2Deg * CosLat(MeanLat) - p[0, 0];
                    l[index * 2 + 1, 0] = point.Phi2Deg - p[1, 0];
                }
                return l;
            }
        }

        public Matrix<double> X { get; set; } = Matrix<double>.Build.Dense(4, 1);

        public bool PopulateGrid(double k, double c, double sn)
        {
            C0 = k;
            Cl = c;
            Sn = sn;

            _griEast.Data.Clear();
            _griNorth.Data.Clear();

            if (!Helmert(k, c, sn))
                return false;

            var l = 0;

            for (var i = NRows - 1; i >= 0; i--)
            {
                var lat = LowerLeftLatitude + DeltaLatitude * i;
                for (var j = 0; j < NColumns; j++)
                {
                    var lon = LowerLeftLongitude + DeltaLongitude * j;
                    var pos = PredictedPosition(k, c, sn, lat, lon);

                    if (pos.Item1 == -88.8888 && pos.Item2 == -88.8888)
                    {
                        _griEast.Data.Add(float.NaN);
                        _griNorth.Data.Add(float.NaN);
                    }
                    else
                    {
                        _griEast.Data.Add((float)(-Ro * pos.Item2));
                        _griNorth.Data.Add((float)(Ro * pos.Item1));
                    }
                    l++;
                }
                Console.Clear();
                Console.Write($"Processing hgrid...  { (int)(100 * l / (NRows * NColumns))} %");
            }
            return true;
        }

        public bool Helmert(double k, double c, double sn)
        {
            try
            {
                var iterations = 0;

                do
                { 
                    X = (A.Transpose() * CovNn_D_Inv(k, c, sn) * A).Inverse() * (A.Transpose() * CovNn_D_Inv(k, c, sn) * L);

                    Apar += X[0, 0];
                    Bpar += X[1, 0];
                    Tx += X[2, 0];
                    Ty += X[3, 0];

                    iterations++;
                } while (!X.ForAll(x => Math.Abs(x) < 1E-8) && iterations < 10);

                _signalNoise = L - A * X;

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }

        public Tuple<double, double> PredictedPosition(double k, double c, double sn, double lat, double lon)
        { 
            var helmert = HelmertPosition(lat, lon);
            var signalObs = CovMn(k, c, lat, lon).Transpose() * CovNn_D_InvSn(k, c, sn);

            var lonHelmert = helmert[0, 0] / CosLat(MeanLat);
            var latHelmert = helmert[1, 0];

            if (signalObs.ForAll(cc=>cc == 0))
                return new Tuple<double, double>(-88.8888, -88.8888);

            var lonSignal = signalObs[0, 0] / CosLat(MeanLat);
            var latSignal = signalObs[1, 0];

            // NOTE: When empty data
            // return new Tuple<double, double>(-88.88880d, -88.88880d);
          
            var latPredicted = latHelmert + latSignal;
            var lonPredicted = lonHelmert + lonSignal;
 
            var tuple = new Tuple<double, double>(latPredicted - lat, lonPredicted - lon);

            return tuple;
        }

        public Matrix<double> D(double sn)
        {
            Matrix<double> d = Matrix<double>.Build.Dense(2 * NumberOfPoints, 2 * NumberOfPoints);

            for (int i = 0; i < 2 * NumberOfPoints; i++)
                d[i, i] = sn * sn;

            return d;
        }

        public Matrix<double> CovNn_D_InvSn(double k, double c, double sn)
        {
            if (_covNn_D_InvSn != null)
                return _covNn_D_InvSn;

            _covNn_D_InvSn = (CovNn(k, c) + D(sn)).Inverse() * _signalNoise;

            return _covNn_D_InvSn;
        }      

        public Matrix<double> CovNn_D_Inv(double k, double c, double sn)
        {
            if (_covNn_D_Inv != null)
                return _covNn_D_Inv;

            _covNn_D_Inv = (CovNn(k, c) + D(sn)).Inverse();

            return _covNn_D_Inv;
        }      

        public Matrix<double> CovNnInv(double k, double c)
        {
            if (_covNnInv != null)
                return _covNnInv;

            _covNnInv = CovNn(k, c).Inverse();

            return _covNnInv;
        }
 
        public Matrix<double> CovNn(double k, double c)
        {
            if (_covNn != null)
                return _covNn;

            _covNn = Matrix<double>.Build.Dense(2 * NumberOfPoints, 2 * NumberOfPoints);
            
            foreach (var p1 in PointList)
            {
                var index1 = PointList.IndexOf(p1);
                foreach (var p2 in PointList)
                {
                    var d = p1.GetDistance(p2);
                    var v = k * Math.Exp(-(Math.PI / 2) * (d / c));
                    var index2 = PointList.IndexOf(p2);

                    _covNn[index1 * 2 + 0, index2 * 2 + 0] = v;
                    _covNn[index1 * 2 + 1, index2 * 2 + 1] = v;
                }
            }
            return _covNn;
        }

        public Matrix<double> CovMn(double k, double c, double lat, double lon)
        {
            var covMN = Matrix<double>.Build.Dense(2 * NumberOfPoints, 2);

            var tempPoint = new CommonPointXYZ()
            {
                Phi1Deg = lat,
                Lambda1Deg = lon,
                Phi2Deg = lat,
                Lambda2Deg = lon
            };

            //  NOTE: Limit interpolation:
            // if (PointList.Min(x => x.GetDistance(lat, lon)) > 50000d)
            //    return covMN;

            foreach (var p in PointList)
            {
                var d = p.GetDistance(tempPoint);
                var v = k * Math.Exp(-(Math.PI / 2) * (d / c));
                var index = PointList.IndexOf(p);

                covMN[index * 2 + 0, 0] = v;
                covMN[index * 2 + 1, 1] = v;
            }
            return covMN;
        }

        internal Matrix<double> HelmertPosition(double lat, double lon)
        {
            if (!HelmertIsComputed)
                return Matrix<double>.Build.Dense(2, 1);

            var r = Matrix<double>.Build.Dense(2, 2);
            r[0, 0] = Apar; r[0, 1] = Bpar;
            r[1, 0] = -Bpar; r[1, 1] = Apar;

            var t = Matrix<double>.Build.Dense(2, 1);
            t[0, 0] = Tx;
            t[1, 0] = Ty;

            var pin = Matrix<double>.Build.Dense(2, 1);
            pin[0, 0] = lon * CosLat(MeanLat);
            pin[1, 0] = lat;

            return t + r * pin;
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
                    var header = "CTABLE V2.0     ".ToCharArray();

                    bw.Write(header);
                    bw.Write(_id);
                    bw.Write(LowerLeftLongitude * Ro);
                    bw.Write(LowerLeftLatitude * Ro);
                    bw.Write(DeltaLongitude * Ro);
                    bw.Write(DeltaLatitude * Ro);
                    bw.Write(NColumns);
                    bw.Write(NRows);
                    
                    for (int i = 0; i < 24; i++)
                        bw.Write(new Byte());

                    if (isRandom)
                        foreach (var v in GridData)
                            bw.Write(v);
                    else
                    {
                        int c = 0; int r = NRows;

                        if (_griNorth.Data.Count() != _griEast.Data.Count())
                            return false;

                        for (var i = 0; i < _griNorth.Data.Count(); i++)
                        {                                                        
                            int index = (r - 1) * NColumns + c;
                            c++;

                            if (c == NColumns)
                            {
                                r--;
                                c = 0;
                            }                             
                           
                            var eastData = _griEast.Data.ElementAtOrDefault(i);
                            var northData = _griNorth.Data.ElementAtOrDefault(i);

                            // With Gri format
                            /*
                            var eastData = _griEast.Data.ElementAtOrDefault(index);
                            var northData = _griNorth.Data.ElementAtOrDefault(index);
                            */
                            bw.Write(eastData);
                            bw.Write(northData);
                        }
                    }
                    bw.Close();
                }
                fs.Close();
            }
            return true;
        }

        public bool ReadCt2(string ct2Filename, bool reverse = false)
        {
            _griEast.Data.Clear();
            _griNorth.Data.Clear();

            if (!File.Exists(ct2Filename))
                return false;

            using (FileStream fs = new FileStream(ct2Filename, FileMode.Open))
            {
                if (fs.Length < 160)
                {
                    fs.Close();
                    return false;
                }

                using (BinaryReader br = new BinaryReader(fs))
                {
                    // Header:
                    var chars = br.ReadChars(16);
                    string header = new string(chars);
                    
                    // Description
                    _id = br.ReadChars(80);

                    LowerLeftLongitude = br.ReadDouble() / Ro;
                    LowerLeftLatitude = br.ReadDouble() / Ro;
                    DeltaLongitude = br.ReadDouble() / Ro;
                    DeltaLatitude = br.ReadDouble() / Ro;
                    NColumns = br.ReadInt32();
                    NRows = br.ReadInt32();

                    // 24 empty bytes
                    var emptybytes = br.ReadBytes(24);
                    var noOfBytes = br.BaseStream.Position;
                    var index = 0;

                    var eastList = new List<List<float>>();
                    var northList = new List<List<float>>();

                    var eastRow = new List<float>();
                    var northRow = new List<float>();

                    while (br.BaseStream.Position < fs.Length)
                    {
                        var eastValue = br.ReadSingle();
                        var northValue = br.ReadSingle();

                        if (index == NColumns)
                        {
                            eastList.Add(eastRow);
                            northList.Add(northRow);

                            eastRow = new List<float>();
                            northRow = new List<float>();

                            index = 0;
                        }
                        index++;

                        eastRow.Add(eastValue);
                        northRow.Add(northValue);

                        noOfBytes += 8;
                    }
                    eastList.Add(eastRow);
                    northList.Add(northRow);

                    // TODO: Fails reading for merging
                    if (reverse)
                    {
                        eastList.Reverse();
                        northList.Reverse();
                    }
                    
                    foreach (var row in eastList)
                         foreach (var col in row)
                            _griEast.Data.Add(col);

                    foreach (var row in northList)
                        foreach (var col in row)
                            _griNorth.Data.Add(col);

                    br.Close();
                }
                fs.Close();
            }
            return true;
        }

        public bool ReadSystem1PointList(string filNamn)
        {
            System1PointList.Clear();

            if (!ReadPointList(filNamn, System1PointList))
                return false;

            return true;
        }

        public bool ReadSystem2PointList(string filNamn)
        {
            System2PointList.Clear();

            if (!ReadPointList(filNamn, System2PointList))
                return false;

            return true;
        }

        private bool ReadPointList(string fileName, List<Tuple<string, double, double>> systemPointList)
        {
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        string line;

                        while ((line = streamReader.ReadLine()) != null)
                        {
                            string[] linearray = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (linearray.Count() != 3)
                                continue;

                            var x = 0d; var y = 0d;

                            if (!double.TryParse(linearray[2], out x))
                                continue;

                            if (!double.TryParse(linearray[1], out y))
                                continue;

                            systemPointList.Add(Tuple.Create(linearray[0], x, y));
                        }
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
                return true;
            }
            catch
            {
                systemPointList.Clear();
                return false;
            }
        }

        public bool ComputeParameters()
        {
            var systemPointList = System1PointList.Count() > System2PointList.Count() ? System1PointList : System2PointList;
            
            LowerLeftLatitude = systemPointList.Min(x => x.Item3);
            LowerLeftLongitude = systemPointList.Min(x => x.Item2);

            var upperRightLatitude = systemPointList.Max(x => x.Item3);
            var upperRightLongitude = systemPointList.Max(x => x.Item2);

            // North:
            NRows = systemPointList.GroupBy(x => x.Item3).Count();

            if (NRows < 2)
                return false;

            // East:
            NColumns = systemPointList.GroupBy(x => x.Item2).Count();

            if (NColumns < 2)
                return false;

            DeltaLatitude = (upperRightLatitude - LowerLeftLatitude) / (NRows - 1);
            DeltaLongitude = (upperRightLongitude - LowerLeftLongitude) / (NColumns - 1);

            return true;
        }

        public bool ComputeGridData(GridFile.Direction dir)
        {
            var outer = System1PointList.Count() > System2PointList.Count() ? System1PointList : System2PointList;
            var inner = System1PointList.Count() > System2PointList.Count() ? System2PointList : System1PointList;
            var joinPointList = outer.GroupJoin(inner, x => x.Item1, y => y.Item1, (x, y) => new { sys1 = x, sys2 = y });

            foreach (var element in joinPointList)
            {
                var pointName1 = element.sys1.Item1;
                var lon1 = element.sys1.Item2 + FalseLon;
                var lat1 = element.sys1.Item3 + FalseLat;
                var lon2 = 0d;
                var lat2 = 0d;

                if (IsInSidePolygons(new NetTopologySuite.Geometries.Point(lon1, lat1)))
                {
                    _griEast.Data.Add(0f);
                    _griNorth.Data.Add(0f);

                   // _griEast.Data.Add(float.NaN);
                   // _griNorth.Data.Add(float.NaN);

                    continue;
                }
                if (!element.sys2.Any())
                {
                    _griEast.Data.Add(float.NaN);
                    _griNorth.Data.Add(float.NaN);
                }
                else
                {
                    var pointName2 = element.sys2.FirstOrDefault().Item1;
                    lon2 = element.sys2.FirstOrDefault().Item2 + FalseLon;
                    lat2 = element.sys2.FirstOrDefault().Item3 + FalseLat;

                    if (dir == Direction.fwd)
                    {
                        _griEast.Data.Add((float)(-Ro * (lon2 - lon1)));
                        _griNorth.Data.Add((float)(Ro * (lat2 - lat1)));
                    }
                    else
                    {
                        _griEast.Data.Add((float)(Ro * (lon2 - lon1)));
                        _griNorth.Data.Add((float)(-Ro * (lat2 - lat1)));
                    }
                }
            }
            return true;
        }
        
        public bool ReadGriFiles()
        {
            if (_griNorth == null || _griEast == null)
                return false;

            if (!_griNorth.ReadGridFile())
                return false;

            if (!_griEast.ReadGridFile())
                return false;
            
            return true;
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

                        if (PointList.Any(p => p.Name == name))
                        {
                            cpPoint = PointList.Find(p => p.Name == name);
                            cpPoint.X1 = lon;
                            cpPoint.Y1 = lat;
                            cpPoint.Z1 = h;
                            cpPoint.Time = epoch;
                        }
                        else
                        {
                            cpPoint = new CommonPointXYZ
                            {
                                Name = name,
                                Lambda1Deg = lon,
                                Phi1Deg = lat,
                                H1 = h,
                                Time = epoch
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

                        if (PointList.Any(p => p.Name == name))
                        {
                            cpPoint = PointList.Find(p => p.Name == name);
                            cpPoint.Lambda2Deg = lon;
                            cpPoint.Phi2Deg = lat;
                            cpPoint.H2 = h;
                            cpPoint.Time = epoch;
                        }
                        else
                        {
                            cpPoint = new CommonPointXYZ
                            {
                                Name = name,
                                Lambda2Deg = lon,
                                Phi2Deg = lat,
                                H2 = h,
                                Time = epoch
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

        public void CleanNullPoints()
        {
            PointList.RemoveAll(x => x.HasNullValues);
        }

        public bool ReadGeoJsonAreas(string geoJsonFile)
        {
            if (!File.Exists(geoJsonFile))
                return false;

            using (var sr = new StreamReader(geoJsonFile))
            {
                string s = sr.ReadToEnd();

                var reader = new NetTopologySuite.IO.GeoJsonReader();
                var featureCollection = reader.Read<GeoJSON.Net.Feature.FeatureCollection>(s);

                foreach (var feature in featureCollection.Features)
                {
                    List<NetTopologySuite.Geometries.Coordinate> coordinates;

                    switch (feature.Geometry.Type)
                    {
                        case GeoJSONObjectType.Point:
                            break;
                        case GeoJSONObjectType.MultiPoint:                          
                            break;
                        case GeoJSONObjectType.Polygon:
                            var polygon = feature.Geometry as GeoJSON.Net.Geometry.Polygon;
                           
                            foreach (var poly in polygon.Coordinates)
                            {
                                coordinates = new List<NetTopologySuite.Geometries.Coordinate>();
                                if (poly.IsLinearRing())
                                {
                                    foreach (var coordinate in poly.Coordinates)
                                    {
                                        var location = coordinate as GeoJSON.Net.Geometry.Position;

                                        if (location == null)
                                            continue;

                                        coordinates.Add(new NetTopologySuite.Geometries.Coordinate(location.Longitude, location.Latitude));
                                    }
                                }
                                _polygonList.Add(new Polygon(new LinearRing(coordinates.ToArray())));                               
                            }
                            break;
                        case GeoJSONObjectType.MultiPolygon:
                            var multiPolygon = feature.Geometry as GeoJSON.Net.Geometry.MultiPolygon;
                            foreach (var mpoly in multiPolygon.Coordinates)
                            {
                                coordinates = new List<NetTopologySuite.Geometries.Coordinate>();
                                foreach (var poly in mpoly.Coordinates)
                                {
                                    if (poly.IsLinearRing())
                                    {
                                        foreach (var coordinate in poly.Coordinates)
                                        {
                                            var location = coordinate as GeoJSON.Net.Geometry.Position;

                                            if (location == null)
                                                continue;

                                            coordinates.Add(new NetTopologySuite.Geometries.Coordinate(location.Longitude, location.Latitude));
                                        }
                                    }
                                    _polygonList.Add(new Polygon(new LinearRing(coordinates.ToArray())));
                                }
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                sr.Close();             
            }
            return true;
        }

        public bool IsInSidePolygons(NetTopologySuite.Geometries.Point point)
        {
            foreach (var poly in _polygonList)        
                if (poly.Covers(point))
                    return true;
           
            return false;
        }
    }

    // TODO: Move to new file
    public class MergedCt2File : Ct2File
    {
        public MergedCt2File(Ct2File grid1, Ct2File grid2)
        {
            Grid1 = grid1;
            Grid2 = grid2;

            this.NColumns = Grid1.NColumns;
            this.NRows = Grid1.NRows;
            this.LowerLeftLatitude = Grid1.LowerLeftLatitude;
            this.LowerLeftLongitude = Grid1.LowerLeftLongitude;
            this.DeltaLatitude = Grid1.DeltaLatitude;
            this.DeltaLongitude = Grid1.DeltaLongitude;
        }

        public Ct2File Grid1 { get; set; } = new Ct2File();
        public Ct2File Grid2 { get; set; } = new Ct2File();

        public bool MergeGrids()
        {
            var index = 0;

            if (Grid1.GriEast.IsEmpty || Grid1.GriNorth.IsEmpty || Grid2.GriEast.IsEmpty || Grid2.GriNorth.IsEmpty)
                return false;        

            foreach (var data1 in Grid1.GriEast.Data)
            {
                var data2 = Grid2.GriEast.Data.ElementAt(index++);

                if (!float.IsNaN(data1) && !float.IsNaN(data2))
                    GriEast.Data.Add(data2);
                else if (!float.IsNaN(data1))
                    GriEast.Data.Add(data1);               
                else
                    GriEast.Data.Add(data2);
            }

            index = 0;

            foreach (var data1 in Grid1.GriNorth.Data)
            {
                var data2 = Grid2.GriNorth.Data.ElementAt(index++);

                if (!float.IsNaN(data1) && !float.IsNaN(data2))
                    GriNorth.Data.Add(data2);
                else if (!float.IsNaN(data1))
                    GriNorth.Data.Add(data1);
                else
                    GriNorth.Data.Add(data2);
            }

            // TODO: Remove code
            var count = GriEast.Data.Count(x => float.IsNaN(x));
            var count1 = Grid1.GriEast.Data.Count(x => float.IsNaN(x));
            var count2 = Grid2.GriEast.Data.Count(x => float.IsNaN(x));

            return true;
        }
    }
}
