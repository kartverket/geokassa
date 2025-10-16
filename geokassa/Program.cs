using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace geokassa
{
    class Program
    {
        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand()
            { 
                new Csv2GeoTiffCommand("csv2geotiff","Converts column separated files to GeoTiff"),
                new JsonTinCommand("jsontin", "Makes triangulated TIN from point clouds"),
                new Lsc2GeoTiffCommand("lsc2geotiff", "Converts GeoTiff translations based on Helmert + Least Squares Collocation"),
                new Bin2GeoTiffCommand("bin2geotiff", "Converts bin file to GeoTiff"),
                new Bin2GtxCommand("bin2gtx", "Converts bin file to Gtx"),
                new Gri2GeoTiffCommand("gri2geotiff", "Converts gri file(s) to GeoTiff"),
                new Gtx2GeoTiffCommand("gtx2geotiff", "Converts gtx file to GeoTiff"),
                new Ct2Gtx2GeoTiffCommand("ct2gtx2geotiff", "Converts gtx or ct2 files to GeoTiff"),
                new ClipCommand("clip", "Clips grid files"),
                new Csvs2Ct2Command("csvs2ct2", "Converts horisontal shift between two csv's into Ct2"),
                new MergeGrids("merge", "Merges two ct2 files"),
                new MakeGrid("makegrid", "Grids points into csv-file" ),
                new Helmert("helmert", "Computes helmert parametres based on two csv point sets"),
                new TiffValueCommand("tiffvalue","Gets interpolated values from a grid")
            };

            var _ = new CommandLineBuilder(rootCommand)
                .UseExceptionHandler()
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
