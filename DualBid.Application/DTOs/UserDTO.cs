using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public record UserDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string LastNames { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }

        [Display(Name = "Role")]
        public RoleDTO Role { get; set; } = new();

        [Display(Name = "State")]
        public UserStateDTO State { get; set; } = new();

        public string CompleteName => $"{Name} {LastNames}";

    }
}
