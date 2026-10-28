using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

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
            }
        }
    }
}
