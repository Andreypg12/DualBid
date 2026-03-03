using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Infraestructure.Models;

namespace DualBid.Application.DTOs
{
    public class AuctionDTO
    {

        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal BasePrice { get; set; }
        public decimal MinimunIncrease { get; set; }
        public AuctionState State { get; set; } = new();
        public Comic Comic { get; set; } = new();
        public User CreatorUser { get; set; } = new();
        public List<Bid> Bids { get; set; } = new();

        public decimal CurrentBid => Bids.Any()
            ? Bids.Max(x => x.AmountOffered)
            : 0m;

        public int NumberOfBids => Bids.Count();


        //public Bid winning_bid { get; set; }

    }
}
