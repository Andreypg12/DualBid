using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Infraestructure.Models;

namespace DualBid.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryAuction
    {
        Task<ICollection<Auction>> ListAsync();
        Task<ICollection<Auction>> ListActiveAsync();
        Task<Auction> FindByIdAsync(int id);
        Task<int> AddAsync(Auction entity);
        Task UpdateAsync(Auction entity);
        Task<bool> UpdateStateAsync(int auctionId, int newStateId);
        Task<bool> EncontrarGanadorAsync(int auctionId);

        //Reportes

        Task<ICollection<Auction>> ListCategoryHistoryAsync(int? categoryId, DateTime? from, DateTime? to);
    }
}
