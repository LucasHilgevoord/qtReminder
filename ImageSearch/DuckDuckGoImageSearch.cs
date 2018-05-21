using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace qtReminder.ImageSearch
{
    public class DuckDuckGoImageSearch
    {
        public static string[] SearchImage(string query, int max_results = 1)
        {
            if(max_results <= 0) throw new Exception("max results can not be below or equal to 0");

            string filename = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "imagesearch.py"
                : "imagesearchold.py";
            
            ProcessStartInfo processInfo = new ProcessStartInfo($"python")
            {
                Arguments = $"{filename} {query}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (Process process = Process.Start(processInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string derr = process.StandardError.ReadToEnd();
                    string result = reader.ReadToEnd();
                    return result.Split("\n");
                }
            }
        }
    }
}