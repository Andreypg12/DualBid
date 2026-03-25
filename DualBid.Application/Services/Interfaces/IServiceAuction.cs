using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceAuction
    {
        Task<ICollection<AuctionDTO>> ListAsync();
        Task<AuctionDTO?> FindByIdAsync(int id);
        Task<int> AddAsync(AuctionDTO dto);
        Task UpdateAsync(int id, AuctionDTO dto);
        Task<bool> UpdateStateAsync(int auctionId, int newStateId);
    }
}
