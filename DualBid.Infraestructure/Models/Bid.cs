using System;
using System.Collections.Generic;

namespace DualBid.Infraestructure.Models;

public partial class Bid
{
    public int Id { get; set; }

    public int AuctionId { get; set; }

    public int UserId { get; set; }

    public decimal AmountOffered { get; set; }

    public DateTime Date { get; set; }

    public virtual ICollection<Auction> Auction { get; set; } = new List<Auction>();

    public virtual Auction AuctionNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
