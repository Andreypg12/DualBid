using DualBid.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.Services.Interfaces
{
    public interface IServiceComic
    {
        Task<ICollection<ComicDTO>> ListAsync();
        Task<ComicDTO?> FindByIdAsync(int id);
       
        Task<int> AddAsync(ComicDTO dto, string[] selectedCategorias);
    }
}
