using DualBid.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceCategory
    {
        Task<ICollection<CategoryDTO>> ListAsync();
        Task<CategoryDTO?> FindByIdAsync(int id);
    }
}
