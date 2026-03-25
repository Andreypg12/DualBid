using System.ComponentModel.DataAnnotations;

namespace DualBid.ViewModels.Login
{
    public record ViewModelLogin
    {
        [Display(Name = "User email")]
        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.EmailAddress)]
        public string User { get; set; } = default!;

        [StringLength(15, MinimumLength = 6, ErrorMessage = "Password must be between {2} and {1} characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only letters and numbers are allowed")]
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Password")]
        public string Password { get; set; } = default!;
    }
}
