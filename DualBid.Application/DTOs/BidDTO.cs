using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualBid.Infraestructure.Models;

namespace DualBid.Application.DTOs
{
    public class BidDTO
    {
        public int id { get; set; }
        public Auction auction { get; set; }
        public User user { get; set; }
        public decimal amount_offerd { get; set; }
        public DateTime date { get; set; }
    }
}
