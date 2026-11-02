using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class GsbImportCommand : Command
    {
        public GsbImportCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("input", "Input gsb file") { ArgumentType = typeof(FileInfo) });

            Handler = CommandHandler.Create<FileInfo>((FileInfo input) => HandleCommand(input));
        }

        private int HandleCommand(FileInfo input)
        { 
            try
            {
                var gsb = new GsbFile(input.FullName);                 

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