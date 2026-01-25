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
        private readonly List<string> servers = new List<string>();
        private readonly ReaderWriterLockSlim serversLock = new ReaderWriterLockSlim();

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
            //See if we already have this server
            serversLock.EnterReadLock();
            bool exists = servers.Contains(serverInfo.Address);
            serversLock.ExitReadLock();

            //Do we need to add this?
            if(exists)
            {
                serversLock.EnterWriteLock();
                servers.Add(serverInfo.Address);
                serversLock.ExitWriteLock();
            }
        }

        public async Task<List<string>> GetServersAsync()
        {
            try
            {
                serversLock.EnterReadLock();
                return [.. servers];
            }
            finally
            {
                serversLock.ExitReadLock();
            }
        }

        public async Task<List<string>> GetSourcesAsync(string serverIP)
        {
            //Connect to server (note we don't actually care about the hardware type for this simple call)
            var client = new SpyderUdpClient(HardwareType.Spyder300, serverIP);
            try
            {
                if (await client.StartupAsync())
                {
                    var sources = await client.GetSources();
                    if(sources != null)
                        return sources?.Select(s => s.Name).ToList();
                }
            }
            finally
            {
                await client?.ShutdownAsync();
            }

            return [];
        }
    }
}
