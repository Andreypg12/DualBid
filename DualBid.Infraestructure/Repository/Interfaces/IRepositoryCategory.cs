using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Infraestructure.Models;

namespace DualBid.Infraestructure.Repository.Interfaces
{
     public interface IRepositoryCategory
    {
        Task<ICollection<Category>> ListAsync();
        Task<Category> FindByIdAsync(int id);
    }
}
