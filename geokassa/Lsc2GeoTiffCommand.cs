using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class Lsc2GeoTiffCommand : Command
    {
        public Lsc2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("inputjson", "Input from Json at geocentric-pointlist.schema.json schema"));
            AddArgument(new Argument<FileInfo>("output", "Output geotiff file"));

            AddOption(new Option<GeoTiffFile.TiffOutputTypeshort>("--type", "TiffOutputType") { Argument = new Argument<GeoTiffFile.TiffOutputTypeshort>("type") });
            AddOption(new Option("--gridname", "Grid name") { Argument = new Argument<string>("gridname"), IsRequired = true });
            AddOption(new Option("--email", "Product manager") { Argument = new Argument<string>("email") });
            AddOption(new Option("--area", "Area of use") { Argument = new Argument<string>("area") });
            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option("--dim", "Dimension") { Argument = new Argument<int>("dim") });
            AddOption(new Option("--epsg2d", "Source EPSG interpolation ('autority:XXXX')") { Argument = new Argument<string>("epsg2d"), IsRequired = true });
            AddOption(new Option("--epsgsource", "Source EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgsource"), IsRequired = true });
            AddOption(new Option("--epsgtarget", "Target EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgtarget"), IsRequired = true });
            AddOption(new Option("--lle", "Lower left longitude in grid (deg)") { Name = "LowerLeftLongitude", Argument = new Argument<double>("lle"), IsRequired = true });
            AddOption(new Option("--lln", "Lower left latitude in grid (deg)") { Name = "LowerLeftLatitude", Argument = new Argument<double>("lln"), IsRequired = true });
            AddOption(new Option("--de", "Longitude resolution in grid (deg)") { Name = "DeltaLongitude", Argument = new Argument<double>("de"), IsRequired = true });
            AddOption(new Option("--dn", "Latitude resolution in grid (deg)") { Name = "DeltaLatitude", Argument = new Argument<double>("dn"), IsRequired = true });
            AddOption(new Option("--rows", "Number of rows in grid") { Argument = new Argument<int>("rows"), IsRequired = true });
            AddOption(new Option("--cols", "Number of cols in grid") { Argument = new Argument<int>("cols"), IsRequired = true });
            AddOption(new Option("--tilesize", "Tile size (multiple of 16)") { Argument = new Argument<int>("tilesize"), IsRequired = true });
            AddOption(new Option("--agl", "Heights above ground level (m)") { Argument = new Argument<double>("agl") });
            AddOption(new Option("--c0", "Covariance signal - LSC (m2)") { Argument = new Argument<double>("c0"), IsRequired = true });
            AddOption(new Option("--cl", "Correlastion length - LSC (m)") { Argument = new Argument<double>("cl"), IsRequired = true });
            AddOption(new Option("--sn", "Covariance noise - LSC (m)") { Argument = new Argument<double>("sn"), IsRequired = true });

            Handler = CommandHandler.Create((Lsc2GeoTiffCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(Lsc2GeoTiffCommandParams par)
        {
            try
            {
                var tiff = new GeoTiffFile();

                tiff.ImageDescription = par.Desc ?? "";
                tiff.Grid_name = par.GridName ?? "";
                tiff.Area_of_use = par.Area ?? "";
                tiff.Email = par.Email ?? "";
                tiff.Dimensions = par.Dim == 0 ? 3 : par.Dim;
                tiff.TileSize = par.TileSize;
                tiff.TiffOutput = (GeoTiffFile.TiffOutputType)par.Type;
                tiff.Epsg2d.CodeString = par.Epsg2d;
                tiff.EpsgSource.CodeString = par.EpsgSource ?? "";
                tiff.EpsgTarget.CodeString = par.EpsgTarget ?? "";
                tiff.NRows = par.Rows;
                tiff.NColumns = par.Cols;
                tiff.LowerLeftLatitude = (double)par.LowerLeftLatitude;
                tiff.LowerLeftLongitude = (double)par.LowerLeftLongitude;
                tiff.DeltaLatitude = (double)par.DeltaLatitude;
                tiff.DeltaLongitude = (double)par.DeltaLongitude;
                tiff.CommonPoints.Agl = par.Agl;

                tiff.CommonPointList.SourceCrs = par.EpsgSource ?? "";
                tiff.CommonPointList.TargetCrs = par.EpsgTarget ?? "";

                if (!(par.InputSource is null))
                {
                    if (!tiff.ReadSourceFromFile(par.InputSource.FullName))
                    {
                        Console.WriteLine($"Could not read {par.InputSource.Name}.");
                        return -1;
                    }
                }
                if (!(par.InputTarget is null))
                {
                    if (!tiff.ReadTargetFromFile(par.InputTarget.FullName))
                    {
                        Console.WriteLine($"Could not read {par.InputTarget.Name}.");
                        return -1;
                    }
                }
                if (!(par.InputJson is null))
                {
                    tiff.CommonPointList =
                        (CommonPointList)JsonGenerator.ReadFromJsonFile<CommonPointList>(tiff.CommonPointList, par.InputJson.FullName);
                }
                // TODO: Remove this!
                if (false)
                {
                    tiff.OutputFileName = "C:\\Temp\\test.schema.json";

                    if (!JsonGenerator.GenerateSchema<CommonPointList>(
                        "C:\\Temp\\test.schema.json",
                        "https://github.com/himsve/geokassa/tree/master/gridfiles/schemas/geocentric-pointlist.schema.json",
                        "http://json-schema.org/draft-07/schema#"))
                        return -1;

                    tiff.CommonPointList.FileName = "geocentric-pointlist.schema.json";
                    JsonGenerator.WriteToJsonFile(tiff.CommonPointList, "C:\\Temp\\test.json");
                }
                tiff.CleanNullPoints();

                tiff.CommonPoints.Helmert(par.C0, par.Cl, par.Sn, false);

                if (!tiff.PopulatedGrid(par.C0, par.Cl, par.Sn))
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
