using DualBid.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DualBid.Application.DTOs
{
    public record ComicDTO
    {

        //Solo números
        //[RegularExpression(@"^\d+$", ErrorMessage = "Solo números")]
        //Solo Letras
        //[RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Solo letras")]
        //Letras y espacios
        //[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Solo letras y espacios")]


        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 50 characters.")]
        public String Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "Description must have between 20 and 100 characters.")]
        public string Description { get; set; } = string.Empty;


        [Required(ErrorMessage = "Edition Number is required.")]
        [Range(1, 100, ErrorMessage = "Edition must be between 1 and 100.")]
        [Display(Name = "Edition number")]
        public int EditionNumber { get; set; }


        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(17, MinimumLength = 13, ErrorMessage = "ISBN must have between 13 and 17 characters.")]
        public string Isbn { get; set; } = string.Empty;


        [Required(ErrorMessage = "Creation Date is required.")]
        [Display(Name = "Creation date")]
        public DateTime CreationDate { get; set; }


        [Required(ErrorMessage = "Year of publication is required.")]
        [Range(1900, 2100, ErrorMessage = "Enter a valid year. 1900 - 2100")]
        [Display(Name = "Year of publication")]
        public int? YearPublication { get; set; }

        public Publisher Publisher { get; set; } = new();

        public StateConservation StateConservation { get; set; } = new();

        [Display(Name = "Images")]
        public List<ImgComicDTO> ImgComic { get; set; } = new();

        [Display(Name = "Categories")]
        public List<CategoryDTO> Category { get; set; } = new List<CategoryDTO>();

        public List<AuctionDTO> Auction { get; set; } = new();

        public  User? Seller { get; set; }

        public int SellerId { get; set; }

        public bool availability { get; set; }

        public int AuctionCount => Auction.Count();



        //Propiedades auxiliares para validaciones
        [Required(ErrorMessage = "Publisher is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Publisher is required.")]
        public int? PublisherId { get; set; }

        [Required(ErrorMessage = "State of conservation is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "State of conservation is required.")]
        public int? StateConservationId { get; set; }
        // ? permite que sea null
    }
}
