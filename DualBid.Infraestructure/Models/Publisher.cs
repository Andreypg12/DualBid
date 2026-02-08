using System;
using System.Collections.Generic;

namespace DualBid.Infraestructure.Models;

public partial class Publisher
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<Comic> Comic { get; set; } = new List<Comic>();
}
