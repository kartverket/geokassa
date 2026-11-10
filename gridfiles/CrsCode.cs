using OSGeo.OSR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OSGeo.OSR;

namespace gridfiles
{
    public class CrsCode
    {
        private string _codeString = "";

        public CrsCode()
        {
        }

        public CrsCode(string codeString)
        {
            _codeString = codeString;
        }

        public string CodeString { get => _codeString; set => _codeString = value; }
       
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

        public void SetCodeString(string value) => CodeString = value;

        public string GetWktString()
        {
            var cmd = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = "projinfo.exe",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = " " + this.CodeString + " -o WKT2:2019"
            };

            cmd.StartInfo = startInfo;
            cmd.Start();

            cmd.WaitForExit();

            var result = cmd.StandardOutput.ReadToEnd();

            if (result.Length < 19)
                return result;

            var trimmedRestult = result.Remove(0, 19);

            return trimmedRestult;
        }    

        public readonly record struct EllipsoidParameters(
        double SemiMajorAxisMeters,
        double SemiMinorAxisMeters,
        double InverseFlattening);

        public static class GdalEllipsoid
        {
            public static EllipsoidParameters FromEpsg(int epsgCode)
            {
                if (epsgCode <= 0)
                    throw new ArgumentOutOfRangeException(nameof(epsgCode), "EPSG code must be positive.");

                gridfiles.GdalConfiguration.ConfigureGdal();

                if (!GdalConfiguration.Usable)
                    throw new InvalidOperationException("GDAL is not configured or usable.");

                using var spatialReference = new SpatialReference("");
                var result = spatialReference.ImportFromEPSG(epsgCode);

                //if (result != OGRErr.OGRERR_NONE)
                 //   throw new ArgumentException($"Could not load EPSG:{epsgCode}.", nameof(epsgCode));

                double semiMajorAxis = spatialReference.GetSemiMajor();
                double inverseFlattening = spatialReference.GetInvFlattening();
                double semiMinorAxis = inverseFlattening == 0
                    ? semiMajorAxis
                    : semiMajorAxis * (1 - 1 / inverseFlattening);

                return new EllipsoidParameters(semiMajorAxis, semiMinorAxis, inverseFlattening);
            }
        }      
    }
}
