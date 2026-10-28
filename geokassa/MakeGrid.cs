using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
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
            AddOption(new Option("--sep", "Columns separator (\" \", \";\")") { Argument = new Argument<string>("sep") });

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
            }
        }
    }

}
