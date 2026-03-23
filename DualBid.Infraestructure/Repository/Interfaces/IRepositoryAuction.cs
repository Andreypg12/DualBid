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
        Task<Auction> FindByIdAsync(int id);
        Task<int> AddAsync(Auction entity);
        Task UpdateAsync(Auction entity);
    }
}
