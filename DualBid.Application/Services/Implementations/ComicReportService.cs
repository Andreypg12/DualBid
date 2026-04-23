using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Reports;
using DualBid.Infraestructure.Repository.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace DualBid.Infraestructure.Services.Implementations;

public class ComicReportService : IComicReporteService
{
    private readonly IRepositoryAuction _repositoryAuction;

    public ComicReportService(IRepositoryAuction repositoryAuction)
    {
        _repositoryAuction = repositoryAuction;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateReportCategoryHistoryAsync(int? categoryId, DateTime? from, DateTime? to)
    {
        var auctions = await _repositoryAuction.ListCategoryHistoryAsync(categoryId, from, to);
        var document = new CategoryHistoryDocument(auctions.ToList(), categoryId, from, to);
        return document.GeneratePdf();
    }
}