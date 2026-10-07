using System;

namespace gridfiles
{    
    public class GridParam
    {
        private double _lowerLeftLatitude = 0d;
        private double _lowerLeftLongitude = 0d;
        private double _deltaLatitude = 0d;
        private double _deltaLongitude = 0d;
        private double _xLower = 0d;
        private double _yLower = 0d;
        private double _zLower = 0d;
        private double _xRes = 0d;
        private double _yRes = 0d;
        private double _zRes = 0d;

        public GridParam()
        {
        }      

        public virtual double LowerLeftLatitude
        {
            get => _lowerLeftLatitude;
            set => _lowerLeftLatitude = value;
        }

        public virtual double LowerLeftLongitude
        {
            get => _lowerLeftLongitude;
            set => _lowerLeftLongitude = value;
        }

        public virtual double LowerRightLatitude => _lowerLeftLatitude;
        
        public virtual double LowerRightLongitude => _lowerLeftLongitude + (NColumns - 1) * _deltaLongitude;

        public virtual double UpperLeftLatitude => _lowerLeftLatitude + (NRows - 1) * _deltaLatitude;

        public virtual double UpperLeftLongitude => _lowerLeftLongitude;

        public virtual double UpperRightLatitude => _lowerLeftLatitude + (NRows - 1) * _deltaLatitude;

        public virtual double UpperRightLongitude => _lowerLeftLongitude + (NColumns - 1) * _deltaLongitude;

        public virtual double DeltaLatitude
        {
            get => _deltaLatitude;
            set => _deltaLatitude = value;
        }

        public virtual double DeltaLongitude
        {
            get => _deltaLongitude;
            set => _deltaLongitude = value;
        }

        public virtual double XRes
        {
            get => _xRes;
            set => _xRes = value;
        }

        public virtual double YRes
        {
            get => _yRes;
            set => _yRes = value;
        }

        public virtual double ZRes
        {
            get => _zRes;
            set => _zRes = value;
        }

        public virtual Int32 NRows { get; set; } = 0;
        public virtual Int32 NColumns { get; set; } = 0;
        public virtual Int32 XPixels { get; set; } = 0;
        public virtual Int32 YPixels { get; set; } = 0;
        public virtual Int32 ZPixels { get; set; } = 0;
        public virtual Int32 NumberOfPixels => NRows * NColumns;

        public virtual double XLower
        {
            get => _xLower;
            set => _xLower = value;
        }

        public virtual double YLower
        {
            get => _yLower;
            set => _yLower = value;
        }

        public virtual double ZLower
        {
            get => _zLower;
            set => _zLower = value;
        }      
    }
}
