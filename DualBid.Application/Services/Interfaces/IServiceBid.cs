using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceBid
    {
        Task<ICollection<BidDTO>> AuctionBiddingHistory(int auctionId);
        Task<ICollection<BidDTO>> ListAsync();
        Task<BidDTO?> FindByIdAsync(int id);
        Task<int> AddAsync(BidDTO dto);
    }
}
