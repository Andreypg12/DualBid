using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public class AuctionDTO
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


        public DateTime? ActualEndDate { get; set; }
        public string ActualEndDateFormat
        {
            get => ActualEndDate.HasValue
                ? ActualEndDate.Value.ToString("dd MMM yyyy 'at' HH:mm")
                : string.Empty;
        }

        [DisplayName("Base Price")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "{0} must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal BasePrice { get; set; }

        [DisplayName("Minimum Increase")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "{0} must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal MinimunIncrease { get; set; }

        [DisplayName("State id")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(1, 4, ErrorMessage = "{0} must be greater than 0")]
        [DataType(DataType.Currency)]
        public int StateId { get; set; }

        public AuctionStateDTO State { get; set; } = new();

        // Comic properties
        [DisplayName("Comic")]
        [Required(ErrorMessage = "Please select a {0}")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid {0}")]
        public int ComicId { get; set; }

        public ComicDTO Comic { get; set; } = new();

        public int CreatorUserId { get; set; }

        public UserDTO CreatorUser { get; set; } = new();

        // Bids
        public List<BidDTO> Bids { get; set; } = new();

        // Computed properties
        public BidDTO CurrentBid => Bids
            .OrderByDescending(x => x.AmountOffered)
            .FirstOrDefault() ?? new BidDTO();

        public int NumberOfBids => Bids?.Count ?? 0;


        // @* Editado por ALE *@
        //Esto es para saber y mostrar en pantalla el ganador de la subasta y quien ganó.
        public int? WinningBidId { get; set; }
        public BidDTO? WinningBid { get; set; }

        public int? WinnerUserId => WinningBid?.UserId;
        public decimal? FinalAmount => WinningBid?.AmountOffered;
    }
}
