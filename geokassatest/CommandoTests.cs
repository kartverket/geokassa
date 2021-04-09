using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text; 
using geokassa;
using gridfiles;
using Xunit;

namespace geokassatests
{
    public class CommandoTests
    {
        [Fact]
        public void JsonTinTest()
        {
            var rootCommand =  new RootCommand()
            {
                new JsonTinCommand("jsontin", "Makes triangulated TIN from point clouds")
            };
        }
    }
}
