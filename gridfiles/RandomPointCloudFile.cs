using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class RandomPointCloudFile
    {
        private string _fileName = "";
        private string _sep = " ";

        public RandomPointCloudFile(string fileName)
        {
            _fileName = fileName;
        }

        public decimal LowerLeftNorth { get; set; } = 0m;
        public decimal LowerLeftEast { get; set; } = 0m;
        public decimal UpperRightNorth { get; set; } = 0m;
        public decimal UpperRightEast { get; set; } = 0m;
        public Int32 NoOfNumber { get; set; } = 0;

        public string Sep
        {
            get => _sep;
            set
            {
                // TODO: Not finished
                /* char[] charArray = value.ToCharArray();
                if (charArray.Count() >= 3)
                {  
                    string charToString = new string(charArray, 1, charArray.Count() - 2);
                    _sep = charToString;
                    return;
                }*/
                _sep = value;
            }
        }


        public List<PointNEH> GridData
        {
            get
            {
                var data = new List<PointNEH>();
                var rand = new Random();
                var k = 0;
                
                var rangeNorth = UpperRightNorth - LowerLeftNorth;
                var rangeEast = UpperRightEast - LowerLeftEast;

                for (var i = 0; i < NoOfNumber; i++)
                {
                    var p = new PointNEH
                    {
                        Name = (++k).ToString(),
                        North =  (decimal)rand.NextDouble() * rangeNorth + LowerLeftNorth,
                        East = (decimal)rand.NextDouble() * rangeEast + LowerLeftEast
                    };
                    data.Add(p);
                }
                return data;
            }
        }

        public void SaveFile()
        {
            if (_fileName == "")
                return;

            using (StreamWriter outputFile = new StreamWriter(_fileName, false))
            {
                foreach (var p in GridData)
                    outputFile.WriteLine($"{p.Name}{Sep}{p.North:F4}{Sep}{p.East:F4}{Sep}{p.Height}{Sep}{p.Time}");

                outputFile.Close();
            }
        }
    }
}
