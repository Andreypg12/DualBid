using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace DualBid.Hubs
{
    [Authorize]
    public class AuctionHub : Hub
    {
        private readonly ILogger<AuctionHub> _logger;

        public AuctionHub(ILogger<AuctionHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinAuctionGroup(string auctionId)
        {
            if (string.IsNullOrWhiteSpace(auctionId))
                throw new HubException("auctionId vacío.");

            var groupName = $"auction-{auctionId}";
            _logger.LogInformation("JoinAuctionGroup: connection={ConnectionId} group={Group}", Context.ConnectionId, groupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RegisterUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("userId vacío.");

            var groupName = $"user-{userId}";
            _logger.LogInformation("RegisterUser: connection={ConnectionId} group={Group}", Context.ConnectionId, groupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // NUEVO: Para notificar tiempo restante
        public async Task NotifyTimeRemaining(string auctionId, string timeRemaining)
        {
            var groupName = $"auction-{auctionId}";
            await Clients.Group(groupName).SendAsync("TimeUpdate", new
            {
                auctionId = auctionId,
                timeRemaining = timeRemaining,
                isEndingSoon = true
            });
        }
    }
}