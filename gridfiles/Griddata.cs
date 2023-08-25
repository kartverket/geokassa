using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class VelocityGrid
    {
        private GridParam _gridParam;
        private List<VelocityPoint> _gridData = null;

        public VelocityGrid(GridParam gridParam)
        {
            _gridParam = gridParam;
        }

        public List<VelocityPoint> VelocityGridData
        {
            get => _gridData = _gridData ?? new List<VelocityPoint>();
            set => _gridData = value;
        }

        public List<VelocityPoint> SortedGridData
        {
            get
            {   
                VelocityGridData.Sort();

                return VelocityGridData;
            }
        }

        public List<float> EastVelocityData
        {
            get => SortedGridData.Select(cust => cust.EastVelocity).ToList();
        }

        public List<float> NorthVelocityData
        {
            get => SortedGridData.Select(cust => cust.NorthVelocity).ToList();
        }

        public List<float> UpVelocityData
        {
            get => SortedGridData.Select(cust => cust.UpVelocity).ToList();            
        }       
    }
}
