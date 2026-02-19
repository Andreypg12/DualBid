using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryPublisher
    {
        Task<ICollection<Publisher>> ListAsync();
        Task<Publisher> FindByIdAsync(int id);
    }
}
