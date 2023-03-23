using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ControllableDevice
{
    public static class Ping
    {
        private static string _executablePath = @"C:\windows\system32\PING.EXE";

        public static bool Send(IPAddress address)
        {
            var process = new Process();
            process.StartInfo.FileName = _executablePath;
            process.StartInfo.Arguments = address.ToString();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return true;
            }
            else return false;
        }
    }
}
