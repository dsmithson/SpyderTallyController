namespace SpyderTallyControllerWebApp.ViewModels
{
    public class HomeViewModel
    {
        public bool[] CurrentTallyStatus { get; set; } = new bool[8];

        public string Uptime { get; set; } = "Unknown";

        public string Version { get; set; } = "Unknown";

        public string MemoryUsage { get; set; } = "Unknown";

        public string CpuUsage { get; set; } = "Unknown";

        public string DiskUsage { get; set; } = "Unknown";

        public string Model { get; set; } = "Unknown";

    }
}
