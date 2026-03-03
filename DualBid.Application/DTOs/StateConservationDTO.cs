using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public record StateConservationDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
