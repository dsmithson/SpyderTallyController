using Spyder.Client;
using Spyder.Client.Common;
using Spyder.Client.Net;
using Spyder.Client.Net.DrawingData.Deserializers;
using Spyder.Client.Net.Notifications;
using System.Linq;

namespace SpyderTallyControllerWebApp.Models
{
    public class SpyderRepository : ISpyderRepository
    {
        private readonly SpyderServerEventListener serverEventListener;
        private readonly HashSet<string> servers = new HashSet<string>();
        private readonly object serversLock = new object();

        public event DrawingDataReceivedHandler DrawingDataReceived;

        public SpyderRepository(SpyderServerEventListener serverEventListener)
        {
            this.serverEventListener = serverEventListener;
            this.serverEventListener.DrawingDataThrottleInterval = TimeSpan.FromMilliseconds(100);
            this.serverEventListener.ServerAnnounceMessageReceived += ServerEventListener_ServerAnnounceMessageReceived;
            this.serverEventListener.DrawingDataReceived += ServerEventListener_DrawingDataReceived;
        }

        private void ServerEventListener_DrawingDataReceived(object sender, DrawingDataReceivedEventArgs e)
        {
            DrawingDataReceived?.Invoke(this, e);
        }

        private void ServerEventListener_ServerAnnounceMessageReceived(object sender, SpyderServerAnnounceInformation serverInfo)
        {
            lock (serversLock)
            {
                servers.Add(serverInfo.Address); // HashSet.Add handles duplicates automatically
            }
        }

        public Task<List<string>> GetServersAsync()
        {
            lock (serversLock)
            {
                return Task.FromResult(servers.ToList());
            }
        }

        public async Task<List<string>> GetSourcesAsync(string serverIP)
        {
            var timeout = TimeSpan.FromSeconds(2);
            
            try
            {
                var task = GetSourcesInternalAsync(serverIP);
                return await task.WaitAsync(timeout);
            }
            catch (TimeoutException)
            {
                return [];
            }
            catch (Exception)
            {
                return [];
            }
        }

        private async Task<List<string>> GetSourcesInternalAsync(string serverIP)
        {
            var client = new SpyderUdpClient(HardwareType.Spyder300, serverIP);
            try
            {
                if (await client.StartupAsync())
                {
                    var sources = await client.GetSources();
                    return sources?.Select(s => s.Name).ToList() ?? [];
                }
                return [];
            }
            finally
            {
                // Fire and forget shutdown to avoid blocking on cleanup
                _ = Task.Run(async () =>
                {
                    try { await client.ShutdownAsync(); }
                    catch { /* ignore cleanup errors */ }
                });
            }
        }
    }
}
