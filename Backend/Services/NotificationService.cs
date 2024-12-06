using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Backend.Models;

namespace Backend.Services
{
    public class NotificationService
    {
        private readonly IHubContext<EmissionHub> _hubContext;

        public NotificationService(IHubContext<EmissionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyEmissionUpdate(string userId, EmissionRecord emission)
        {
            await _hubContext.Clients.Group(userId).SendAsync("EmissionUpdated", emission);
        }

        public async Task NotifyEmissionCreated(string userId, EmissionRecord emission)
        {
            await _hubContext.Clients.Group(userId).SendAsync("EmissionCreated", emission);
        }

        public async Task NotifyEmissionDeleted(string userId, int emissionId)
        {
            await _hubContext.Clients.Group(userId).SendAsync("EmissionDeleted", emissionId);
        }

        public async Task NotifyStatisticsUpdate(string userId, UserEmissionStatistics statistics)
        {
            await _hubContext.Clients.Group(userId).SendAsync("StatisticsUpdated", statistics);
        }
    }
} 