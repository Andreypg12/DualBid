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

        public int id { get; set; }
        public Comic comic { get; set; }
        public User user_Creator { get; set; }
        public DateTime startDate { get; set; }
        public DateTime expected_end_date { get; set; }
        public DateTime actual_end_date { get; set; }
        public decimal base_Price { get; set; }
        public decimal minimun_increase { get; set; }
        public AuctionState state { get; set; }
        public Bid winning_bid { get; set; }

    }
}
