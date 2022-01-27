using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class CommonPointLLH
    {
        private PointLLH _fromPoint;
        private PointLLH _toPoint;    

        public CommonPointLLH()
        {
            _fromPoint = new PointLLH();
            _toPoint = new PointLLH();
        }

        public string Name { get; set; }

        public decimal Lat1
        {
            get => _fromPoint.Lat;
            set => _fromPoint.Lat = value;
        }

        public decimal Lon1
        {
            get => _fromPoint.Lon;
            set => _fromPoint.Lon = value;
        }

        public double Height1
        {
            get => _fromPoint.Height;
            set => _fromPoint.Height = value;
        }

        public decimal Lat2
        {
            get => _toPoint.Lat;
            set => _toPoint.Lat = value;
        }

        public decimal Lon2
        {
            get => _toPoint.Lon;
            set => _toPoint.Lon = value;
        }

        public double Height2
        {
            get => _toPoint.Height;
            set => _toPoint.Height = value;
        }        
    }

    public class CommonPointXYZ
    {
        private PointXYZ _fromPoint;
        private PointXYZ _toPoint;

        private const double Ro = Math.PI / 180;

        public CommonPointXYZ()
        {
            _fromPoint = new PointXYZ();
            _toPoint = new PointXYZ();
        }

        public string Name { get; set; }

        public double X1
        {
            get => _fromPoint.X;
            set => _fromPoint.X = value;
        }

        public double Y1
        {
            get => _fromPoint.Y;
            set => _fromPoint.Y = value;
        }

        public double Z1
        {
            get => _fromPoint.Z;
            set => _fromPoint.Z = value;
        }

        public double Phi1
        {
            get => _fromPoint.Phi;
            set => _fromPoint.Phi = value;
        }

        public double Lambda1
        {
            get => _fromPoint.Lambda;
            set => _fromPoint.Lambda = value;
        }           

        public double H1
        {
            get => _fromPoint.H;
            set => _fromPoint.H = value;
        }

        public double Phi1Deg
        {
            get => _fromPoint.PhiDeg;
            set => _fromPoint.PhiDeg = value;
        }

        public double Lambda1Deg
        {
            get => _fromPoint.LambdaDeg;
            set => _fromPoint.LambdaDeg = value;
        }

        public double Noise1
        {
            get => _fromPoint.Noise;
            set => _fromPoint.Noise = value;
        }

        public double Time { get; set; }

        public double X2
        {
            get => _toPoint.X;
            set => _toPoint.X = value;
        }

        public double Y2
        {
            get => _toPoint.Y;
            set => _toPoint.Y = value;
        }

        public double Z2
        {
            get => _toPoint.Z;
            set => _toPoint.Z = value;
        }

        public double Phi2
        {
            get => _toPoint.Phi;
            set => _toPoint.Phi = value;
        }

        public double Lambda2
        {
            get => _toPoint.Lambda;
            set => _toPoint.Lambda = value;
        }

        public double H2
        {
            get => _toPoint.H;
            set => _toPoint.H = value;
        }

        public double Phi2Deg
        {
            get => Phi2 * 180d / Math.PI;
            set => Phi2 = value * Math.PI / 180d;
        }        

        public double Lambda2Deg
        {
            get => Lambda2 * 180d / Math.PI;
            set => Lambda2 = value * Math.PI / 180d;
        }

        public double Noise2
        {
            get => _toPoint.Noise;
            set => _toPoint.Noise = value;
        }

        public double Distance
        {
            get
            {
                if (HasNullValues)
                    return 0d;

                return Math.Sqrt(Math.Pow(X1 - X2, 2) + Math.Pow(Y1 - Y2, 2) + Math.Pow(Z1 - Z2, 2));
            }
        }
       
        public bool HasNullValues => X1 == 0d || Y1 == 0d || Z1 == 0d || X2 == 0d || Y2 == 0d || Z2 == 0d; 

        public double GetDistance(double x, double y, double z)
        {
            if (HasNullValues)
                return 0d;

            return Math.Sqrt(Math.Pow(X1 - x, 2) + Math.Pow(Y1 - y, 2) + Math.Pow(Z1 - z, 2));
        }

        public double GetDistance(CommonPointXYZ point)
        {
            if (HasNullValues)
                return 0d;

            if (point.HasNullValues)
                return 0d;

            return Math.Sqrt(Math.Pow(this.X1 - point.X1, 2) +
                Math.Pow(this.Y1 - point.Y1, 2) + 
                Math.Pow(this.Z1 - point.Z1, 2));
        }

        public double GetDistance(double lat, double lon) => Math.Sqrt(Math.Pow((Lambda1Deg - lon) * CosLat(lat), 2) + Math.Pow(Phi1Deg - lat, 2));

        internal double CosLat(double lat) => Math.Cos(lat * Ro);
    }

    public class PointXYZ
    {  
        // Links: 
        // https://www.oc.nps.edu/oc2902w/coord/coordcvt.pdf
        // https://ciencias.ulisboa.pt/sites/default/files/fcul/dep/dqb/doc/GRS80_Moritz.pdf
        // https://ntnuopen.ntnu.no/ntnu-xmlui/bitstream/handle/11250/240413/742883_FULLTEXT01.pdf?sequence=1&isAllowed=y

        private const double a = 6378137d;
        private const double f = 1 / 298.257222101d; //298.257222100882711243

        private double _phi = 0d, _lambda = 0d, _h = 0d;     
        private double _x = 0d, _y = 0d, _z = 0d;
        private double _phiOld = 0d, _lambdaOld = 0d, _hOld = 0d;
        private double _xOld = 0d, _yOld = 0d, _zOld = 0d;

        public PointXYZ()
        { }

        public string Name { get; set; }

        public double X
        {
            get
            {
                if (_phi == _phiOld && _h == _hOld)
                    return _x;

                var e2 = f * (2 - f);
                var v = a / Math.Sqrt(1d - e2 * Math.Pow(Math.Sin(_phi), 2));
                _x = (v + _h) * Math.Cos(_phi) * Math.Cos(_lambda);

                return _x;
            }
            set
            {
                var isChanged = _x != value;
                _x = value;

                if (isChanged)
                {
                    Phi = Phi;
                    H = H;
                }
                _xOld = _x;
            }
        }

        public double Y
        {
            get
            {
                if (_lambda == _lambdaOld && _h == _hOld)
                    return _y;

                var e2 = f * (2 - f);
                var v = a / Math.Sqrt(1d - e2 * Math.Pow(Math.Sin(_phi), 2));
                _y = (v + _h) * Math.Cos(_phi) * Math.Sin(_lambda);

                return _y;
            }
            set
            {
                var isChanged = _y != value;
                _y = value;

                if (isChanged)
                {
                    Lambda = Lambda;
                    H = H;
                }
                _yOld = _y;
            }
        }

        public double Z
        {
            get
            {
                if (_phi == _phiOld && _h == _hOld)
                    return _z;

                var e2 = f * (2 - f);
                var v = a / Math.Sqrt(1d - e2 * Math.Pow(Math.Sin(_phi), 2));
                _z = ((1d - e2) * v + _h) * Math.Sin(_phi);

                return _z;
            }
            set
            {
                var isChanged = _z != value;
                _z = value;

                if (isChanged)
                {
                    Phi = Phi;
                    H = H;
                }
                _zOld = _z;
            }
        }

        public double Phi
        {
            get
            {
                if (_x == 0d && _y == 0d && _z == 0d)
                    return _phi;

                if (_x == _xOld && _y == _yOld && _z == _zOld)
                    return _phi;

                var e2 = f * (2 - f);
                var p = Math.Sqrt(_x * _x + _y * _y);
                var v = a / Math.Sqrt(1d - e2 * Math.Pow(Math.Sin(_phi), 2));
                
                var phi = _phi == 0 ? Math.Atan2(_z, p * (1d - e2)) : Math.Atan2(_z + e2 * v * Math.Sin(_phi), p);

                while (Math.Abs(_phi - phi) > 1E-15)
                {
                    phi = _phi;
                    v = a / Math.Sqrt(1d - e2 * Math.Pow(Math.Sin(_phi), 2));
                    _phi = Math.Atan2(_z + e2 * v * Math.Sin(_phi), p);
                }
                return _phi;
            }
            set
            {
                var isChanged = _phi != value;
                _phi = value;

                if (isChanged)
                {
                    X = X;
                    Y = Y;
                    Z = Z;
                }
                _phiOld = _phi;
            }
        }

        public double Lambda
        {
            get
            {
                if (_x == 0d && _y == 0d)
                    return _lambda;

                if (_x == _xOld && _y == _yOld)
                    return _lambda;

                _lambda = Math.Atan2(_y, _x);
                return _lambda;
            }
            set
            {
                var isChanged = _lambda != value;
                _lambda = value;

                if (isChanged)
                {
                    X = X;
                    Y = Y;
                }
                _lambdaOld = _lambda;
            } 
        }

        public double H
        {
            get
            {
                if (_x == 0d && _y == 0d)
                    return _h;

                if (_x == _xOld && _y == _yOld && _z == _zOld)
                    return _h;

                var e2 = f * (2 - f);
                var p = Math.Sqrt(_x * _x + _y * _y);
                var v =  a / Math.Sqrt(1d - e2 * Math.Pow(Math.Sin(_phi), 2));
                var h = (p / Math.Cos(_phi)) - v;

                while (Math.Abs(_h - h) > 1E-07)
                {
                    h = _h;
                    v = a / Math.Sqrt(1d - e2 * Math.Pow(Math.Sin(_phi), 2)); 
                    _h = (p / Math.Cos(_phi)) - v;
                }
                return _h;
            }
            set
            {
                var isChanged = _h != value;
                _h = value;

                if (isChanged)
                {
                    X = X;
                    Y = Y;
                    Z = Z;
                }
                _hOld = _h;
            }
        }
        
        public double PhiDeg
        {
            get => Phi * 180d / Math.PI;
            set => Phi = value * Math.PI / 180d;
        }

        public double LambdaDeg
        {
            get => Lambda * 180d / Math.PI;
            set => Lambda = value * Math.PI / 180d;
        }

        public double Noise { get; set; } = 0d;
    }

    public class PointXYZT
    {
        public PointXYZT()
        { }
   
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Time { get; set; }
    }

    public class PointNEH
    {
        public string Name { get; set; }
        public decimal North { get; set; }
        public decimal East { get; set; }
        public double Height { get; set; }
        public float Time { get; set; }
    }

    public class PointLLH
    {
        public string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lon { get; set; }
        public double Height { get; set; }
        public float Time { get; set; }
    }
}
