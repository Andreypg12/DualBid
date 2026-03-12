using Microsoft.AspNetCore.SignalR;

namespace DualBid.Hubs
{
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
    }
}