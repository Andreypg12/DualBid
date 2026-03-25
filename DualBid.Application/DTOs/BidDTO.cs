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
        public int Id { get; set; }
        public decimal AmountOffered { get; set; }
        public DateTime Date { get; set; }
        public UserDTO User { get; set; } = new();
        public int UserId { get; set; }
        public int AuctionId { get; set; }
    }
}
