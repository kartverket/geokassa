using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{

    /*
    ┌────────────────────────────────────────────────────────┐
    │ Overview Header (11 records x 16 bytes = 176 bytes)    │
    ├────────────────────────────────────────────────────────┤
    │ Sub-File Header (11 records x 16 bytes = 176 bytes)   │
    ├────────────────────────────────────────────────────────┤
    │ Grid Nodes (Lat/Lon/Accuracy data... variable size)   │
    ├────────────────────────────────────────────────────────┤
    │ ... (More Sub-Files & Grid Nodes can follow)           │
    ├────────────────────────────────────────────────────────┤
    │ End Record                                             │
    └────────────────────────────────────────────────────────┘
    */

    public class Bin2GsbCommand : Command
    {
        public Bin2GsbCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input bin file") { ArgumentType = typeof(FileInfo) });
            AddArgument(new Argument<FileInfo>("output", "Output gsb file") { ArgumentType = typeof(FileInfo) });

            Handler = CommandHandler.Create<FileInfo, FileInfo>((FileInfo input, FileInfo output) => HandleCommand(input, output));
        }

        private int HandleCommand(FileInfo input, FileInfo output)
        { 
            try
            {
                var gsb = new GsbFile();

                if (!gsb.Read(input.FullName))
                {
                    Console.WriteLine($"Importing of bin file {input.Name} failed.");
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