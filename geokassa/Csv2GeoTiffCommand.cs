using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class Csv2GeoTiffCommand : Command
    {
        public Csv2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input column separation file name"));
            AddArgument(new Argument<FileInfo>("output", "Output geotiff file"));

            AddOption(new Option<GeoTiffFile.TiffOutputTypeshort>("--type", "TiffOutputType") { Argument = new Argument<GeoTiffFile.TiffOutputTypeshort>("type"), IsRequired = true });
            AddOption(new Option("--gridname", "Grid name") { Argument = new Argument<string>("gridname"), IsRequired = true });
            AddOption(new Option("--version", "Grid version") { Argument = new Argument<string>("version") });
            AddOption(new Option("--email", "Product manager") { Argument = new Argument<string>("email") });
            AddOption(new Option("--area", "Area of use") { Argument = new Argument<string>("area") });
            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option("--epsg2d", "Source EPSG interpolation CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg2d"), IsRequired = true });
            AddOption(new Option("--epsg3d", "Source EPSG 3D CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg3d"), IsRequired = true });
            AddOption(new Option("--epsgsource", "Source EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgsource"), IsRequired = false });
            AddOption(new Option("--epsgtarget", "Target EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgtarget"), IsRequired = true });

            AddOption(new Option("--tilesize", "Tile size (multiple of 16)") { Argument = new Argument<int>("tilesize"), IsRequired = true });

            Handler = CommandHandler.Create((Csv2GeoTiffCommandParams par) =>
            {
                return HandleCommand(par);
            });
        }

        private int HandleCommand(Csv2GeoTiffCommandParams par)
        {
            try
            {
                var tiff = new GeoTiffFile();

                tiff.Area_of_use = par.Area ?? "";
                tiff.ImageDescription = par.Desc ?? "";
                tiff.Email = par.Email ?? "";
                tiff.Grid_name = par.GridName ?? "";
                tiff.TileSize = par.TileSize;
                tiff.Epsg2d.CodeString = par.Epsg2d;
                tiff.Epsg3d.CodeString = par.Epsg3d;
                tiff.EpsgSource.CodeString = par.EpsgSource ?? "";
                tiff.EpsgTarget.CodeString = par.EpsgTarget;
                tiff.Dimensions = 3;
                tiff.TiffOutput = (GeoTiffFile.TiffOutputType)par.Type;

                tiff.OutputFileName = par.Output.FullName;

                if (!tiff.ReadVelocityFile(par.Input.FullName))
                {
                    Console.WriteLine($"Could not read {par.Input.Name}.");
                    return -1;
                }
                tiff.GenerateGridFile(par.Output.FullName);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
    }
}
