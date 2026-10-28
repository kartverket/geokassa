using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
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
                var gtx = new GtxFile();

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
            }
        }
    }
}
