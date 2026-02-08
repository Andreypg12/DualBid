using System;
using System.Collections.Generic;

namespace DualBid.Infraestructure.Models;

public partial class Auction
{
    public int Id { get; set; }

    public int ComicId { get; set; }

    public int CreatorUserId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpectedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public decimal BasePrice { get; set; }

    public decimal MinimunIncrease { get; set; }

    public bool State { get; set; }

    public int? WinningBidId { get; set; }

    public virtual ICollection<Bid> Bid { get; set; } = new List<Bid>();

    public virtual Comic Comic { get; set; } = null!;

    public virtual User CreatorUser { get; set; } = null!;

    public virtual Bid? WinningBid { get; set; }
}
