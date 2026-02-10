using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Infraestructure.Models;

namespace DualBid.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryBid
    {
        Task<ICollection<Bid>> ListAsync();
        Task<Bid> FindByIdAsync(int id);
    }
}
