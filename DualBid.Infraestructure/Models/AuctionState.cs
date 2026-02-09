using System;
using System.Collections.Generic;

namespace DualBid.Infraestructure.Models;

public partial class AuctionState
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<Auction> Auction { get; set; } = new List<Auction>();
}
