using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class Ct2Gtx2GeoTiffCommand : Command
    {
        public Ct2Gtx2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("output", "Output GeoTiff file") { ArgumentType = typeof(FileInfo) });

            AddOption(new Option("--ct2", "Input ct2 file") { Argument = new Argument<FileInfo>("ct2") });
            AddOption(new Option("--gtx", "Input gtx file") { Argument = new Argument<FileInfo>("gtx") });
            AddOption(new Option<GeoTiffFile.TiffOutputTypeshort>("--type", "TiffOutputType") { Argument = new Argument<GeoTiffFile.TiffOutputTypeshort>("type"), IsRequired = true });
            AddOption(new Option("--gridname", "Grid name") { Argument = new Argument<string>("gridname"), IsRequired = true });
            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option("--area", "Area of use") { Argument = new Argument<string>("area") });
            AddOption(new Option("--email", "Product manager") { Argument = new Argument<string>("email") });
            AddOption(new Option("--dim", "Dimension") { Argument = new Argument<int>("dim") });

            // TODO: Are the EPSG codes correct? Make unit or factory tests.           
            AddOption(new Option("--epsg2d", "Source EPSG interpolation CRS ('autority:XXXX')") { Argument = new Argument<string>("epsg2d"), IsRequired = true });
            AddOption(new Option("--epsgsource", "Source EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgsource"), IsRequired = true });
            AddOption(new Option("--epsgtarget", "Target EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgtarget"), IsRequired = true });
            AddOption(new Option("--tilesize", "Tile size (multiple of 16)") { Argument = new Argument<int>("tilesize"), IsRequired = true });

            Handler = CommandHandler.Create((Ct2Gtx2GeoTiffCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(Ct2Gtx2GeoTiffCommandParams par)
        {
            try
            {
                var tiff = new GeoTiffFile();

                tiff.OutputFileName = par.Output.FullName;
                tiff.Grid_name = par.GridName;
                tiff.ImageDescription = par.Desc ?? "";
                tiff.Area_of_use = par.Area ?? "";
                tiff.Email = par.Email ?? "";
                tiff.TileSize = par.TileSize;
                tiff.Dimensions = (par.Dim == 0) ?
                    ((par.Ct2 != null ? 2 : 0) + (par.Gtx != null ? 1 : 0)) :
                    par.Dim;
                tiff.Epsg2d.CodeString = par.Epsg2d;
                tiff.EpsgSource.CodeString = par.EpsgSource ?? "";
                tiff.EpsgTarget.CodeString = par.EpsgTarget ?? "";
                tiff.TiffOutput = (GeoTiffFile.TiffOutputType)par.Type;

                if (par.Ct2 != null && !tiff.Ct2.ReadCt2(par.Ct2.FullName, true))
                {
                    Console.WriteLine($"Cound not read the ct2 file {par.Ct2.Name}.");
                    return -1;
                }
                if (par.Gtx != null && !tiff.Gtx.ReadGtx(par.Gtx.FullName))
                {
                    Console.WriteLine($"Cound not read the gtx file {par.Gtx.Name}.");
                    return -1;
                }
                if (!tiff.GenerateGridFile(par.Output.FullName))
                {
                    Console.WriteLine($"Feil i generering av tiff-fil {par.Output.Name}.");
                    return -1;
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
