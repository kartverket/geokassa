using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
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
            }
        }
    }

}
