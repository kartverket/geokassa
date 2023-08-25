using System;

namespace gridfiles
{    
    public class GridParam
    {
        private double _lowerLeftLatitude = 0d;
        private double _lowerLeftLongitude = 0d;
        private double _deltaLatitude = 0d;
        private double _deltaLongitude = 0d;

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

        public virtual Int32 NRows { get; set; } = 0;
        public virtual Int32 NColumns { get; set; } = 0;
    }
}
