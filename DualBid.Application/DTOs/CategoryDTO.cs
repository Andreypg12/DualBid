using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public record CategoryDTO
    {
        public int Id {  get; set; }
        public String Description { get; set; } = string.Empty;

    }
}
