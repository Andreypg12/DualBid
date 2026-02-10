using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public record RoleDTO
    {
        public int id { get; set; }
        public string description { get; set; } = string.Empty;

    }
}
