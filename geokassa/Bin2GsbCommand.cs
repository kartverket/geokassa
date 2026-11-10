using BitMiracle.LibTiff.Classic;
using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class Bin2GsbCommand : Command
    {
        public Bin2GsbCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input bin file") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("output", "Output gsb file") { ArgumentType = typeof(FileInfo) });
            
            AddOption(new Option("--epsgsource", "Source EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgsource"), IsRequired = false });
            AddOption(new Option("--epsgtarget", "Target EPSG CRS ('autority:XXXX')") { Argument = new Argument<string>("epsgtarget"), IsRequired = true });

            Handler = CommandHandler.Create((Bin2GsbCommandParams pars) =>
            {
                return HandleCommand(pars);
            });
        }

        private int HandleCommand(Bin2GsbCommandParams pars)
        { 
            try
            {
                var gtx = new GtxFile(pars.Input.FullName);

                if (!gtx.ReadBin(pars.Input.FullName))
                {
                    Console.WriteLine($"Importing of bin file {pars.Input.Name} failed.");
                    return -1;
                }

                var gsb = new GsbFile
                {
                    OutputFileName = pars.Output.FullName,
                    DataField1 = gtx.Data,
                    GridParam = gtx.GridParam
                };
                var epsgSource = new CrsCode(pars.EpsgSource ?? "");
                gsb.EpsgSource = epsgSource;

                var epsgTarget = new CrsCode(pars.EpsgTarget?? "");
                gsb.EpsgTarget = epsgTarget;

                if (!gsb.GenerateGridFile(pars.Output.FullName))
                {
                    Console.WriteLine($"Importing of gsb file {pars.Output.Name} failed.");
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