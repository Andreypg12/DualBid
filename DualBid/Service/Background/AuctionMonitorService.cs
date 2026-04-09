using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using DualBid.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DualBid.Application.DTOs;

namespace DualBid.Services.BackgroundServices
{
    public class AuctionMonitorService : BackgroundService
    {
        private readonly ILogger<AuctionMonitorService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

        public AuctionMonitorService(
            ILogger<AuctionMonitorService> logger,
            IServiceProvider serviceProvider,
            IHubContext<AuctionHub> hubContext)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Auction Monitor Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndProcessAuctions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing auctions");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckAndProcessAuctions()
        {
            using var scope = _serviceProvider.CreateScope();
            var auctionService = scope.ServiceProvider.GetRequiredService<IServiceAuction>();
            var comicService = scope.ServiceProvider.GetRequiredService<IServiceComic>();

            var activeAuctions = await auctionService.ListActiveAsync();
            var now = DateTime.Now;

            foreach (var auction in activeAuctions)
            {
                try
                {
                    //await ProcessAuctionActivation(auction, auctionService, now);
                    await ProcessAuctionClosure(auction, auctionService, comicService, now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Error processing auction {auction.Id}");
                }
            }
        }

        private async Task ProcessAuctionActivation(AuctionDTO auction, IServiceAuction auctionService, DateTime now)
        {
            // Activar subastas que están en espera y ya llegó su hora de inicio
            if (auction.State.Id == 1 && auction.StartDate <= now)
            {
                _logger.LogInformation($"🎯 Activando subasta {auction.Id}");

                await auctionService.UpdateStateAsync(auction.Id, 2);

                // Notificar a todos en el grupo de la subasta
                await _hubContext.Clients.Group($"auction-{auction.Id}")
                    .SendAsync("AuctionActivated", new
                    {
                        auctionId = auction.Id.ToString(),
                        message = "¡La subasta ha comenzado!",
                        comicTitle = auction.Comic.Title
                    });

                // Notificar al creador específicamente
                await _hubContext.Clients.Group($"user-{auction.CreatorUser.Id}")
                    .SendAsync("YourAuctionActivated", new
                    {
                        auctionId = auction.Id.ToString(),
                        message = $"Tu subasta de {auction.Comic.Title} ha sido activada automáticamente"
                    });
            }
        }

        private async Task ProcessAuctionClosure(AuctionDTO auction, IServiceAuction auctionService,
            IServiceComic comicService, DateTime now)
        {
            // Cerrar subastas que ya expiraron
            if (auction.State.Id == 2 && auction.ExpectedEndDate <= now)
            {
                var hasBids = auction.Bids != null && auction.Bids.Any();
                var finalState = hasBids ? 3 : 4; // 3 = Finalizada con pujas, 4 = Cancelada sin pujas

                _logger.LogInformation($"🔒 Cerrando subasta {auction.Id}. Estado final: {finalState}");

                await auctionService.UpdateStateAsync(auction.Id, finalState);

                // Si no tuvo pujas, liberar el cómic
                if (!hasBids)
                {
                    await comicService.UpdateAvailabilityAsync(auction.Comic.Id, true);
                }

                // Determinar ganador si hay pujas
                var winningBid = hasBids ?
                    auction.Bids?.OrderByDescending(b => b.AmountOffered).FirstOrDefault() : null;

                // Notificar cierre a todos en el grupo
                await _hubContext.Clients.Group($"auction-{auction.Id}")
                    .SendAsync("AuctionClosed", new
                    {
                        auctionId = auction.Id.ToString(),
                        hasBids = hasBids,
                        finalState = finalState,
                        winningBid = winningBid?.AmountOffered,
                        winnerUserId = winningBid?.UserId.ToString(),
                        winnerName = winningBid?.User?.CompleteName,
                        message = hasBids ?
                            $"¡Subasta finalizada! Ganador: {winningBid?.User?.CompleteName}" :
                            "Subasta finalizada sin pujas"
                    });

                // Notificar al ganador específicamente
                if (winningBid != null)
                {
                    await _hubContext.Clients.Group($"user-{winningBid.UserId}")
                        .SendAsync("YouWonAuction", new
                        {
                            auctionId = auction.Id.ToString(),
                            comicTitle = auction.Comic.Title,
                            winningAmount = winningBid.AmountOffered,
                            message = $"¡Felicidades! Has ganado la subasta de {auction.Comic.Title}"
                        });
                }

                // Notificar al creador
                await _hubContext.Clients.Group($"user-{auction.CreatorUser.Id}")
                    .SendAsync("YourAuctionEnded", new
                    {
                        auctionId = auction.Id.ToString(),
                        comicTitle = auction.Comic.Title,
                        hasBids = hasBids,
                        finalAmount = winningBid?.AmountOffered,
                        message = hasBids ?
                            $"Tu subasta finalizó con una puja ganadora de ${winningBid?.AmountOffered:N0}" :
                            "Tu subasta finalizó sin pujas"
                    });
            }
        }
    }
}