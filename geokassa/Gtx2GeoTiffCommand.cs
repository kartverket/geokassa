using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class Gtx2GeoTiffCommand : Command
    {
        public Gtx2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input gtx file"));
            AddArgument(new Argument<FileInfo>("output", "Output GeoTiff file"));

            AddOption(new Option<GeoTiffFile.TiffOutputTypeshort>("--type", "TiffOutputType") { Argument = new Argument<GeoTiffFile.TiffOutputTypeshort>("type"), IsRequired = true });
            AddOption(new Option("--gridname", "Grid name") { Argument = new Argument<string>("gridname"), IsRequired = true });
            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option("--area", "Area of use") { Argument = new Argument<string>("area") });
            AddOption(new Option("--email", "Product manager") { Argument = new Argument<string>("email") });
            AddOption(new Option("--epsg2d", "Source EPSG interpolation CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg2d"), IsRequired = true });
            AddOption(new Option("--epsg3d", "Source EPSG 3D CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg3d"), IsRequired = true });
            AddOption(new Option("--epsgsource", "Source EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgsource"), IsRequired = false });
            AddOption(new Option("--epsgtarget", "Target EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgtarget"), IsRequired = false });
            AddOption(new Option("--tilesize", "Tile size (multiple of 16)") { Argument = new Argument<int>("tilesize"), IsRequired = true });

            Handler = CommandHandler.Create((Gtx2GeoTiffCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(Gtx2GeoTiffCommandParams parameters)
        {
            try
            {
                var tiff = new GeoTiffFile();

                tiff.Grid_name = parameters.GridName;
                tiff.ImageDescription = parameters.Desc ?? "";
                tiff.Area_of_use = parameters.Area ?? "";
                tiff.Email = parameters.Email ?? "";
                tiff.TileSize = parameters.TileSize;
                tiff.Dimensions = 1;
                tiff.Epsg2d.CodeString = parameters.Epsg2d;
                tiff.Epsg3d.CodeString = parameters.Epsg3d;
                tiff.EpsgSource.CodeString = parameters.EpsgSource;
                tiff.EpsgTarget.CodeString = parameters.EpsgTarget;
                tiff.TiffOutput = (GeoTiffFile.TiffOutputType)parameters.Type;

                if (!tiff.Gtx.ReadGtx(parameters.Input.FullName))
                {
                    Console.WriteLine($"Cound not read the gtx file {parameters.Input.Name}.");
                    return -1;
                }
                if (!tiff.GenerateGridFile(parameters.Output.FullName))
                {
                    Console.WriteLine($"Generation of tiff file {parameters.Output.Name} failed.");
                    return -1;
                }
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
