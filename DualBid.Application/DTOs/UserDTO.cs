using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public class UserDTO
    {
        public int IdUser { get; set; }

        public string Name { get; set; } = string.Empty;

        public string LastNames { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public DateTime Registration_date { get; set; }

        // public Role role { get; set; } = string.Empty;
        // public State state { get; set; } = string.Empty;
    }
}
