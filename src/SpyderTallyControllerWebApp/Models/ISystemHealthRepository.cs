namespace SpyderTallyControllerWebApp.Models
{
    public interface ISystemHealthRepository
    {
        Task<string> GetCPUUsageInfo();

        Task<string> GetMemoryUsageInfo();

        Task<string> GetRootFileSystemUsageInfo();

        Task<string> GetApplicationVersion();

        Task<string> GetSystemModel();

        Task<string> GetUptime();
    }
}
