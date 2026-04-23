using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceReports
    {
        Task<byte[]> GenerateReportCategoryHistoryAsync(int? categoryId, DateTime? from, DateTime? to);
        Task<byte[]> GenerateFinishedAuctionsReportAsync(DateTime? from, DateTime? to);
    }
}