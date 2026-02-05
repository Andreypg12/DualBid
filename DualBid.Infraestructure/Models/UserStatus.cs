using System;
using System.Collections.Generic;

namespace DualBid.Infraestructure.Models;

public partial class UserStatus
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<User> User { get; set; } = new List<User>();
}
