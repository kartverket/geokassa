using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class Bin2GeoTiffCommand : Command
    {
        public Bin2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input bin file"));
            AddArgument(new Argument<FileInfo>("output", "Output GeoTiff file"));

            AddOption(new Option<FileInfo>("--sdfile", "Input standard deviation bin file"));

            AddOption(new Option<GeoTiffFile.TiffOutputTypeshort>("--type", "TiffOutputType") { Argument = new Argument<GeoTiffFile.TiffOutputTypeshort>("type"), IsRequired = true });
            AddOption(new Option("--gridname", "Grid name") { Argument = new Argument<string>("gridname"), IsRequired = true });
            AddOption(new Option("--email", "Product manager") { Argument = new Argument<string>("email") });
            AddOption(new Option("--area", "Area of use") { Argument = new Argument<string>("area") });
            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option("--epsg2d", "Source EPSG interpolation CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg2d"), IsRequired = true });
            AddOption(new Option("--epsg3d", "Source EPSG 3D CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg3d"), IsRequired = true });
            AddOption(new Option("--epsgsource", "Source EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgsource"), IsRequired = false });
            AddOption(new Option("--epsgtarget", "Target EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgtarget"), IsRequired = true });
            AddOption(new Option("--geoid", "Geoid- or separationmodel") { Argument = new Argument<bool>("geoid") });
            AddOption(new Option("--tilesize", "Tile size (multiple of 16)") { Argument = new Argument<int>("tilesize"), IsRequired = true });

            Handler = CommandHandler.Create((Bin2GeoTiffCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(Bin2GeoTiffCommandParams par)
        {
            try
            {
                var tiff = new GeoTiffFile();

                tiff.Grid_name = par.GridName;
                tiff.ImageDescription = par.Desc;
                tiff.Area_of_use = par.Area;
                tiff.Email = par.Email;
                tiff.TileSize = par.TileSize;
                tiff.Epsg2d.CodeString = par.Epsg2d;
                tiff.Epsg3d.CodeString = par.Epsg3d;
                tiff.EpsgSource.CodeString = par.EpsgSource ?? "";
                tiff.EpsgTarget.CodeString = par.EpsgTarget ?? "";
                tiff.Dimensions = 1;
                tiff.TiffOutput = (GeoTiffFile.TiffOutputType)par.Type;

                if (!tiff.Gtx.ReadBin(par.Input.FullName))
                {
                    Console.WriteLine($"Importing of bin file {par.Input.Name} failed.");
                    return -1;
                }
                if (par.SdFile != null)
                {
                    if (!tiff.GtxSdFile.ReadBin(par.SdFile.FullName))
                    {
                        Console.WriteLine($"Importing of sd bin file {par.SdFile.Name} failed.");
                        return -1;
                    }
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
