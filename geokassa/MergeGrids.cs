using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
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
            }
        }
    }
}
