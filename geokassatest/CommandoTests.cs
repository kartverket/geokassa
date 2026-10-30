using geokassa;
using gridfiles;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace geokassatests
{
    public class CommandoTests
    {
        [Theory]
        [InlineData(
            "geokassa",
            "jsontin",
            "no_kv_ETRS89NO_NGO48_TIN.csv",
            "geokassa.json",
            "--epsgsource",
            "EPSG:4258",
            "--epsgtarget",
            "EPSG:4273",
            "--version",
            "1.2")]
        public static void JsonTinTest(params string[] args)
        {
            try
            {
                var rootCommand = new RootCommand()
                {
                    new JsonTinCommand("jsontin", "Makes triangulated TIN from point clouds")
                };

                var _ = new CommandLineBuilder(rootCommand)
                    .UseVersionOption()
                    .UseHelp()
                    .UseEnvironmentVariableDirective()
                    .UseParseDirective()
                    .UseDebugDirective()
                    .UseSuggestDirective()
                    .RegisterWithDotnetSuggest()
                    .UseTypoCorrections()
                    .UseParseErrorReporting()
                    .CancelOnProcessTermination()
                    .Build();
                
                /*  if (rootCommand.Invoke(args) == 0)
                 *  Assert.False(false);
                 *  else
                 *  Assert.False(true);
                 */

                 rootCommand.InvokeAsync(args).Wait();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Theory]
        [InlineData(
          "C:\\Users\\himsve\\Onedrive - Statens Kartverk\\Prosjekter\\Høgde\\Ny_HREF\\HREF2025A_NN2000_EUREF89.bin",
          "C:\\Users\\himsve\\Onedrive - Statens Kartverk\\Prosjekter\\Høgde\\Ny_HREF\\HREF2025A_NN2000_EUREF89.gsb"
        )]
        public async Task Bin2GsbTest(params string[] args)
        {
            try
            {
                var rootCommand = new Bin2GsbCommand("bin2gsb", "Converts bin file to gsb file");

                var exitCode = await rootCommand.InvokeAsync(args);

                Assert.Equal(0, exitCode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
