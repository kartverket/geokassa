using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gridfiles;

namespace geokassa
{
    public class JsonTinCommand : Command
    {
        public JsonTinCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input file name"));
            AddArgument(new Argument<FileInfo>("output", "Output json file name"));

            AddOption(new Option("--epsgsource", "EPSG code source CRS") { Argument = new Argument<string>("epsgsource"), IsRequired = true });
            AddOption(new Option("--epsgtarget", "EPSG code target CRS") { Argument = new Argument<string>("epsgtarget"), IsRequired = true });
            AddOption(new Option("--version", "Grid version") { Argument = new Argument<string>("version") });

            Handler = CommandHandler.Create((TinModelParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(TinModelParams par)
        {
            try
            {
                var tin = new TinModel();

                tin.OutputFileName = par.Output.FullName;
                tin.EpsgSource.CodeString = par.EpsgSource;
                tin.EpsgTarget.CodeString = par.EpsgTarget;
                tin.Coord.Version = par.Version;

                if (!tin.CptFile.ReadInputFile(par.Input.FullName))
                {
                    Console.WriteLine($"Could not read from input file {par.Input.Name}");
                    return -1;
                }
                tin.InitTriangleObject();
                if (!tin.Triangulate())
                {
                    Console.WriteLine($"Trianguleringa feila.");
                    return -1;
                }
                if (!tin.SerializeJson())
                {
                    Console.WriteLine($"Serialisering til Json feila.");
                    return -1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
                throw ex;
            }
        }
    }

    public class Lsc2GeoTiffCommand : Command
    {
        public Lsc2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("inputsource", "Source csv file (ID, X/lon, Y/lat, Z/h, Epoch)"));
            AddArgument(new Argument<FileInfo>("inputtarget", "Target csv file (ID, X/lon, Y/lat, Z/h, Epoch)"));
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

                if (!tiff.ReadSourceFromFile(par.InputSource.FullName))
                {
                    Console.WriteLine($"Could not read {par.InputSource.Name}.");
                    return -1;
                }
                if (!tiff.ReadTargetFromFile(par.InputTarget.FullName))
                {
                    Console.WriteLine($"Could not read {par.InputTarget.Name}.");
                    return -1;
                }
                tiff.CleanNullPoints();
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
                throw ex;
            }
        }
    }

    public class Bin2GeoTiffCommand : Command
    {
        public Bin2GeoTiffCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input bin file"));
            AddArgument(new Argument<FileInfo>("output", "Output GeoTiff file"));

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

                // Test av ReadGeoTiff
                string inputTiffFile = @"C:\Users\himsve\Miniconda3\Library\share\proj\eur_nkg_nkgrf17vel.tif";
                if (!tiff.ReadGeoTiff(inputTiffFile))
                {
                    Console.WriteLine($"Tiff file {inputTiffFile} does not exist.");
                    return -1;
                }

                tiff.GetGeoTiffValue(60.1d, 10.1d, out object[] values);
            
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
                throw ex;
            }
        }
    }

    public class Bin2GtxCommand : Command
    {
        public Bin2GtxCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input bin file") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("output", "Output gtx file") { ArgumentType = typeof(FileInfo) });

            Handler = CommandHandler.Create<FileInfo, FileInfo>((FileInfo input, FileInfo output) => HandleCommand(input, output));
        }

        private int HandleCommand(FileInfo input, FileInfo output)
        {
            try
            {
                var gtx = new GtxFile(/*input.FullName*/);

                if (!gtx.ReadBin(input.FullName))
                {
                    Console.WriteLine($"Importing of bin file {input.Name} failed.");
                    return -1;
                }
                if (!gtx.GenerateGridFile(output.FullName))
                {
                    Console.WriteLine($"Importing of gtx file {output.Name} failed.");
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
                throw ex;
            }        
        }
    }

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
                throw ex;
            }
        }
    }

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
            AddOption(new Option("--epsgtarget", "Target EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgtarget"), IsRequired = true });
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
                throw ex;
            }
        }
    }

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
                Console.Error.WriteLine(ex);
                return -1;
                throw ex;
            }
        }
    }
    
    public class TiffValueCommand : Command
    {
        public TiffValueCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("gridfile", "Input GeoTiff file") { ArgumentType = typeof(FileInfo) });

            Handler = CommandHandler.Create<FileInfo>((FileInfo gridfile) => HandleCommand(gridfile));
        }

        private int HandleCommand(FileInfo gridfile)
        {
            try
            {
                if (!File.Exists(gridfile.FullName))
                {
                    Console.WriteLine($"The file {gridfile.Name} does not exist.");
                    return -1;
                }
                var tiff = new GeoTiffFile();

                if (!tiff.ReadGeoTiff(gridfile.FullName))
                {
                    Console.WriteLine($"Could not read the file {gridfile.Name}.");
                    return -1;
                }
                Console.WriteLine("Enter latitude longitude: ");

                do
                {
                    while (!Console.KeyAvailable)
                    {
                        while (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        {
                            return 0;
                        }

                        var inputCoord = Console.ReadLine().Split(new char[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (inputCoord.Length != 2)
                            continue;

                        if (!double.TryParse(inputCoord[0], out double latInput) || !double.TryParse(inputCoord[1], out double lonInput))
                        {
                            Console.WriteLine("Input parsing failed");
                            continue;
                        }
                        if (!tiff.GetGeoTiffValue(latInput, lonInput, out object[] output))
                        {
                            Console.WriteLine("Corrupt output value");
                            continue;
                        }
                        foreach (var v in output)
                        {
                            Console.Write($"{v} ");
                        }
                        Console.WriteLine();
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
                throw ex;
            }
        }
    }

    public class Csvs2Ct2 : Command
    {
        public Csvs2Ct2(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("fromsys", "Input csv From system") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("tosys", "Input csv To system") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("output", "Output Ct2 file") { ArgumentType = typeof(FileInfo) });

            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option("--flat", "False latitude") { Argument = new Argument<double>("flat") });
            AddOption(new Option("--flon", "False longitude") { Argument = new Argument<double>("flon") });
            AddOption(new Option<GridFile.Direction>("--dir", "Direction of transformation") { Argument = new Argument<GridFile.Direction>("dir") });
            AddOption(new Option("--gjs", "Input geojson file") { Argument = new Argument<FileInfo>("gjs") });

            Handler = CommandHandler.Create((Csvs2Ct2CommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(Csvs2Ct2CommandParams par)
        {
            try
            {
                var ct2 = new Ct2File();

                ct2.FalseLat = par.FLat;
                ct2.FalseLon = par.FLon;
                ct2.Description = par.Desc;

                if (par.GJs != null && !ct2.ReadGeoJsonAreas(par.GJs.FullName))
                {
                    Console.WriteLine($"Cound not parse the geojson file {par.GJs.Name}.");
                    return -1;
                }
                if (par.FromSys != null && !ct2.ReadSystem1PointList(par.FromSys.FullName))
                {
                    Console.WriteLine($"Cound not read the csv file {par.FromSys.Name}.");
                    return -1;
                }
                if (par.ToSys != null && !ct2.ReadSystem2PointList(par.ToSys.FullName))
                {
                    Console.WriteLine($"Cound not read the csv file {par.ToSys.Name}.");
                    return -1;
                }
                if (!ct2.ComputeParameters())
                {
                    Console.WriteLine($"Cound not compute parameters.");
                    return -1;
                }
                if (!ct2.ComputeGridData(par.Dir))
                {
                    Console.WriteLine($"Cound not compute grid data.");
                    return -1;
                }
                if (!ct2.GenerateGridFile(par.Output.FullName))
                {
                    Console.WriteLine($"Cound not write the ct2 file {par.Output.Name}.");
                    return -1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
                throw ex;
            }
        }
    }

    public class MakeGrid : Command
    {
        public MakeGrid(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("output", "Output csv file") { ArgumentType = typeof(FileInfo) });

            AddOption(new Option("--lle", "Lower left longitude in grid (deg)") { Name = "LowerLeftLongitude", Argument = new Argument<double>("lle"), IsRequired = true });
            AddOption(new Option("--lln", "Lower left latitude in grid (deg)") { Name = "LowerLeftLatitude", Argument = new Argument<double>("lln"), IsRequired = true });
            AddOption(new Option("--de", "Longitude resolution in grid (deg)") { Name = "DeltaLongitude", Argument = new Argument<double>("de"), IsRequired = true });
            AddOption(new Option("--dn", "Latitude resolution in grid (deg)") { Name = "DeltaLatitude", Argument = new Argument<double>("dn"), IsRequired = true });
            AddOption(new Option("--rows", "Number of rows in grid") { Name = "Rows", Argument = new Argument<int>("rows"), IsRequired = true });
            AddOption(new Option("--cols", "Number of cols in grid") { Name = "Columns", Argument = new Argument<int>("cols"), IsRequired = true });
            AddOption(new Option("--sep", "Columns separator (\" \", \";\")") { Argument = new Argument<string>("sep")});


            Handler = CommandHandler.Create((MakeGridCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(MakeGridCommandParams par)
        {
            try
            {
                if (par.Output == null || par.Output.Name == null)
                {
                    Console.WriteLine($"{par.Output.Name} is not a valid outout file.");
                    return -1;
                }

                var p = new PointCloudFile(par.Output.FullName)
                {
                    DeltaEast = (decimal)par.DeltaLongitude,
                    DeltaNorth = (decimal)par.DeltaLatitude,
                    LowerLeftEast = (decimal)par.LowerLeftLongitude,
                    LowerLeftNorth = (decimal)par.LowerLeftLatitude,
                    NColumns = par.Columns,
                    NRows = par.Rows,
                    Sep = par.Sep
                };

                p.SaveFile();

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
                throw ex;
            }
        }
    }

    public class Helmert : Command
    {
        public Helmert(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("fromsys", "Input csv From system") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("tosys", "Input csv To system") { ArgumentType = typeof(FileInfo) });

            Handler = CommandHandler.Create<FileInfo, FileInfo>((FileInfo fromsys, FileInfo tosys) => HandleCommand(fromsys, tosys));
        }

        private int HandleCommand(FileInfo fromsys, FileInfo tosys)
        {
            try
            {
                var cps = new CommonPointSet();

                if (!cps.ReadSourceFromFile(fromsys.FullName))
                {
                    Console.WriteLine($"Could not read {fromsys.Name}.");
                    return -1;
                }
                if (!cps.ReadTargetFromFile(tosys.FullName))
                {
                    Console.WriteLine($"Could not read {tosys.FullName}.");
                    return -1;
                }
                if (!cps.Helmert(0d, 0d, 0d, true))
                {
                    Console.WriteLine($"Helmert parametres calculation failed");
                    return -1;
                }
                cps.PrintResiduals();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
                throw ex;
            }
        }
    }

    public class MergeGrids : Command
    {
        public MergeGrids(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("grid1", "Primary grid") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("grid2", "Secondary grid") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("gridtarget", "Target grid") { ArgumentType = typeof(FileInfo) });

            AddOption(new Option("--desc", "Description") { Argument = new Argument<string>("desc") });
            AddOption(new Option<GridFile.GridType>("--type", "GridType") { Argument = new Argument<GridFile.GridType>("type"), IsRequired = true });

            Handler = CommandHandler.Create((MergeGridsCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(MergeGridsCommandParams par)
        {
            try
            {
                switch (par.Type)
                {
                    case GridFile.GridType.gtx:
                        return -1;
                    case GridFile.GridType.tiff:
                        return -1;
                    default:
                        var grid1 = new Ct2File();
                        if (!grid1.ReadCt2(par.Grid1.FullName))
                        {
                            Console.WriteLine($"Cound not read the grid file {par.Grid1.Name}.");
                            return -1;
                        }
                        var grid2 = new Ct2File();
                        if (!grid2.ReadCt2(par.Grid2.FullName))
                        {
                            Console.WriteLine($"Cound not read the grid file {par.Grid2.Name}.");
                            return -1;
                        }

                        var mergedFile = new MergedCt2File(grid1, grid2);
                        if (!mergedFile.MergeGrids())
                        {
                            Console.WriteLine($"Cound not merge the grids.");
                            return -1;
                        }
                        mergedFile.Description = par.Desc;
                        mergedFile.GenerateGridFile(par.GridTarget.FullName);

                        return 0;
                }                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
                throw ex;
            }
        }
    }
}
