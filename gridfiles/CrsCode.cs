using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gridfiles
{
    public class CrsCode
    {
        private string _codeString = "";

        public string CodeString { get => _codeString; set => _codeString = value; }

        public void SetCodeString(string value) => CodeString = value;

        public string AutorityName
        {
            get
            {
                if (CodeString.Contains(":"))
                    return CodeString.Split(':')[0];

                return CodeString;
            }
        }

        public string CodeName
        {
            get
            {
                if (CodeString.Contains(":"))
                    return CodeString.Split(':')[1];

                return CodeString;
            }           
        }

        public UInt16 CodeNumber
        {
            get
            {
                if (UInt16.TryParse(CodeName, out ushort n))
                    return n;

                return 0;
            }
        }

        public string GetWktString()
        {
            var cmd = new Process();
            var startInfo = new ProcessStartInfo();

            startInfo.FileName = "projinfo.exe";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = " " + this.CodeString + " -o WKT2:2019";
         
            cmd.StartInfo = startInfo;
            cmd.Start();

            cmd.WaitForExit();

            var result = cmd.StandardOutput.ReadToEnd();

            if (result.Length < 19)
                return result;

            var trimmedRestult = result.Remove(0, 19);

            return trimmedRestult;
        }
        
        /*
        public string GetWktBoundaryBox()
        {
            var cmd = new Process();
            var startInfo = new ProcessStartInfo();

            startInfo.FileName = "projinfo.exe";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "-k crs " + this.CodeString;

            cmd.StartInfo = startInfo;
            cmd.Start();

            cmd.WaitForExit();

            var result = cmd.StandardOutput.ReadToEnd();

            return "";
        }
        */
    }
}
