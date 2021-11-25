using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace gridfiles
{
    public class CommonPointSet : GridFile
    {
        private Matrix<double> _covNn = null;
        private Matrix<double> _covNnInv = null;
        private Matrix<double> _covNn_D_Inv = null;
        private Matrix<double> _covNn_D_InvSn = null;
        private Matrix<double> _a = null;
        private Matrix<double> _v = null;
        private Matrix<double> _signalNoise = Matrix<double>.Build.Dense(1, 1);

        private GriFile _griX;
        private GriFile _griY;
        private GriFile _griZ;
        private GridParam _gridParam;

        private double _factor = 1000000d;

        private List<CommonPointXYZ> _list;

        public CommonPointSet()
        {
            _gridParam = new GridParam();

            _griX = new GriFile(_gridParam);
            _griY = new GriFile(_gridParam);
            _griZ = new GriFile(_gridParam);
        }

        public CommonPointSet(GridParam gridParam)
        {
            _gridParam = gridParam;

            _griX = new GriFile(_gridParam);
            _griY = new GriFile(_gridParam);
            _griZ = new GriFile(_gridParam);            
        }

        public GriFile GriX
        {
            get => _griX;
            set => _griX = value;
        }

        public GriFile GriY
        {
            get => _griY;
            set => _griY = value;
        }

        public GriFile GriZ
        {
            get => _griZ;
            set => _griZ = value;
        }
        
        public double X0
        {
            get => 0d;// PointList.Average(x => x.X1);
        }

        public double Y0
        {
            get => 0d;// PointList.Average(x => x.Y1);
        }

        public double Z0
        {
            get => 0d;// PointList.Average(x => x.Z1);
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

        public double UpperLeftLatitude
        {
            get => _gridParam.UpperLeftLatitude;
        }

        public double UpperLeftLongitude
        {
            get => _gridParam.UpperLeftLongitude;
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

        // Average Ground Level
        public double Agl { get; set; } = 0d;
        
        public Matrix<double> SignalNoise
        {
            get => _signalNoise;
            set => _signalNoise = value;
        }

        public double C0 { get; set; } = 0d;        

        public double Sn { get; set; } = 0d;   
    
        public double Cl { get; set; } = 0d;

        public double Rx { get; set; } = 0d;
      
        public double Ry { get; set; } = 0d;

        public double Rz { get; set; } = 0d;

        public double Tx { get; set; } = 0d;

        public double Ty { get; set; } = 0d;

        public double Tz { get; set; } = 0d;

        public double S { get; set; } = 1d;
        
        public Matrix<double> A
        {
            get
            {
                if (_a != null)
                    return _a;

                _a = Matrix<double>.Build.Dense(3 * NumberOfPoints, 7);

                foreach (var point in ValidPointList)
                {
                    var index = ValidPointList.IndexOf(point);

                    double x = (point.X1 - X0) / _factor;
                    double y = (point.Y1 - Y0) / _factor;
                    double z = (point.Z1 - Z0) / _factor;
                     
                    _a[index * 3 + 0, 0] = 0d; _a[index * 3 + 0, 1] =  z; _a[index * 3 + 0, 2] = -y; _a[index * 3 + 0, 3] = x; _a[index * 3 + 0, 4] = 1d; _a[index * 3 + 0, 5] = 0d; _a[index * 3 + 0, 6] = 0d;
                    _a[index * 3 + 1, 0] = -z; _a[index * 3 + 1, 1] = 0d; _a[index * 3 + 1, 2] =  x; _a[index * 3 + 1, 3] = y; _a[index * 3 + 1, 4] = 0d; _a[index * 3 + 1, 5] = 1d; _a[index * 3 + 1, 6] = 0d;
                    _a[index * 3 + 2, 0] =  y; _a[index * 3 + 2, 1] = -x; _a[index * 3 + 2, 2] = 0d; _a[index * 3 + 2, 3] = z; _a[index * 3 + 2, 4] = 0d; _a[index * 3 + 2, 5] = 0d; _a[index * 3 + 2, 6] = 1d;
                }               
                return _a;
            }
        }

        public Matrix<double> L
        {
            get
            {
                var l = Matrix<double>.Build.Dense(3 * NumberOfPoints, 1);

                var r = Matrix<double>.Build.Dense(3, 3);
                r[0, 0] = 1d; r[0, 1]  = -Rz; r[0, 2] = Ry;
                r[1, 0] = Rz; r[1, 1]  = 1d; r[1, 2]  = -Rx;
                r[2, 0] = -Ry; r[2, 1] = Rx; r[2, 2]  = 1d;

                var t = Matrix<double>.Build.Dense(3, 1);
                t[0, 0] = Tx;
                t[1, 0] = Ty;
                t[2, 0] = Tz;

                var t0 = Matrix<double>.Build.Dense(3, 1);
                t0[0, 0] = X0;
                t0[1, 0] = Y0;
                t0[2, 0] = Z0;

                foreach (var point in ValidPointList.Where(x => !x.HasNullValues))
                {
                    var index = ValidPointList.IndexOf(point);

                    var p1 = Matrix<double>.Build.Dense(3, 1);
                    p1[0, 0] = point.X1 - X0;
                    p1[1, 0] = point.Y1 - Y0;
                    p1[2, 0] = point.Z1 - Z0;

                    var p = t + t0 + S * r * p1;

                    l[index * 3 + 0, 0] = point.X2 - p[0, 0];
                    l[index * 3 + 1, 0] = point.Y2 - p[1, 0];
                    l[index * 3 + 2, 0] = point.Z2 - p[2, 0];
                }
                return l;
            }           
        }

        public Matrix<double> X { get; set; }

        public Matrix<double> V
        {
            get
            {
                if (X == null)
                    return null;

                if (_v != null)
                    return _v;

                return _v = L - A * X;
            }
        }
        
        public double Rms => HelmertIsComputed ? Math.Sqrt((V.Transpose() * V).At(0, 0) / (NumberOfPoints * 3)):0d;
             
        public double MinX => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 0).Min() : 0d;

        public double MinY => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 1).Min() : 0d;

        public double MinZ => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 2).Min() : 0d;

        public double MaxX => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 0).Max() : 0d;

        public double MaxY => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 1).Max() : 0d;

        public double MaxZ => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 2).Max() : 0d;

        public double MeanX => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 0).Sum() / NumberOfPoints : 0d;

        public double MeanY => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 1).Sum() / NumberOfPoints : 0d;
         
        public double MeanZ => HelmertIsComputed ? V.Enumerate().Where((x, i) => i % 3 == 2).Sum() / NumberOfPoints : 0d;

        public double AverageX => HelmertIsComputed ? Math.Sqrt(V.Enumerate().Where((x, i) => i % 3 == 0).Sum(y => (y - MeanX) * (y - MeanX)) / (NumberOfPoints - 1)) : 0d;

        public double AverageY => HelmertIsComputed ? Math.Sqrt(V.Enumerate().Where((x, i) => i % 3 == 1).Sum(y => (y - MeanY) * (y - MeanY)) / (NumberOfPoints - 1)) : 0d;

        public double AverageZ => HelmertIsComputed ? Math.Sqrt(V.Enumerate().Where((x, i) => i % 3 == 2).Sum(y => (y - MeanZ) * (y - MeanZ)) / (NumberOfPoints - 1)) : 0d;

        public double RmsX => HelmertIsComputed ? Math.Sqrt(V.Enumerate().Where((x, i) => i % 3 == 0).Sum(y => y * y) / NumberOfPoints) : 0d;

        public double RmsY => HelmertIsComputed ? Math.Sqrt(V.Enumerate().Where((x, i) => i % 3 == 1).Sum(y => y * y) / NumberOfPoints) : 0d;

        public double RmsZ => HelmertIsComputed ? Math.Sqrt(V.Enumerate().Where((x, i) => i % 3 == 2).Sum(y => y * y) / NumberOfPoints) : 0d;

        internal int NumberOfPoints => PointList.Count(x=> !x.HasNullValues);

        internal bool HelmertIsComputed => (NumberOfPoints > 0 && V != null && V.RowCount >= 3 && V.ColumnCount == 1) && (Rx != 0d || Ry != 0d || Rz != 0d || Tx != 0d || Ty != 0d || Tz != 0d || S != 1d);

        internal string HelmertResult
        {
            get
            {
                if (HelmertIsComputed)
                    return $" Helmert parameters: " +
                        $"Tx: {Tx,7:F5} m, " +
                        $"Ty: {Ty,7:F5} m, " +
                        $"Tz: {Tz,7:F5} m, " +
                        $"Rx: {Rx.ToArcSec(),11:F9}'', " +
                        $"Ry: {Ry.ToArcSec(),11:F9}'', " +
                        $"Rz: {Rz.ToArcSec(),11:F9}'', " +
                        $"Scale: {S.ToPpm(),11:F9} ppm, " +
                        $"Rms: {Rms,8:F6} m.";
                return "";
            }
        }

        internal string LscParameters
        {
            get
            {
                if (HelmertIsComputed)
                    return $" LSC parameters: " +
                        $"C0: {C0,7:F5} m2, " +
                        $"CL: {Cl/1000,2:F0} km, " +
                        $"Sn: {Sn,7:F5} m";
                return "";
            }
        }

        public Matrix<double> CovNn(double k, double c)
        {
            if (_covNn != null)
                return _covNn;

            _covNn = Matrix<double>.Build.Dense(3 * NumberOfPoints, 3 * NumberOfPoints);

            foreach (var p1 in ValidPointList)
            {
                var index1 = ValidPointList.IndexOf(p1);

                foreach (var p2 in ValidPointList)
                {
                    var index2 = ValidPointList.IndexOf(p2);
                    var d = p1.GetDistance(p2);
                    var v = k * Math.Exp(-(Math.PI / 2) * (d / c));

                    for (var i = 0; i < 3; i++)
                        _covNn[index1 * 3 + i, index2 * 3 + i] = v;
                }
            }
            return _covNn;
        }

        public Matrix<double> CovNnInv(double k, double c)
        {
            if (_covNnInv != null)
                return _covNnInv;

            _covNnInv = CovNn(k, c).Inverse();

            return _covNnInv;
        }

        public Matrix<double> CovMn(double k, double c, double x, double y, double z)
        {
            var covMN = Matrix<double>.Build.Dense(3 * NumberOfPoints, 3);
            var filter = false;

            if (filter)            
                if (PointList.Min(p => p.GetDistance(x, y, z)) > 100000d)
                    return covMN;                     

            foreach (var p in ValidPointList)
            {
                var d = p.GetDistance(x, y, z);
                var v = k * Math.Exp(-(Math.PI / 2) * (d / c));

                var index = ValidPointList.IndexOf(p);

                covMN[index * 3 + 0, 0] = v;
                covMN[index * 3 + 1, 1] = v;
                covMN[index * 3 + 2, 2] = v;
            }
            return covMN;
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

        public Matrix<double> D(double sn)
        {
            Matrix<double> d = Matrix<double>.Build.Dense(3 * NumberOfPoints, 3 * NumberOfPoints);

            for (int i = 0; i < 3 * NumberOfPoints; i++)
                d[i, i] = sn * sn;

            return d;
        }

        ///<Summary>
        /// This is a math function I found <see href="http://www.ipublishing.co.in/jggsarticles/volseven/EIJGGS7021.pdf">HERE</see>
        /// https://core.ac.uk/download/pdf/85211743.pdf
        /// https://core.ac.uk/download/pdf/81178247.pdf
        /// https://www.mdpi.com/2227-7390/8/4/591/htm
        /// https://www.mdpi.com/2072-4292/11/22/2692/pdf
        /// https://www.redalyc.org/pdf/3939/393946272009.pdf
        /// https://www.topo.auth.gr/greek/ORG_DOMI/EMERITUS/TOMOS_ASTERIADI/files/1-11%20Kotsakis.pdf       
        /// https://watermark.silverchair.com/168-1-1.pdf?token=AQECAHi208BE49Ooan9kkhW_Ercy7Dm3ZL_9Cf3qfKAc485ysgAAAqowggKmBgkqhkiG9w0BBwagggKXMIICkwIBADCCAowGCSqGSIb3DQEHATAeBglghkgBZQMEAS4wEQQMNtNYpH42iVWcUNOwAgEQgIICXR7f-qL-JGmrwURSCWZ18mdsbBO8gMbwiOfc1xtStFoIyr6HwZ51CTHotVtLst2AfW6EYaRmdaKcDZXNRL_xMRoU6CymkOTA2eiJEm5gyga8SLx3ixgLrcqyToxqAKi3wgXPRvBucTabNPm_pJU2bMjiWU0IPCeaSNUkLAHKsOKY6N0DhBiq_N8vwDUjioWe2tf-5DGygbSy0xjhmJ1yz_5Bsyt3CoDBN1ERhBgZCe1nXHxkxILgz5Wd40s4N1LJdSSnGpLMlvacOZq_ZkIH9Kbvj9-yzXJdjTcIKVQblSGYYapoMI10e68V4dhI5e3T6q61OYXt-wmeebqtjFCJVhl2d5h5zCPf2hp_lyGZRQxh9w5tGBnXfh-DnQ65uRBNpFtcB3AYpMoEh8piX7VbKeJGWdHSMZxO0VS-olGyTKXuymqEv0DIt6vNnsfdZQS7wgD3Sg3eonVlw8x1PGMyLjnGrL_L_1sWJ72dLz6CQvYaRIjkuXuf6Kfx2fmTAj1BCKXr1iPM2YsK6n9uFpAsdHHVOrZm66tn3h5MQuamWdR1SD4PVAeVpwwc_eEmuxX09pNroKI7kIg20dH536kF89XwrAbahNmaChTStYRpTjiUjOaRb6v-xMkeLSQS9dxU7ZbAIo0otpZv3Au9Fbs6wyl9Bi64OlrAeHxo8zK-RK2_CB4PcJyVtO-AwyRP8BiNtXKF3xUYGaQovUk9kmIXdwKG1mSOTJZvv0XYM51yG2jNqMyv2dm9Z1uo7lyw0UGRXii9zv_NMrGR5SqnH7XWykBnVbF7ahELWEqOqohw
        /// https://journal.geo.sav.sk/cgg/article/download/83/78/
        /// https://www.researchgate.net/publication/227127968_Least-squares_collocation_with_covariance-matching_constraints
        /// https://www.fig.net/resources/proceedings/fig_proceedings/athens/papers/ts07/ts07_2_mitsakaki.pdf
        /// https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6832662/
        /// https://www.researchgate.net/publication/227127968_Least-squares_collocation_with_covariance-matching_constraints
        /// https://www.lantmateriet.se/contentassets/ff12c6e07463427691d8bd432fc08ef6/steffen-etal-egu2019.pdf
        ///</Summary>
        public bool Helmert(double k, double c, double sn, bool runAsLs = false)
        {
            try
            {
                var iterations = 0;

                do
                {
                    if (runAsLs) // Least Squares Method:
                        X = (A.Transpose() * A).Inverse() * A.Transpose() * L;
                    else // Least Squares Collocation:
                         // Without noise parameter, Sn:
                         // X = (A.Transpose() * CovNn(k, c).Inverse() * A).Inverse() * (A.Transpose() * CovNn(k, c).Inverse() * L);
                         // With noise parameter, Sn:
                        X = (A.Transpose() * CovNn_D_Inv(k, c, sn) * A).Inverse() * (A.Transpose() * CovNn_D_Inv(k, c, sn) * L);                                    

                    Rx += X[0, 0] / _factor;
                    Ry += X[1, 0] / _factor;
                    Rz += X[2, 0] / _factor;
                    S += X[3, 0] / _factor;
                    Tx += X[4, 0];
                    Ty += X[5, 0];
                    Tz += X[6, 0];

                    iterations++;
                } while (!X.ForAll(x => Math.Abs(x) < 1E-8) && iterations < 10);

                SignalNoise = (L - A * X);

                return true;
            } 
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }

        public void PrintResiduals()
        {
            if (!HelmertIsComputed)
                return;

            var fileName = "Helmert_results_" + DateTime.Now.ToShortDateString()  + ".txt";

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine("Helmert parameters");
                writer.WriteLine("------------------------");
                writer.WriteLine($"Tx: {Tx,20:F7} meter");
                writer.WriteLine($"Ty: {Ty,20:F7} meter");
                writer.WriteLine($"Tz: {Tz,20:F7} meter");
                writer.WriteLine($"Rx: {Rx,20:F15} rad {Rx.ToArcSec(),15:F9} arcsec");
                writer.WriteLine($"Ry: {Ry,20:F15} rad {Ry.ToArcSec(),15:F9} arcsec");
                writer.WriteLine($"Rz: {Rz,20:F15} rad {Rz.ToArcSec(),15:F9} arcsec");
                writer.WriteLine($"S.: {S,20:F15} {S.ToPpm(),20:F9} ppm");
                writer.WriteLine($"RMS: {Rms,19:F6} meter");

                writer.WriteLine();
                writer.WriteLine("------------------------------------");
                writer.WriteLine("               X(mm)   Y(mm)   Z(mm)");
                writer.WriteLine("------------------------------------");
                writer.WriteLine($"St.dev:     {AverageX * 1000,8:F2}{AverageY * 1000,8:F2}{AverageZ * 1000,8:F2}");
                writer.WriteLine($"Max.:       {MaxX * 1000,8:F2}{MaxY * 1000,8:F2}{MaxZ * 1000,8:F2}");
                writer.WriteLine($"Min.:       {MinX * 1000,8:F2}{MinY * 1000,8:F2}{MinZ * 1000,8:F2}");
                writer.WriteLine($"Mean:       {MeanX * 1000,8:F2}{MeanY * 1000,8:F2}{MeanZ * 1000,8:F2}");
                writer.WriteLine($"RMS:        {RmsX * 1000,8:F2}{RmsY * 1000,8:F2}{RmsZ * 1000,8:F2}");

                writer.WriteLine();
                writer.WriteLine("       From points                                  To points                                    Residuals");
                writer.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
                writer.WriteLine("Name              X              Y              Z              X              Y              Z        vX        vY        vZ");
                writer.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
                
                foreach (var point in ValidPointList.Where(x => !x.HasNullValues))
                {
                    var index = ValidPointList.IndexOf(point);

                    double vx = V[index * 3 + 0, 0];
                    double vy = V[index * 3 + 1, 0];
                    double vz = V[index * 3 + 2, 0];

                    writer.WriteLine($"{point.Name}{point.X1,15:F4}{point.Y1,15:F4}{point.Z1,15:F4}{point.X2,15:F4}{point.Y2,15:F4}{point.Z2,15:F4}{vx,10:F4}{vy,10:F4}{vz,10:F4}");
                }
                writer.Close();
            }
        }

        internal void EstimatedCovariance(double k, double c, double sn)
        {
            var o = new List<(double, double, double)>();
            var step = 5000d;
            var min = 0d;
            var max = 80000d;

            System.Diagnostics.Debug.WriteLine($"k = {k}, c = {c}, sn = {sn}");
            System.Diagnostics.Debug.WriteLine($"Dist, v, ec, n");

            for (double r1 = min; r1 < max; r1 += step)
            {
                var sum = 0d;
                var n = 0;

                foreach (var p1 in ValidPointList)
                {
                    var index1 = ValidPointList.IndexOf(p1);

                    foreach (var p2 in ValidPointList)
                    {
                        var index2 = ValidPointList.IndexOf(p2);
                        var d = p1.GetDistance(p2);

                        if (d >= r1 + step || d <= r1)
                            continue;

                        var x1 = SignalNoise.At(index1 + 0, 0);
                        var y1 = SignalNoise.At(index1 + 1, 0);
                        var z1 = SignalNoise.At(index1 + 2, 0);

                        var dev1 = Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2) + Math.Pow(z1, 2));

                        var x2 = SignalNoise.At(index2 + 0, 0);
                        var y2 = SignalNoise.At(index2 + 1, 0);
                        var z2 = SignalNoise.At(index2 + 2, 0);
                       
                        var pos = PredictedPosition(k, c, sn, p2.X1, p2.Y1, p2.Z1);
                        var dx = (p2.X1 + pos.Item1) - p2.X2;
                        var dy = (p2.Y1 + pos.Item2) - p2.Y2;
                        var dz = (p2.Z1 + pos.Item3) - p2.Z2;

                        var dev2 = Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2) + Math.Pow(z2, 2));

                        var sumDev = false;
                        if (sumDev)
                        {
                            sum += dev1 * dev2;
                            n++;
                        }
                        else
                        {
                            sum += x1 * x2;
                            n++;

                            sum += y1 * y2;
                            n++;

                            sum += z1 * z2;
                            n++;
                        }
                    }
                }

                if (n == 0)
                    continue;

                // var v = k * Math.Exp(-(Math.PI / 2) * (d / c));
                var v = k * Math.Exp(-(Math.PI / 2) * ((r1 + step / 2) / c));
                var ec = sum / n;
                System.Diagnostics.Debug.WriteLine($"{r1 + step / 2}, {v}, {ec}, {n}");

               // o.Add((r1 + step / 2, v, ec));
            }
        }
        
        internal Matrix<double> HelmertPosition(double x, double y, double z)
        {
            if (!HelmertIsComputed)
                return Matrix<double>.Build.Dense(3, 1);

            var r = Matrix<double>.Build.Dense(3, 3);
            r[0, 0] =  1d; r[0, 1] = -Rz; r[0, 2] =  Ry;
            r[1, 0] =  Rz; r[1, 1] =  1d; r[1, 2] = -Rx;
            r[2, 0] = -Ry; r[2, 1] =  Rx; r[2, 2] =  1d;

            var t = Matrix<double>.Build.Dense(3, 1);
            t[0, 0] = Tx;
            t[1, 0] = Ty;
            t[2, 0] = Tz;

            var pin = Matrix<double>.Build.Dense(3, 1);
            pin[0, 0] = x;
            pin[1, 0] = y;
            pin[2, 0] = z;

            // NOTE: Alternative transformation notation:           
            /* var Ai = Matrix<double>.Build.Dense(3, 7);
            Ai[0, 0] = 0d; Ai[0, 1] =  z; Ai[0, 2] = -y; Ai[0, 3] = x; Ai[0, 4] = 1d; Ai[0, 5] = 0d; Ai[0, 6] = 0d;
            Ai[1, 0] = -z; Ai[1, 1] = 0d; Ai[1, 2] =  x; Ai[1, 3] = y; Ai[1, 4] = 0d; Ai[1, 5] = 1d; Ai[1, 6] = 0d;
            Ai[2, 0] =  y; Ai[2, 1] = -x; Ai[2, 2] = 0d; Ai[2, 3] = z; Ai[2, 4] = 0d; Ai[2, 5] = 0d; Ai[2, 6] = 1d;
 
            var xIn  = Matrix<double>.Build.Dense(7, 1);
            xIn[0, 0] = Rx; xIn[1, 0] = Ry; xIn[2, 0] = Rz; xIn[3, 0] = S; xIn[4, 0] = Tx; xIn[5, 0] = Ty; xIn[6, 0] = Tz;

            var xOut = Ai * xIn; 
            return xOut; */

            return t + S * r * pin;
        }
        
        // TODO: Refactorize architectur
        public override bool PopulatedGrid(double k, double c, double sn)
        {
            C0 = k;
            Cl = c;
            Sn = sn;

            _griX.Data.Clear();
            _griY.Data.Clear();
            _griZ.Data.Clear();

            // TODO: Move to virtual method
            if (!Helmert(k, c, sn))
                return false;

            if (HelmertIsComputed)
                PrintResiduals();
            
            var count = 0;
          
            for (var i = NRows - 1; i >= 0; i--)
            {
                var lat = LowerLeftLatitude + DeltaLatitude * i;
                for (var j = 0; j < NColumns; j++)
                {
                    var lon = LowerLeftLongitude + DeltaLongitude * j;

                    var p = new PointXYZ();

                    p.PhiDeg = lat; p.LambdaDeg = lon; p.H = Agl;

                    var pos = PredictedPosition(k, c, sn, p.X, p.Y, p.Z);
                    
                    if (pos.Item1 == -88.8888 && pos.Item2 == -88.8888 && pos.Item3 == -88.8888)
                    {
                        _griX.Data.Add(float.NaN);
                        _griY.Data.Add(float.NaN);
                        _griZ.Data.Add(float.NaN);
                    }
                    else
                    {
                        _griX.Data.Add((float)pos.Item1);
                        _griY.Data.Add((float)pos.Item2);
                        _griZ.Data.Add((float)pos.Item3);
                    }
                    count++;
                }
                Console.Clear();
                Console.Write($"Processing grid...  { (int)(100 * count / (NRows * NColumns))} %");             
            }
            
            // TestAutoCorr(k, c, sn);
            return true;
        }

        public Tuple<double, double, double> PredictedPosition(double k, double c, double sn, double x, double y, double z)
        {
            var helmert = HelmertPosition(x, y, z);

            var signalObs = CovMn(k, c, x, y, z).Transpose() * CovNn_D_InvSn(k, c, sn);

            if (signalObs.ForAll(cc => cc == 0))
                return new Tuple<double, double, double>(-88.8888, -88.8888, -88.8888);

            var xPredicted = helmert[0, 0] + signalObs[0, 0];
            var yPredicted = helmert[1, 0] + signalObs[1, 0];
            var zPredicted = helmert[2, 0] + signalObs[2, 0];

            var tuple = new Tuple<double, double, double>(xPredicted - x, yPredicted - y, zPredicted - z);

            return tuple;
        }

        public void CleanNullPoints()
        {
            PointList.RemoveAll(x => x.HasNullValues);
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
                    if (!double.TryParse(values[1], out double x) ||
                        !double.TryParse(values[2], out double y) ||
                        !double.TryParse(values[3], out double z) ||
                        !double.TryParse(values[4], out double epoch))                    
                        continue;
                    else
                    {
                        CommonPointXYZ cpPoint;

                        if (PointList.Any(p => p.Name == name))
                        {
                            cpPoint = PointList.Find(p => p.Name == name);
                            cpPoint.X1 = x;
                            cpPoint.Y1 = y;
                            cpPoint.Z1 = z;
                            cpPoint.Time = epoch;
                        }
                        else
                        {
                            cpPoint = new CommonPointXYZ
                            {
                                Name = name,
                                X1 = x,
                                Y1 = y,
                                Z1 = z,
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
                    if (!double.TryParse(values[1], out double x) ||
                        !double.TryParse(values[2], out double y) ||
                        !double.TryParse(values[3], out double z) ||
                        !double.TryParse(values[4], out double epoch))
                        continue;
                    else
                    {
                        CommonPointXYZ cpPoint;

                        if (PointList.Any(p => p.Name == name))
                        {
                            cpPoint = PointList.Find(p => p.Name == name);
                            cpPoint.X2 = x;
                            cpPoint.Y2 = y;
                            cpPoint.Z2 = z;
                            cpPoint.Time = epoch;
                        }
                        else
                        {
                            cpPoint = new CommonPointXYZ
                            {
                                Name = name,
                                X2 = x,
                                Y2 = y,
                                Z2 = z,
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
    }
}
