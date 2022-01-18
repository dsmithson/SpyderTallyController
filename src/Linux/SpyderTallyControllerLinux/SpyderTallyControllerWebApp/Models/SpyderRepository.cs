using Spyder.Client;
using System.Linq;

namespace SpyderTallyControllerWebApp.Models
{
    public class SpyderRepository : ISpyderRepository
    {
        private readonly SpyderClientManager spyderManager;

        public SpyderRepository(SpyderClientManager spyderManager, IConfigurationRepository configurationRepository)
        {
            this.spyderManager = spyderManager;
        }

        public async Task<List<string>> GetServersAsync()
        {
            var servers = await spyderManager.GetServers();
            if (servers == null)
                return new List<string>();

            return servers
                .Select(s => s.ServerIP)
                .ToList();
        }

        public async Task<List<string>> GetSourcesAsync(string serverIP)
        {
            var server = await spyderManager.GetServerAsync(serverIP);
            var sources = await server?.GetSources();
            if (sources == null)
                return new List<string>();

            return sources
                .Where(s => !string.IsNullOrWhiteSpace(s?.Name))
                .Select(s => s.Name)
                .ToList();
        }
    }
}
