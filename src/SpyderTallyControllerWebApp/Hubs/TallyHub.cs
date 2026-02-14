using Microsoft.AspNetCore.SignalR;
using SpyderTallyControllerWebApp.Models;

namespace SpyderTallyControllerWebApp.Hubs
{
    public class TallyHub : Hub
    {
        public const string TallyStatusUpdatedMessage = "TallyStatusUpdated";

        private readonly IRelayRepository _relayRepository;

        public TallyHub(IRelayRepository relayRepository)
        {
            _relayRepository = relayRepository;
        }

        public override async Task OnConnectedAsync()
        {
            // Send current tally status to newly connected clients
            var currentStatus = _relayRepository.GetRelayStatus();
            await Clients.Caller.SendAsync(TallyStatusUpdatedMessage, currentStatus);
            await base.OnConnectedAsync();
        }
    }
}
