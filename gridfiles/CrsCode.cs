using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public EllipsoidParameters? GetEllipsoidParameters()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "projinfo.exe",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            startInfo.ArgumentList.Add("-q");
            startInfo.ArgumentList.Add("-k");
            startInfo.ArgumentList.Add("crs");
            startInfo.ArgumentList.Add(CodeString);
            startInfo.ArgumentList.Add("-o");
            startInfo.ArgumentList.Add("PROJJSON");

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return null;

            var jsonStart = output.IndexOf('{');

            if (jsonStart < 0)
                return null;

            var json = output[jsonStart..];

            using var document = JsonDocument.Parse(json);

            return TryFindEllipsoid(document.RootElement, out var parameters)
                ? parameters
                : null;
        }

        private static bool TryFindEllipsoid(
            JsonElement element,
            out EllipsoidParameters? parameters)
        {
            parameters = null;

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("ellipsoid", out var ellipsoid))
                {
                    if (ellipsoid.TryGetProperty("semi_major_axis", out var semiMajorAxis) &&
                        ellipsoid.TryGetProperty("inverse_flattening", out var inverseFlattening) &&
                        semiMajorAxis.TryGetDouble(out var semiMajorAxisValue) &&
                        inverseFlattening.TryGetDouble(out var inverseFlatteningValue))
                    {
                        parameters = new EllipsoidParameters(
                            semiMajorAxisValue,
                            inverseFlatteningValue);

                        return true;
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (TryFindEllipsoid(property.Value, out parameters))
                        return true;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (TryFindEllipsoid(item, out parameters))
                        return true;
                }
            }

            return false;
        }

        public sealed record EllipsoidParameters(
            double SemiMajorAxis,
            double InverseFlattening);
    }
}
