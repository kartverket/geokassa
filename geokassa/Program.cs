using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.Linq;
using System.Threading.Tasks;

namespace geokassa
{   
    class Program
    {
        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand()
            {
                new JsonTinCommand("jsontin", "Makes triangulated TIN from point clouds"),
                new Lsc2GeoTiffCommand("lsc2geotiff", "Converts GeoTiff translations based on Helmert + Least Squares Collocation"),
                new Bin2GeoTiffCommand("bin2geotiff", "Converts bin file to GeoTiff"),
                new Gri2GeoTiffCommand("gri2geotiff", "Converts gri file(s) to GeoTiff"),
                new Gtx2GeoTiffCommand("gtx2geotiff", "Converts gtx file to GeoTiff"),
                new Ct2Gtx2GeoTiffCommand("ct2gtx2geotiff", "Converts gtx or ct2 files to GeoTiff"),
                new Csvs2Ct2("csvs2ct2", "Converts horisontal shift between two csv's into Ct2")
            };

            var _ = new CommandLineBuilder(rootCommand)
                .UseVersionOption()
                .UseHelp()
                .UseEnvironmentVariableDirective()
                .UseParseDirective()
                .UseDebugDirective()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseTypoCorrections()
                .UseParseErrorReporting()
                .CancelOnProcessTermination()
                .Build();

            rootCommand.InvokeAsync(args).Wait();
        } 
    }  
}
