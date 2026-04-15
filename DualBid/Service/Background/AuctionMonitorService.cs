using DualBid.Application.Services.Interfaces;
using DualBid.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DualBid.Services.BackgroundServices
{
    public class AuctionMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ILogger<AuctionMonitorService> _logger;

        private readonly System.Collections.Concurrent.ConcurrentDictionary<int, DateTime> _scheduledClosings = new();

        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan InitialLoadDelay = TimeSpan.FromSeconds(5);

        public AuctionMonitorService(
            IServiceScopeFactory scopeFactory,
            IHubContext<AuctionHub> hubContext,
            ILogger<AuctionMonitorService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AuctionMonitorService iniciado.");

            try
            {
                await Task.Delay(InitialLoadDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return; // App cerrándose durante el delay inicial, salir limpiamente
            }

            await LoadActiveAuctionsAsync();

            // El loop captura las excepciones para que nunca muera
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCloseExpiredAuctionsAsync();
                }
                catch (Exception ex)
                {
                    // Loguear pero NUNCA dejar que esto mate el loop
                    _logger.LogError(ex, "Error inesperado en CheckAndCloseExpiredAuctionsAsync. El monitor continúa.");
                }

                try
                {
                    //Usar Task.Delay SIN stoppingToken para que una cancelación
                    // no mate el delay con OperationCanceledException no capturada.
                    // Verificamos el token manualmente después.
                    await Task.Delay(CheckInterval);
                }
                catch (OperationCanceledException)
                {
                    break; // App cerrándose, salir limpiamente
                }
            }

            _logger.LogInformation("AuctionMonitorService detenido.");
        }

        private async Task LoadActiveAuctionsAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IServiceAuction>();
                var active = await svc.GetActiveAuctionsAsync();

                int count = 0;
                foreach (var a in active)
                {
                    if (a.EndDate.HasValue)
                    {
                        // Guardar en hora LOCAL para comparar con DateTime.Now
                        var localEnd = a.EndDate.Value.Kind == DateTimeKind.Utc
                            ? a.EndDate.Value.ToLocalTime()
                            : a.EndDate.Value;

                        _scheduledClosings[a.Id] = localEnd;
                        count++;
                        _logger.LogInformation(
                            "Monitor cargó subasta {Id} → cierre: {End:dd/MM/yyyy HH:mm:ss} (local)",
                            a.Id, localEnd);
                    }
                }

                _logger.LogInformation(
                    "Carga inicial completa: {Count} subasta(s) activa(s) en el monitor. Hora local servidor: {Now:HH:mm:ss}",
                    count, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LoadActiveAuctionsAsync. El monitor seguirá sin datos iniciales.");
            }
        }

        private async Task CheckAndCloseExpiredAuctionsAsync()
        {
            var now = DateTime.Now; // Hora local, igual que SQL Server

            // Log de heartbeat cada vez que corre — así sabes que el loop vive
            _logger.LogInformation(
                "Monitor tick: {Now:HH:mm:ss} | Subastas monitoreadas: {Count} | Pendientes: {Pending}",
                now,
                _scheduledClosings.Count,
                string.Join(", ", _scheduledClosings.Select(kv =>
                    $"#{kv.Key}→{kv.Value:HH:mm:ss}")));

            var expired = _scheduledClosings
                .Where(kv => kv.Value <= now)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var id in expired)
            {
                await CloseAuctionAsync(id);
            }
        }

        private async Task CloseAuctionAsync(int auctionId)
        {
            if (!_scheduledClosings.TryRemove(auctionId, out _))
                return;

            _logger.LogInformation("Cerrando subasta {AuctionId}...", auctionId);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IServiceAuction>();

                var result = await svc.CloseAuctionAsync(auctionId);

                if (result == null)
                {
                    _logger.LogWarning(
                        "Subasta {AuctionId}: retornó null (ya cerrada previamente o no encontrada).",
                        auctionId);
                    return;
                }

                _logger.LogInformation(
                    "Subasta {AuctionId} cerrada. Estado={State}, Ganador={Winner}, Monto={Amount}",
                    auctionId, result.FinalStateId,
                    result.WinnerName ?? "ninguno", result.FinalAmount);

                // ── Notificar a todos los que ven la subasta ─────────────────
                await _hubContext.Clients
                    .Group($"auction-{auctionId}")
                    .SendAsync("AuctionClosed", new
                    {
                        auctionId = auctionId,
                        message = result.WinnerName != null
                                         ? $"Auction ended! Winner: {result.WinnerName} with ${result.FinalAmount:N2}"
                                         : "Auction ended with no bids.",
                        hasBids = result.WinnerUserId.HasValue,
                        winnerUserId = result.WinnerUserId,
                        winnerName = result.WinnerName,
                        finalAmount = result.FinalAmount,
                        finalState = result.FinalStateId
                    });

                // ── Notificar al ganador ──────────────────────────────────────
                if (result.WinnerUserId.HasValue)
                {
                    await _hubContext.Clients
                        .Group($"user-{result.WinnerUserId}")
                        .SendAsync("YouWonAuction", new
                        {
                            auctionId = auctionId,
                            message = $"Congratulations! You won \"{result.ComicTitle}\" for ${result.FinalAmount:N2}",
                            finalAmount = result.FinalAmount,
                            comicTitle = result.ComicTitle
                        });
                }

                // ── Notificar al creador ──────────────────────────────────────
                if (result.OwnerUserId.HasValue)
                {
                    await _hubContext.Clients
                        .Group($"user-{result.OwnerUserId}")
                        .SendAsync("YourAuctionEnded", new
                        {
                            auctionId = auctionId,
                            message = result.WinnerName != null
                                            ? $"Your auction ended. Winner: {result.WinnerName} — ${result.FinalAmount:N2}"
                                            : "Your auction ended with no bids. The comic is available again.",
                            hasBids = result.WinnerUserId.HasValue,
                            finalAmount = result.FinalAmount
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar subasta {AuctionId}. Reintentando en 2 min.", auctionId);
                // Re-agendar para reintento
                _scheduledClosings[auctionId] = DateTime.Now.AddMinutes(2);
            }
        }

        public void ScheduleAuction(int auctionId, DateTime expectedEndDate)
        {
            var localEnd = expectedEndDate.Kind == DateTimeKind.Utc
                ? expectedEndDate.ToLocalTime()
                : expectedEndDate;

            _scheduledClosings[auctionId] = localEnd;
            _logger.LogInformation(
                "Subasta {AuctionId} agregada al monitor. Cierre local: {End:dd/MM/yyyy HH:mm:ss}",
                auctionId, localEnd);
        }

        public void UnscheduleAuction(int auctionId)
        {
            _scheduledClosings.TryRemove(auctionId, out _);
            _logger.LogInformation("Subasta {AuctionId} removida del monitor.", auctionId);
        }

        public IReadOnlyDictionary<int, DateTime> GetScheduledAuctions() => _scheduledClosings;
    }
}