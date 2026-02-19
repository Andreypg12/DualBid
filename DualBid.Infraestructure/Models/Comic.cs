using System;
using System.Collections.Generic;

namespace DualBid.Infraestructure.Models;

public partial class Comic
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public string Title { get; set; } = null!;

    public int EditionNumber { get; set; }

    public string? Isbn { get; set; }

    public DateTime CreationDate { get; set; }

    public int YearPublication { get; set; }

    public string Description { get; set; } = null!;

    public int PublisherId { get; set; }

    public int StateConservationId { get; set; }

    public int ConditionId { get; set; }

    public virtual ICollection<Auction> Auction { get; set; } = new List<Auction>();

    public virtual ICollection<ImgComic> ImgComic { get; set; } = new List<ImgComic>();

    public virtual Publisher Publisher { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;

    public virtual StateConservation StateConservation { get; set; } = null!;

    public virtual ICollection<Category> Category { get; set; } = new List<Category>();
}
