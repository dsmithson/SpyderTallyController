namespace SpyderTallyControllerWebApp.Models
{
    public interface ISpyderRepository
    {
        Task<List<string>> GetServersAsync();

        Task<List<string>> GetSourcesAsync(string serverIP);
    }
}
