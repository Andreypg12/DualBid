// ViewModels/Register/ViewModelRegister.cs
using System.ComponentModel.DataAnnotations;

namespace DualBid.ViewModels.Register
{
    public class ViewModelRegister
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Last names are required")]
        [Display(Name = "Last Names")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last names must be between 2 and 100 characters")]
        public string LastNames { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Please select an account type")]
        [Range(2, 3, ErrorMessage = "Invalid account type")]
        [Display(Name = "Account Type")]
        public int RoleId { get; set; }
    }
}