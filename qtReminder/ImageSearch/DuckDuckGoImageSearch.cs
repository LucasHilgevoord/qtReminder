using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace qtReminder.ImageSearch
{
    public static class DuckDuckGoImageSearch
    {
        /// <summary>
        ///     Requires Python 2 for Mac OS and Linux. (Check if you can run it using "python -v" in the commandline)
        ///     Requires Python 3 for Windows. (Check if you can run it using "python -v" in the commandline)
        /// </summary>
        /// <param name="query">What to look for</param>
        /// <param name="max_results">How many results to return (doesn't do anything yet, returns max of 100)</param>
        /// <returns>a list of strings with the images.</returns>
        /// <exception cref="Exception">max_results is 0, or less. Don't do this.</exception>
        public static string[] SearchImage(string query, int max_results = 1)
        {
            if (max_results <= 0) throw new Exception("max results can not be below or equal to 0");

            var filename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "imagesearch.py"
                : "imagesearchold.py";

            var processInfo = new ProcessStartInfo($"python")
            {
                Arguments = $"{filename} {query}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(processInfo))
            {
                using (var reader = process.StandardOutput)
                {
                    var derr = process.StandardError.ReadToEnd();
                    var result = reader.ReadToEnd();
                    return result.Split("\n");
                }
            }
        }
    }
}