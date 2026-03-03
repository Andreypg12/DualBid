using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryImgComic
    {
        Task<ICollection<ImgComic>> FindByComicIdAsync(int comicId);
    }
}
