using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SpyderTallyControllerWebApp.Models
{
    public class SystemHealthRepository : ISystemHealthRepository
    {
        public Task<string> GetApplicationVersion()
        {
            string version = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            return Task.FromResult(version);
        }

        public async Task<string> GetCPUUsageInfo()
        {
            string text = await RunProcessAndGetLine("/bin/uptime");
            if(text != null)
            {
                //Example response:
                //14:51:38 up  9:43,  2 users,  load average: 0.33, 0.24, 0.09
                string[] parts = text.Split(' ', 8, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 7)
                    return parts[7];
            }
            return null;
        }

        public async Task<string> GetUptime()
        {
            string text = await RunProcessAndGetLine("/bin/uptime");
            if (text != null)
            {
                //Example response:
                //14:51:38 up  9:43,  2 users,  load average: 0.33, 0.24, 0.09
                string[] parts = text.Split(',', 2, StringSplitOptions.RemoveEmptyEntries);
                string[] parts2 = parts[0].Split(' ', 4);
                return parts2.Last();
            }
            return null;
        }

        public async Task<string> GetMemoryUsageInfo()
        {
            string text = await RunProcessAndGetLine("/bin/cat", "/proc/meminfo");
            string total = SplitAndGetValue(text, "MemTotal");
            string free = SplitAndGetValue(text, "MemFree");
            return $"{free} free / {total} total";
        }

        public async Task<string> GetRootFileSystemUsageInfo()
        {
            string text = await RunProcessAndGetLine("/bin/df", "-h");
            string rootLine = SplitAndGetValue(text, "/dev/root", " ");
            if (rootLine != null)
            {
                string[] parts = rootLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 5)
                {
                    return $"{parts[2]} free / {parts[0]} total";
                }
            }
            return null;
        }

        public async Task<string> GetSystemModel()
        {
            string text = await RunProcessAndGetLine("/bin/cat", "/proc/cpuinfo");
            return SplitAndGetValue(text, "Model");
        }

        private string SplitAndGetValue(string fullText, string prefix, string delimiter = ":")
        {
            if (string.IsNullOrWhiteSpace(fullText))
                return null;

            string[] splitBy = new string[] { "\r", "\n", Environment.NewLine };
            string[] lines = fullText.Split(splitBy, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if(line.StartsWith(prefix))
                {
                    string[] split = line.Split(delimiter, 2);
                    if(split.Length == 2)
                    {
                        return split[1].Trim();
                    }
                }
            }
            return null;
        }

        private async Task<string> RunProcessAndGetLine(string processName, string args = null)
        {
            //Sanity check on process
            if (!OperatingSystem.IsLinux())
                return null;

            try
            {
                Process process = Process.Start(processName, args);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string response = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();
                return response;
            }
            catch(Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
