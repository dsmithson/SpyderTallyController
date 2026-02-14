using Microsoft.AspNetCore.SignalR;
using SpyderTallyControllerWebApp.Hubs;
using SpyderTallyControllerWebApp.Models;

namespace SpyderTallyControllerWebApp.Services
{
    public class TallyStatusBroadcaster : IHostedService
    {
        private readonly IRelayRepository _relayRepository;
        private readonly IHubContext<TallyHub> _hubContext;

        public TallyStatusBroadcaster(IRelayRepository relayRepository, IHubContext<TallyHub> hubContext)
        {
            _relayRepository = relayRepository;
            _hubContext = hubContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _relayRepository.RelayStatusChanged += OnRelayStatusChanged;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _relayRepository.RelayStatusChanged -= OnRelayStatusChanged;
            return Task.CompletedTask;
        }

        private async void OnRelayStatusChanged(object sender, EventArgs e)
        {
            var currentStatus = _relayRepository.GetRelayStatus();
            await _hubContext.Clients.All.SendAsync(TallyHub.TallyStatusUpdatedMessage, currentStatus);
        }
    }
}
