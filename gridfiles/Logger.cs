using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gridfiles
{
    internal class Logger
    {
        private static readonly string logPath = "app.log";

        public static void Log(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}";
            File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }

    }
}
