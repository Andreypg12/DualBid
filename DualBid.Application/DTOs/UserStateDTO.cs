using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public record UserStateDTO
    {
        public string id { get; init; }
        public string description { get; init; }
    }
}
