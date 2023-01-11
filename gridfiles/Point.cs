using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;

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
        private PointXYZ _sourcePoint;
        private PointXYZ _targetPoint;

        private const double Ro = Math.PI / 180;

        public CommonPointXYZ()
        {
            _sourcePoint = new PointXYZ();
            _targetPoint = new PointXYZ();
        }
                
        [Description("Point name")]
        [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; } = "";
        
        [Description("Source geocentric X coordinate (m).")]
        [JsonProperty("x_source", NullValueHandling = NullValueHandling.Include)]
        public double X_Source
        {
            get => _sourcePoint.X;
            set => _sourcePoint.X = value;
        }

        [Description("Source geocentric Y coordinate (m).")]
        [JsonProperty("y_source", NullValueHandling = NullValueHandling.Include)]
        public double Y_Source
        {
            get => _sourcePoint.Y;
            set => _sourcePoint.Y = value;
        }

        [Description("Source geocentric Z coordinate (m).")]
        [JsonProperty("z_source", NullValueHandling = NullValueHandling.Include)]
        public double Z_Source
        {
            get => _sourcePoint.Z;
            set => _sourcePoint.Z = value;
        }
                
        [Description("Source latitude (rad).")]
        [JsonProperty("phi_source", Required = Required.Default, NullValueHandling = NullValueHandling.Include)]
        //[JsonProperty("phi_source", NullValueHandling = NullValueHandling.Include)]
        public double Phi_Source
        {
            get => _sourcePoint.Phi;
            set => _sourcePoint.Phi = value;
        }

        [Description("Source longitude (rad).")]
        [JsonProperty(Required = Required.Default)]
        public double Lambda_Source
        {
            get => _sourcePoint.Lambda;
            set => _sourcePoint.Lambda = value;
        }

        [Description("Source height coordinate (m).")]
        [JsonProperty(Required = Required.Default)]
        public double H_Source
        {
            get => _sourcePoint.H;
            set => _sourcePoint.H = value;
        }

        [Description("Source latitude (deg).")]
        [JsonProperty(Required = Required.Default)]
        public double Phi_SourceDeg
        {
            get => _sourcePoint.PhiDeg;
            set => _sourcePoint.PhiDeg = value;
        }

        [Description("Source longitude (deg).")]
        [JsonProperty(Required = Required.Default)]
        public double Lambda_SourceDeg
        {
            get => _sourcePoint.LambdaDeg;
            set => _sourcePoint.LambdaDeg = value;
        }

        [Description("Source noise (m).")]
        [JsonProperty(Required = Required.Default)]
        public double Noise_Source
        {
            get => _sourcePoint.Noise;
            set => _sourcePoint.Noise = value;
        }

        [Description("Epoch (year).")]
        public double Epoch { get; set; } = 0d;

        [Description("Target geocentric X coordinate (m).")]
        public double X_Target
        {
            get => _targetPoint.X;
            set => _targetPoint.X = value;
        }

        [Description("Target geocentric Y coordinate (m).")]
        public double Y_Target
        {
            get => _targetPoint.Y;
            set => _targetPoint.Y = value;
        }

        [Description("Target geocentric Z coordinate (m).")]
        public double Z_Target
        {
            get => _targetPoint.Z;
            set => _targetPoint.Z = value;
        }
        
        [Description("Target latitude (rad).")]
        [JsonProperty(Required = Required.Default)]
        public double Phi_Target
        {
            get => _targetPoint.Phi;
            set => _targetPoint.Phi = value;
        }

        [Description("Target longitude (rad).")]
        [JsonProperty(Required = Required.Default)]
        public double Lambda_Target
        {
            get => _targetPoint.Lambda;
            set => _targetPoint.Lambda = value;
        }

        [Description("Target height coordinate (m).")]
        [JsonProperty(Required = Required.Default)]
        public double H_Target
        {
            get => _targetPoint.H;
            set => _targetPoint.H = value;
        }

        [Description("Target latitude (deg).")]
        [JsonProperty(Required = Required.Default)]
        public double Phi_TargetDeg
        {
            get => Phi_Target * 180d / Math.PI;
            set => Phi_Target = value * Math.PI / 180d;
        }

        [Description("Target longitude (deg).")]        
        [JsonProperty("lambda_targetdeg", Required = Required.Default, NullValueHandling = NullValueHandling.Include)]
        public double Lambda_TargetDeg
        {
            get => Lambda_Target * 180d / Math.PI;
            set => Lambda_Target = value * Math.PI / 180d;
        }

        [Description("Target noise (m).")]        
        [JsonProperty("noise_target", Required = Required.Default, NullValueHandling = NullValueHandling.Include)]
        public double Noise_Target
        {
            get => _targetPoint.Noise;
            set => _targetPoint.Noise = value;
        }

        [JsonIgnore]
        public double Distance
        {
            get
            {
                if (HasNullValues)
                    return 0d;

                return Math.Sqrt(Math.Pow(X_Source - X_Target, 2) + Math.Pow(Y_Source - Y_Target, 2) + Math.Pow(Z_Source - Z_Target, 2));
            }
        }

        [JsonIgnore]
        public bool HasNullValues => X_Source == 0d || Y_Source == 0d || Z_Source == 0d || X_Target == 0d || Y_Target == 0d || Z_Target == 0d; 

        public double GetDistance(double x, double y, double z)
        {
            if (HasNullValues)
                return 0d;

            return Math.Sqrt(Math.Pow(X_Source - x, 2) + Math.Pow(Y_Source - y, 2) + Math.Pow(Z_Source - z, 2));
        }
        
        public double GetDistance(CommonPointXYZ point)
        {
            if (HasNullValues)
                return 0d;

            if (point.HasNullValues)
                return 0d;

            return Math.Sqrt(
                Math.Pow(this.X_Source - point.X_Source, 2) +
                Math.Pow(this.Y_Source - point.Y_Source, 2) + 
                Math.Pow(this.Z_Source - point.Z_Source, 2));
        }

        public double GetDistance(double lat, double lon) => Math.Sqrt(Math.Pow((Lambda_SourceDeg - lon) * CosLat(lat), 2) + Math.Pow(Phi_SourceDeg - lat, 2));

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
