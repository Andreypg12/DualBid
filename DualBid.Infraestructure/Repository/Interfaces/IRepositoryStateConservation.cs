using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryStateConservation
    {
        Task<ICollection<StateConservation>> ListAsync();
        Task<StateConservation> FindByIdAsync(int id);
    }
}
