using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class PointCloudFile
    {
        private string _fileName = "";
        private string _sep = " ";

        public PointCloudFile(string fileName)
        {
            _fileName = fileName;
        }

        public decimal LowerLeftNorth { get; set; } = 0m;      
        public decimal LowerLeftEast { get; set; } = 0m;        
        public decimal DeltaNorth { get; set; } = 0m;
        public decimal DeltaEast { get; set; } = 0m;
        public Int32 NRows { get; set; } = 0;
        public Int32 NColumns { get; set; } = 0;

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
                var k = 0;

                for (var j = 0; j < NRows; j++)
                {
                    for (var i = 0; i < NColumns; i++)
                    {
                        var p = new PointNEH
                        {
                            Name = (++k).ToString(),
                            East = LowerLeftEast + i * DeltaEast,
                            North = LowerLeftNorth + j * DeltaNorth
                        };
                        data.Add(p);
                    }
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
                    outputFile.WriteLine($"{p.East}{Sep}{p.North}{Sep}{p.Height}{Sep}{p.Time}");
                // outputFile.WriteLine($"{p.Name}{Sep}{p.North}{Sep}{p.East}{Sep}{p.Height}{Sep}{p.Time}");

                outputFile.Close();
            }
        }
    }  
}
