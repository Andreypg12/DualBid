// DualBid.Infraestructure/Reports/CategoryHistoryDocument.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DualBid.Infraestructure.Models;

namespace DualBid.Infraestructure.Reports;

public class CategoryHistoryDocument : IDocument
{
    private readonly List<Auction> _auctions;
    private readonly int? _categoryId;
    private readonly DateTime? _from;
    private readonly DateTime? _to;



    //Colores para el estilo
    private static readonly string BgDark = "#2b1e3e";
    private static readonly string CosmicBlue = "#0091B9";
    private static readonly string Lavender = "#95D1DC";
    private static readonly string Silver = "#FFFFFF";
    private static readonly string Accent1 = "#FFD500";
    private static readonly string Accent2 = "#B0BEC5";
    private static readonly string RowAlt = "#f3f0f8";
    private static readonly string HeaderBg = "#2b1e3e";
    private static readonly string TextDark = "#2b1e3e";

    private static readonly string[] ChartColors = {
    "#7986CB", // índigo suave
    "#4DB6AC", // teal suave
    "#FFD54F", // amarillo suave
    "#A5D6A7", // verde suave
    "#EF9A9A", // rojo suave
    "#90CAF9", // azul suave
    "#CE93D8", // lila suave
    "#FFCC80", // naranja suave
    "#80CBC4", // menta suave
    "#B0BEC5"  // gris azulado
};



    public CategoryHistoryDocument(List<Auction> auctions, int? categoryId, DateTime? from, DateTime? to)
    {
        _auctions = auctions;
        _categoryId = categoryId;
        _from = from;
        _to = to;
    }

    //Usa valores por defecto es algo para que funcione QuestPDF
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;


    // Composición del documento PDF usando QuestPDF
    public void Compose(IDocumentContainer container)
    {

        // Agrupamos las subastas por categoría usando Description y contamos totales y finalizadas
        var byCategory = _auctions
            .SelectMany(a => a.Comic.Category.Select(c => new
            {
                CategoryName = c.Description,
                CategoryId = c.Id,
                Auction = a
            }))
            .Where(x => !_categoryId.HasValue || x.CategoryId == _categoryId.Value)
            .GroupBy(x => x.CategoryName)
            .Select(g => new
            {
                Category = g.Key,
                TotalAuctions = g.Select(x => x.Auction.Id).Distinct().Count(),
                AuctionsFinished = g.Select(x => x.Auction)
                                    .DistinctBy(a => a.Id)
                                    .Count(a => a.StateId == 3)
            })
            .OrderByDescending(x => x.TotalAuctions)
            .ToList();

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
                col.Item().Text("Auction History by Category")
                    .FontSize(11).FontColor("#95D1DC");

                col.Item().PaddingTop(6).Row(row =>
                {
                    if (_categoryId.HasValue)
                    {
                        var catName = _auctions
                            .SelectMany(a => a.Comic.Category)
                            .FirstOrDefault(c => c.Id == _categoryId)?.Description ?? "N/A";
                        row.AutoItem().Background(Accent1).Padding(4).PaddingHorizontal(8)
                            .Text($"Category: {catName}").FontSize(8).Bold().FontColor(Silver);
                        row.ConstantItem(6);
                    }
                    if (_from.HasValue && _to.HasValue)
                    {
                        row.AutoItem().Background(Accent2).Padding(4).PaddingHorizontal(8)
                            .Text($"{_from:dd/MM/yyyy} → {_to:dd/MM/yyyy}").FontSize(8).Bold().FontColor(HeaderBg);
                    }
                    if (!_categoryId.HasValue && !_from.HasValue)
                    {
                        row.AutoItem().Background(Lavender).Padding(4).PaddingHorizontal(8)
                            .Text("All periods and categories").FontSize(8).Bold().FontColor(HeaderBg);
                    }
                });

                col.Item().PaddingTop(12).Row(row =>
                {
                    row.RelativeItem(3).Height(3).Background(Accent1);
                    row.RelativeItem(2).Height(3).Background(CosmicBlue);
                    row.RelativeItem(1).Height(3).Background(Accent2);
                });
            });

            // CONTENT 
            page.Content().PaddingHorizontal(30).PaddingTop(20).Column(col =>
            {
                // Table 
                col.Item().Text("Distribution by Category")
                    .Bold().FontSize(13).FontColor(TextDark);

                //SUMA DE TODAS LAS SUBASTAS
                // Contar subastas únicas, no la suma de categorías
                var totalShown = _auctions.Select(a => a.Id).Distinct().Count();
                var totalFinished = _auctions.Where(a => a.StateId == 3).Select(a => a.Id).Distinct().Count();


                //SUMA DE TODAS LAS SUBASTAS
                col.Item().PaddingTop(6).PaddingBottom(4).Row(row =>
                {
                    row.AutoItem().Background(HeaderBg).Padding(6).PaddingHorizontal(12).Column(c =>
                    {
                        c.Item().Text("Total Auctions").FontSize(7).FontColor("#95D1DC");
                        c.Item().Text(totalShown.ToString()).Bold().FontSize(16).FontColor(Silver);
                    });
                });

                col.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        foreach (var h in new[] { "Category", "Total Auctions", "Finished Auctions" })
                            header.Cell()
                                .Background(HeaderBg).Padding(8)
                                .Text(h).Bold().FontSize(9).FontColor(Silver);
                    });

                    bool alt = false;
                    foreach (var cat in byCategory)
                    {
                        var bg = alt ? RowAlt : Silver;
                        alt = !alt;

                        table.Cell().Background(bg).Padding(7)
                            .Text(cat.Category).FontSize(9).FontColor(TextDark);
                        table.Cell().Background(bg).Padding(7).AlignCenter()
                            .Text(cat.TotalAuctions.ToString()).Bold().FontSize(9).FontColor(BgDark);
                        table.Cell().Background(bg).Padding(7).AlignCenter()
                            .Text(cat.AuctionsFinished.ToString()).Bold().FontSize(9).FontColor(Accent1);
                    }
                });

                // Page break
                col.Item().Element(x => x.PageBreak());

                //  Bar chart 
                col.Item().Text("Chart — Auctions by Category")
                    .Bold().FontSize(13).FontColor(TextDark);

                col.Item().PaddingTop(16).Background(RowAlt).Padding(16).Column(chartCol =>
                {
                    var maxBar = byCategory.Any() ? byCategory.Max(x => x.TotalAuctions) : 1;
                    var chartHeight = 150f;
                    var pageWidth = 455f;
                    var totalBars = byCategory.Count;
                    var barWidth = Math.Max((int)(pageWidth / (totalBars * 1.6f)), 10);
                    var gap = Math.Max((int)(barWidth * 0.3f), 3);

                    // Bars — all start from the bottom
                    chartCol.Item().Height(chartHeight).Row(barsRow =>
                    {
                        int colorIndex = 0;
                        foreach (var cat in byCategory)
                        {
                            var color = ChartColors[colorIndex % ChartColors.Length];
                            var pct = maxBar > 0 ? (float)cat.TotalAuctions / maxBar : 0f;
                            var barH = Math.Max(pct * (chartHeight - 16f), 2f);
                            var emptyH = Math.Max(chartHeight - barH - 16f, 0.001f);

                            barsRow.ConstantItem(barWidth).Column(barCol =>
                            {
                                barCol.Item().Height(emptyH);
                                barCol.Item().Height(16).AlignCenter().AlignBottom()
                                    .Text(cat.TotalAuctions.ToString())
                                    .FontSize(6).Bold().FontColor(TextDark);
                                barCol.Item().Height(barH).Background(color);
                            });

                            barsRow.ConstantItem(gap);
                            colorIndex++;
                        }
                    });

                    // Base line
                    chartCol.Item().LineHorizontal(1.5f).LineColor(HeaderBg);

                    // Labels below
                    chartCol.Item().PaddingTop(4).Row(labelsRow =>
                    {
                        foreach (var cat in byCategory)
                        {
                            var label = cat.Category.Length > 5 ? cat.Category[..5] + "." : cat.Category;
                            labelsRow.ConstantItem(barWidth).AlignCenter()
                                .Text(label).FontSize(5).FontColor(TextDark);
                            labelsRow.ConstantItem(gap);
                        }
                    });

                    // Legend in 2 columns
                    chartCol.Item().PaddingTop(12).LineHorizontal(0.5f).LineColor(Lavender);
                    chartCol.Item().PaddingTop(8).Column(legendCol =>
                    {
                        var legendRows = byCategory
                            .Select((cat, i) => new { cat, color = ChartColors[i % ChartColors.Length] })
                            .ToList();

                        for (int i = 0; i < legendRows.Count; i += 2)
                        {
                            legendCol.Item().PaddingBottom(4).Row(r =>
                            {
                                r.ConstantItem(12).Height(10).Background(legendRows[i].color);
                                r.ConstantItem(4);
                                r.RelativeItem().AlignMiddle()
                                    .Text($"{legendRows[i].cat.Category} ({legendRows[i].cat.TotalAuctions})")
                                    .FontSize(8).FontColor(TextDark);

                                if (i + 1 < legendRows.Count)
                                {
                                    r.ConstantItem(12).Height(10).Background(legendRows[i + 1].color);
                                    r.ConstantItem(4);
                                    r.RelativeItem().AlignMiddle()
                                        .Text($"{legendRows[i + 1].cat.Category} ({legendRows[i + 1].cat.TotalAuctions})")
                                        .FontSize(8).FontColor(TextDark);
                                }
                            });
                        }
                    });
                });
            });

            // FOOTER
            page.Footer().Background(HeaderBg).PaddingHorizontal(30).PaddingVertical(10).Row(row =>
            {
                row.RelativeItem()
                    .Text($"DualBid  •  Report generated on {DateTime.Now:MM/dd/yyyy HH:mm}")
                    .FontSize(8).FontColor("#95D1DC");
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(8).FontColor("#95D1DC");
                    text.CurrentPageNumber().FontSize(8).FontColor(Silver);
                    text.Span(" of ").FontSize(8).FontColor("#95D1DC");
                    text.TotalPages().FontSize(8).FontColor(Silver);
                });
            });
        });
    }
}