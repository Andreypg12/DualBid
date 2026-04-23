// DualBid.Infraestructure/Reports/FinishedAuctionsReportDocument.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DualBid.Infraestructure.Models;

namespace DualBid.Infraestructure.Reports;

public class FinishedAuctionsReportDocument : IDocument
{
    private readonly List<Auction> _auctions;
    private readonly DateTime? _from;
    private readonly DateTime? _to;

    private static readonly string BgDark = "#2b1e3e";
    private static readonly string CosmicBlue = "#0091B9";
    private static readonly string Lavender = "#95D1DC";
    private static readonly string Silver = "#FFFFFF";
    private static readonly string Accent1 = "#FFD500";
    private static readonly string SuccessGreen = "#28a745";
    private static readonly string WarningYellow = "#ffc107";
    private static readonly string DangerRed = "#dc3545";
    private static readonly string RowAlt = "#f3f0f8";
    private static readonly string HeaderBg = "#2b1e3e";
    private static readonly string TextDark = "#2b1e3e";
    private static readonly string TextMuted = "#6c757d";

    public FinishedAuctionsReportDocument(List<Auction> auctions, DateTime? from, DateTime? to)
    {
        _auctions = auctions;
        _from = from;
        _to = to;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginHorizontal(0);
            page.MarginVertical(0);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("DejaVu Sans"));

            // HEADER
            page.Header().Background(HeaderBg).Padding(30).Column(col =>
            {
                col.Item().Text("DualBid")
                    .Bold().FontSize(26).FontColor(Silver);
                col.Item().Text("Finished Auctions Report")
                    .FontSize(11).FontColor(Lavender);

                col.Item().PaddingTop(6).Row(row =>
                {
                    if (_from.HasValue && _to.HasValue)
                    {
                        row.AutoItem().Background(Accent1).Padding(4).PaddingHorizontal(8)
                            .Text($"{_from:dd/MM/yyyy} → {_to:dd/MM/yyyy}").FontSize(8).Bold().FontColor(HeaderBg);
                    }
                    else if (_from.HasValue)
                    {
                        row.AutoItem().Background(Accent1).Padding(4).PaddingHorizontal(8)
                            .Text($"From: {_from:dd/MM/yyyy}").FontSize(8).Bold().FontColor(HeaderBg);
                    }
                    else if (_to.HasValue)
                    {
                        row.AutoItem().Background(Accent1).Padding(4).PaddingHorizontal(8)
                            .Text($"Until: {_to:dd/MM/yyyy}").FontSize(8).Bold().FontColor(HeaderBg);
                    }
                    else
                    {
                        row.AutoItem().Background(Lavender).Padding(4).PaddingHorizontal(8)
                            .Text("All finished auctions").FontSize(8).Bold().FontColor(HeaderBg);
                    }
                });

                col.Item().PaddingTop(12).Row(row =>
                {
                    row.RelativeItem(3).Height(3).Background(Accent1);
                    row.RelativeItem(2).Height(3).Background(CosmicBlue);
                    row.RelativeItem(1).Height(3).Background(Lavender);
                });
            });

            // CONTENT
            page.Content().PaddingHorizontal(30).PaddingTop(20).Column(col =>
            {
                // Resumen de estadísticas
                col.Item().Text("Summary")
                    .Bold().FontSize(13).FontColor(TextDark);

                var totalAuctions = _auctions.Count;
                var auctionsWithWinner = _auctions.Count(a => a.WinningBidId.HasValue || a.Bid.Any());
                var auctionsPaid = _auctions.Count(a => a.StateId == 3);
                var totalAmount = _auctions
                    .Where(a => a.WinningBidId.HasValue)
                    .Sum(a => a.WinningBid?.AmountOffered ?? 0);

                col.Item().PaddingTop(6).PaddingBottom(16).Row(row =>
                {
                    row.AutoItem().Background(HeaderBg).Padding(6).PaddingHorizontal(12).Column(c =>
                    {
                        c.Item().Text("Total finished Auctions").FontSize(7).FontColor(Lavender);
                        c.Item().Text(totalAuctions.ToString()).Bold().FontSize(16).FontColor(Silver);
                    });
                    row.ConstantItem(8);
                    row.AutoItem().Background(SuccessGreen).Padding(6).PaddingHorizontal(12).Column(c =>
                    {
                        c.Item().Text("With Winner").FontSize(7).FontColor(Silver);
                        c.Item().Text(auctionsWithWinner.ToString()).Bold().FontSize(16).FontColor(Silver);
                    });
                    row.ConstantItem(8);
                    row.AutoItem().Background(Accent1).Padding(6).PaddingHorizontal(12).Column(c =>
                    {
                        c.Item().Text("Completed (Paid)").FontSize(7).FontColor(HeaderBg);
                        c.Item().Text(auctionsPaid.ToString()).Bold().FontSize(16).FontColor(HeaderBg);
                    });
                    row.ConstantItem(8);
                    row.AutoItem().Background(CosmicBlue).Padding(6).PaddingHorizontal(12).Column(c =>
                    {
                        c.Item().Text("Total Amount").FontSize(7).FontColor(Silver);
                        c.Item().Text($"${totalAmount:N0}").Bold().FontSize(16).FontColor(Silver);
                    });
                });

                // Tabla de subastas finalizadas
                col.Item().Text("Finished Auctions List")
                    .Bold().FontSize(13).FontColor(TextDark);

                col.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(35);   // ID Subasta
                        cols.ConstantColumn(35);   // ID Comic
                        cols.RelativeColumn(2);     // Comic
                        cols.RelativeColumn(2);     // Creador (nombre + email)
                        cols.RelativeColumn(2);     // Ganador (nombre + email)
                        cols.ConstantColumn(60);    // Monto
                        cols.RelativeColumn(2);     // Fecha Cierre
                        cols.ConstantColumn(65);    // Estado
                    });

                    table.Header(header =>
                    {
                        var headers = new[] {
                            "Auction ID", "Comic ID", "Comic", "Creator",
                            "Winner", "Final Amount", "Close Date", "Status"
                        };

                        foreach (var h in headers)
                            header.Cell()
                                .Background(HeaderBg).Padding(6)
                                .Text(h).Bold().FontSize(8).FontColor(Silver);
                    });

                    bool alt = false;
                    foreach (var auction in _auctions)
                    {
                        var winningBid = auction.WinningBidId.HasValue
                            ? auction.Bid.FirstOrDefault(b => b.Id == auction.WinningBidId)
                            : auction.Bid.OrderByDescending(b => b.AmountOffered).FirstOrDefault();

                        var hasWinner = winningBid != null;
                        var closeDate = auction.ActualEndDate ?? auction.ExpectedEndDate;
                        var isCompleted = auction.StateId == 3;
                        var isClosedManually = auction.StateId == 4;

                        string paymentStatus;
                        string statusColor;

                        if (isCompleted)
                        {
                            paymentStatus = "Paid";
                            statusColor = SuccessGreen;
                        }
                        else if (isClosedManually && hasWinner)
                        {
                            paymentStatus = "Unpaid";
                            statusColor = WarningYellow;
                        }
                        else if (isClosedManually)
                        {
                            paymentStatus = "Cancelled";
                            statusColor = DangerRed;
                        }
                        else
                        {
                            paymentStatus = "Unknow";
                            statusColor = TextMuted;
                        }

                        var bg = alt ? RowAlt : Silver;
                        alt = !alt;

                        // Auction ID
                        table.Cell().Background(bg).Padding(5)
                            .Text(auction.Id.ToString()).FontSize(8).FontColor(TextDark);

                        // Comic ID
                        table.Cell().Background(bg).Padding(5)
                            .Text(auction.ComicId.ToString()).FontSize(8).FontColor(TextDark);

                        // Comic Title
                        table.Cell().Background(bg).Padding(5)
                            .Text(auction.Comic.Title).FontSize(8).FontColor(TextDark);

                        // Creator (Nombre + Email secundario)
                        table.Cell().Background(bg).Padding(5).Column(creatorCol =>
                        {
                            creatorCol.Item().Text($"{auction.CreatorUser.Name} {auction.CreatorUser.LastNames}")
                                .FontSize(8).FontColor(TextDark);
                            creatorCol.Item().Text(auction.CreatorUser.Email)
                                .FontSize(7).FontColor(TextMuted).Italic();
                        });

                        // Winner (Nombre + Email secundario)
                        table.Cell().Background(bg).Padding(5).Column(winnerCol =>
                        {
                            if (hasWinner)
                            {
                                winnerCol.Item().Text($"{winningBid!.User.Name} {winningBid.User.LastNames}")
                                    .FontSize(8).FontColor(TextDark);
                                winnerCol.Item().Text(winningBid!.User.Email)
                                    .FontSize(7).FontColor(TextMuted).Italic();
                            }
                            else
                            {
                                winnerCol.Item().Text("---")
                                    .FontSize(8).FontColor(TextMuted);
                            }
                        });

                        // Final Amount
                        table.Cell().Background(bg).Padding(5).AlignRight()
                            .Text(hasWinner ? $"${winningBid!.AmountOffered:N2}" : "$0")
                            .Bold().FontSize(8).FontColor(TextDark);

                        // Close Date
                        table.Cell().Background(bg).Padding(5)
                            .Text(closeDate.ToString("dd/MM/yyyy HH:mm")).FontSize(8).FontColor(TextDark);

                        // Status
                        table.Cell().Background(bg).Padding(5)
                            .Text(paymentStatus).FontSize(8).FontColor(statusColor).Bold();
                    }
                });

                // Si no hay datos
                if (!_auctions.Any())
                {
                    col.Item().PaddingTop(30).AlignCenter()
                        .Text("No finished auctions found for the selected period.")
                        .FontSize(11).Italic().FontColor(Accent1);
                }
            });

            // FOOTER
            page.Footer().Background(HeaderBg).PaddingHorizontal(30).PaddingVertical(10).Row(row =>
            {
                row.RelativeItem()
                    .Text($"DualBid  •  Report generated on {DateTime.Now:MM/dd/yyyy HH:mm}")
                    .FontSize(8).FontColor(Lavender);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(8).FontColor(Lavender);
                    text.CurrentPageNumber().FontSize(8).FontColor(Silver);
                    text.Span(" of ").FontSize(8).FontColor(Lavender);
                    text.TotalPages().FontSize(8).FontColor(Silver);
                });
            });
        });
    }
}