using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class Csvs2Ct2Command : Command
    {
        public Csvs2Ct2Command(string name, string description = null) : base(name, description)
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
            }
        }
    }

}
