using System;
using System.Collections.Generic;

namespace DualBid.Infraestructure.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LastNames { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public int StateId { get; set; }

    public DateTime RegistrationDate { get; set; }

    public virtual ICollection<Auction> Auction { get; set; } = new List<Auction>();

    public virtual ICollection<Bid> Bid { get; set; } = new List<Bid>();

    public virtual ICollection<Comic> Comic { get; set; } = new List<Comic>();

    public virtual Role Role { get; set; } = null!;

    public virtual UserStatus State { get; set; } = null!;
}
