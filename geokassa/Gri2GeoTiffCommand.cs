using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class Gri2GeoTiffCommand : Command
    {
        public Gri2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("output", "Output GeoTiff file"));

            AddOption(new Option<GeoTiffFile.TiffOutputTypeshort>("--type", "TiffOutputType") { Argument = new Argument<GeoTiffFile.TiffOutputTypeshort>("type"), IsRequired = true });
            AddOption(new Option("--inpute", "Gri file - easting") { Argument = new Argument<string>("inpute") });
            AddOption(new Option("--inputn", "Gri file - northing") { Argument = new Argument<string>("inputn") });
            AddOption(new Option("--inputu", "Gri file - height") { Argument = new Argument<string>("inputu") });
            AddOption(new Option("--gridname", "Grid name") { Argument = new Argument<string>("gridname"), IsRequired = true });
            AddOption(new Option("--area", "Area of use") { Argument = new Argument<string>("area") });
            AddOption(new Option("--email", "Product manager") { Argument = new Argument<string>("email") });
            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option("--epsg2d", "Source EPSG interpolation CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg2d"), IsRequired = true });
            AddOption(new Option("--epsg3d", "Source EPSG 3D CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg3d"), IsRequired = true });
            AddOption(new Option("--epsgtarget", "Target EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgtarget"), IsRequired = true });
            AddOption(new Option("--geoid", "Geoid- or separationmodel") { Argument = new Argument<bool>("geoid") });
            AddOption(new Option("--tilesize", "Tile size (multiple of 16)") { Argument = new Argument<int>("tilesize"), IsRequired = true });

            Handler = CommandHandler.Create((Gri2GeoTiffCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(Gri2GeoTiffCommandParams par)
        {
            try
            {
                var inputE = par.InputE;
                var inputN = par.InputN;
                var inputU = par.InputU;

                string inputEName = inputE != null ? inputE.FullName : "";
                string inputNName = inputN != null ? inputN.FullName : "";
                string inputUName = inputU != null ? inputU.FullName : "";

                var tiff = new GeoTiffFile(inputEName, inputNName, inputUName);

                tiff.Grid_name = par.GridName;
                tiff.ImageDescription = par.Desc ?? "";
                tiff.Area_of_use = par.Area ?? "";
                tiff.Email = par.Email ?? "";
                tiff.TileSize = par.TileSize;
                tiff.Epsg2d.CodeString = par.Epsg2d;
                tiff.Epsg3d.CodeString = par.Epsg3d;
                tiff.EpsgTarget.CodeString = par.EpsgTarget;
                tiff.Dimensions = par.Dim;
                tiff.TiffOutput = (GeoTiffFile.TiffOutputType)par.Type;

                if (!tiff.ReadGriFiles())
                {
                    Console.WriteLine($"Could not read gri files.");
                    return -1;
                }
                if (!tiff.GenerateGridFile(par.Output.FullName))
                {
                    Console.WriteLine($"Generation of tiff file {par.Output.Name} failed.");
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
