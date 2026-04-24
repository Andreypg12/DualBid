using DualBid.Application.DTOs;
using DualBid.Application.Services.Interfaces;
using DualBid.Infraestructure.Reports;
using DualBid.Infraestructure.Repository.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace DualBid.Infraestructure.Services.Implementations;

public class ServiceReports : IServiceReports
{
    private readonly IRepositoryAuction _repositoryAuction;

    public ServiceReports(IRepositoryAuction repositoryAuction)
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


    public async Task<byte[]> GenerateFinishedAuctionsReportAsync(DateTime? from, DateTime? to)
    {
        // Repository devuelve List<Auction> (Entities)
        var auctions = await _repositoryAuction.GetFinishedAuctionsForReportAsync(from, to);

        // El documento PDF recibe directamente las Entities (igual que CategoryHistoryDocument)
        var document = new FinishedAuctionsReportDocument(auctions.ToList(), from, to);
        return document.GeneratePdf();
    }
}