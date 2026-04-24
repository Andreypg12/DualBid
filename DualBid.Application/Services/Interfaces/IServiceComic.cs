using DualBid.Application.DTOs;
using DualBid.Infraestructure.Models;
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
        Task<bool> UpdateAvailabilityAsync(int id, bool availability);
        Task<bool> UpdateAsync(ComicDTO dto, string[] selectedCategorias,List<ImgComicDTO> newImages,int[] imagesToDelete);
        Task<ICollection<ComicDTO>> ListComicsForAuctionByUserAsync(int userId);
        Task<ICollection<ComicDTO>> ListByUserAsync(int userId);
    }
}
