using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class ClipCommand : Command
    {
        public ClipCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("inputFile", "Path to input grid file") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("outputFile", "Path to output grid file") { ArgumentType = typeof(FileInfo) });

            AddOption(new Option<GridFile.GridType>("--type", "GridType") { Argument = new Argument<GridFile.GridType>("type"), IsRequired = true });
            AddOption(new Option("--bbox", "Clip boundary box (west_long,south_lat,east_long,north_lat)") { Argument = new Argument<string[]>("bbox"), IsRequired = true });
            AddOption(new Option("--tilesize", "Tile size (multiple of 16)") { Argument = new Argument<int>("tilesize"), IsRequired = true });

            Handler = CommandHandler.Create((ClipCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(ClipCommandParams pars)
        {
            var west_long = 0d;
            var south_lat = 0d;
            var east_long = 0d;
            var north_lat = 0d;

            try
            {
                if (pars.Bbox == null)
                    throw new ArgumentException("--bbox is not defined");

                if (pars.Bbox.Length != 4)
                    throw new ArgumentException("Incorrect size of --bbox");

                if (!double.TryParse(pars.Bbox[0], out west_long) || !double.TryParse(pars.Bbox[1], out south_lat) || !double.TryParse(pars.Bbox[2], out east_long) || !double.TryParse(pars.Bbox[3], out north_lat))
                    throw new FormatException("Invalid type in --bbox");

                switch (pars.Type)
                {
                    case GridFile.GridType.bin:
                        break;
                    case GridFile.GridType.ct2:
                        break;
                    case GridFile.GridType.gtx:
                        break;
                    default:
                        {
                            var tiff = new GeoTiffFile();

                            if (!tiff.ReadGeoTiff(pars.InputFile.FullName))
                            {
                                Console.WriteLine($"The file {pars.InputFile.FullName} does not exist!");
                                return -1;
                            }
                            tiff.TileSize = pars.TileSize;

                            if (!tiff.ClipGrid(west_long, south_lat, east_long, north_lat))
                                return -1;

                            if (!tiff.GenerateGridFile(pars.OutputFile.FullName))
                                return -1;
                        }
                        break;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}
