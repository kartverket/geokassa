using gridfiles;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace geokassa
{
    public class TiffValueCommand : Command
    {
        public TiffValueCommand(string name, string description = null) : base(name, description)
        {
            Name = name;
            Description = description;

            AddArgument(new Argument<FileInfo>("gridfile", "Path to input GeoTiff file") { ArgumentType = typeof(FileInfo) });

            Handler = CommandHandler.Create<FileInfo>((FileInfo gridfile) => HandleCommand(gridfile));

            // dotnet geokassa.dll tiffvalue ~/anaconda3/share/proj/eur_nkg_nkgrf17vel.tif
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
                        var inputCoord = Console.ReadLine().Split(new char[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (inputCoord.Length != 2)
                            return 0;

                        if (inputCoord.Length != 2)
                            continue;

                        if (!double.TryParse(inputCoord[0], out double latInput) || !double.TryParse(inputCoord[1], out double lonInput))
                        {
                            Console.WriteLine("Input parsing failed");
                            continue;
                        }
                        if (!tiff.GetGeoTiffValue(latInput, lonInput, out object[] output))
                        {
                            Console.WriteLine("Corrupt input values");
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
}
