using Spyder.Client.Net.DrawingData.Deserializers;
using Spyder.Client.Net.Notifications;

namespace SpyderTallyControllerWebApp.Models
{
    public interface ISpyderRepository
    {
        event DrawingDataReceivedHandler DrawingDataReceived;

        Task<List<string>> GetServersAsync();

        Task<List<string>> GetSourcesAsync(string serverIP);
    }
}
