using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class TextLsc2GeoTiffCommand : Command
    {
        public TextLsc2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("inputtextfile", "Input from column separated inputfile"));
            AddArgument(new Argument<FileInfo>("output", "Output geotiff file"));

            AddOption(new Option<GeoTiffFile.TiffOutputTypeshort>("--type", "TiffOutputType") { Argument = new Argument<GeoTiffFile.TiffOutputTypeshort>("type") });
            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option("--gridname", "Grid name") { Argument = new Argument<string>("gridname"), IsRequired = true });
            AddOption(new Option("--email", "Product manager") { Argument = new Argument<string>("email") });
            AddOption(new Option("--tilesize", "Tile size (multiple of 16)") { Argument = new Argument<int>("tilesize"), IsRequired = true });
            AddOption(new Option("--dim", "Dimension") { Argument = new Argument<int>("dim") });
            AddOption(new Option("--xpixels", "Number of pixel in x") { Argument = new Argument<int>("xpixels"), IsRequired = true });
            AddOption(new Option("--ypixels", "Number of pixel in y") { Argument = new Argument<int>("ypixels"), IsRequired = true });
            AddOption(new Option("--zpixels", "Number of pixel in z") { Argument = new Argument<int>("zpixels"), IsRequired = true });
            AddOption(new Option("--xres", "Resolution in x") { Argument = new Argument<double>("xres"), IsRequired = true });
            AddOption(new Option("--yres", "Resolution in y") { Argument = new Argument<double>("yres"), IsRequired = true });
            AddOption(new Option("--zres", "Resolution in z") { Argument = new Argument<double>("zres"), IsRequired = true });
            AddOption(new Option("--xlower", "Lower x coordinate in m") { Argument = new Argument<double>("xlower"), IsRequired = true });
            AddOption(new Option("--ylower", "Lower y coordinate in m") { Argument = new Argument<double>("ylower"), IsRequired = true });
            AddOption(new Option("--zlower", "Lower z coordinate in m") { Argument = new Argument<double>("zlower"), IsRequired = true });
            AddOption(new Option("--c0", "Covariance signal - LSC (m2)") { Argument = new Argument<double>("c0"), IsRequired = true });
            AddOption(new Option("--cl", "Correlastion length - LSC (m)") { Argument = new Argument<double>("cl"), IsRequired = true });
            AddOption(new Option("--sn", "Covariance noise - LSC (m)") { Argument = new Argument<double>("sn"), IsRequired = true });

            Handler = CommandHandler.Create((TextLsc2GeoTiffCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(TextLsc2GeoTiffCommandParams par)
        {
            try
            {
                var tiff = new GeoTiffFile();

                tiff.ImageDescription = par.Desc ?? "";
                tiff.Grid_name = par.GridName ?? "";
                tiff.Email = par.Email ?? "";
                tiff.Dimensions = par.Dim == 0 ? 3 : par.Dim;
                tiff.TileSize = par.TileSize;
                tiff.TiffOutput = (GeoTiffFile.TiffOutputType)par.Type;
                tiff.XPixels = par.XPixels;
                tiff.YPixels = par.YPixels;
                tiff.ZPixels = par.ZPixels;
                tiff.XRes = par.XRes;
                tiff.YRes = par.YRes;
                tiff.ZRes = par.ZRes;
                tiff.XLower = par.XLower;
                tiff.YLower = par.YLower;
                tiff.ZLower = par.ZLower;

                if (!(par.InputTextFile is null))
                {
                    if (!tiff.ReadPointsFromCsv(par.InputTextFile.FullName))
                    {
                        Console.WriteLine($"Could not read {par.InputTextFile.Name}.");
                        return -1;
                    }
                }
                tiff.CleanNullPoints();
                tiff.CommonPoints.Helmert(par.C0, par.Cl, par.Sn, false);

                if (!tiff.CommonPoints.PopulationGridFromCsv(par.C0, par.Cl, par.Sn))
                {
                    Console.WriteLine($"Gridding failed.");
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
