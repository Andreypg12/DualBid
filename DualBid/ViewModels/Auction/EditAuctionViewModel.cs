using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DualBid.ViewModels.Auction
{
    public class EditAuctionViewModel
    {

        public int Id { get; set; }

        [DisplayName("Start Date")]
        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DisplayName("Expected End Date")]
        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ExpectedEndDate { get; set; }

        [DisplayName("Base Price")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "{0} must be greater than 0$")]
        [DataType(DataType.Currency)]
        public decimal BasePrice { get; set; }

        [DisplayName("Minimum Increase")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "{0} must be greater than 0$")]
        [DataType(DataType.Currency)]
        public decimal MinimunIncrease { get; set; }

    }
}
