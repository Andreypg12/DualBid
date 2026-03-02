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
    public class RepositoryAuction : IRepositoryAuction
    {
        private readonly DualBidContext _context;

        public RepositoryAuction(DualBidContext Context) 
        {
            this._context = Context;
        }

        public async Task<Auction> FindByIdAsync(int id)
        {
            var @object = await _context.Set<Auction>()
                .Where(a => a.Id == id)
                .Include(a => a.State)
                .Include(a => a.Comic)
                .Include(a => a.Comic.ImgComic)
                .Include(a => a.Comic.Category)
                .Include(a => a.Comic.StateConservation)
                .Include(a => a.Bid)
                .Include(a => a.CreatorUser)
                .FirstOrDefaultAsync();

            return @object!;
        }

        public async Task<ICollection<Auction>> ListAsync()
        {
            //Select * from Autor 
            var collection = await _context.Set<Auction>()
                .Include(a => a.State)
                .Include(a => a.Comic)
                .Include(a => a.Comic.ImgComic)
                .Include(a => a.Bid)
                .AsNoTracking()
                .ToListAsync();
            return collection;
        }
    }
}
