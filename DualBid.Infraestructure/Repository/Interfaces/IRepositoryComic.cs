using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DualBid.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryComic
    {
        Task<ICollection<Comic>> ListAsync();
        Task<Comic> FindByIdAsync(int id);
        Task<int> AddAsync(Comic entity, string[] selectedCategorias);

        Task<bool> UpdateAvailabilityAsync(int id, bool availability);
        Task<bool> UpdateAsync(Comic entity,string[] selectedCategorias,List<ImgComic> newImages,int[] imagesToDelete
);
    }
}
