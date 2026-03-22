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
    public class AuctionDTO : IValidatableObject
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

        // Additional validation method
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validar que StartDate no sea en el pasado
            if (StartDate < DateTime.Now)
            {
                results.Add(new ValidationResult(
                    "Start Date cannot be in the past",
                    new[] { nameof(StartDate) }));
            }

            // Validar que ExpectedEndDate sea después de StartDate
            if (ExpectedEndDate <= StartDate)
            {
                results.Add(new ValidationResult(
                    "Expected End Date must be greater than Start Date",
                    new[] { nameof(ExpectedEndDate) }));
            }

            // Validar que ExpectedEndDate no sea en el pasado
            if (ExpectedEndDate < DateTime.Now)
            {
                results.Add(new ValidationResult(
                    "Expected End Date cannot be in the past",
                    new[] { nameof(ExpectedEndDate) }));
            }

            // Validar que MinimumIncrease sea menor que BasePrice (opcional)
            if (MinimunIncrease >= BasePrice)
            {
                results.Add(new ValidationResult(
                    "Minimum Increase should be less than Base Price",
                    new[] { nameof(MinimunIncrease) }));
            }

            return results;
        }
    }
}
