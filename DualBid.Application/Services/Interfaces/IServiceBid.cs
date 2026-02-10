using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Application.DTOs;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceBid
    {
        Task<ICollection<BidDTO>> ListAsync();
        Task<BidDTO?> FindByIdAsync(int id);
    }
}
