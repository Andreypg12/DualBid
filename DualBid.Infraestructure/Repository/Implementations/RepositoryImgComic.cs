using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Infraestructure.Data;
using DualBid.Infraestructure.Models;
using DualBid.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DualBid.Infraestructure.Repository.Implementations
{
    public class RepositoryImgComic : IRepositoryImgComic
    {
        private readonly DualBidContext _context;

        public RepositoryImgComic(DualBidContext Context)
        {
            this._context = Context;
        }

        public async Task<ICollection<ImgComic>> FindByComicIdAsync(int comicId)
        {
            var collection = await _context.Set<ImgComic>()
                .Where(i => i.ComicId == comicId)
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }
    }
}
