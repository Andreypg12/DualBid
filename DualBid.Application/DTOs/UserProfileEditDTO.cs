using System.ComponentModel.DataAnnotations;

namespace DualBid.Application.DTOs
{
    public record UserProfileEditDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last names are required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last names must be between 2 and 100 characters")]
        public string LastNames { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmNewPassword { get; set; }

        // Solo lectura - para mostrar
        public string RoleDescription { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string StateDescription { get; set; } = string.Empty;
    }
}