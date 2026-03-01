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
        //La validacion de longitud es unicamente para valores de tipo string
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 50 characters.")]
        public String Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(100, MinimumLength =5, ErrorMessage = "Description must have between 5 and 100 characters.")]
        public string Description { get; set; } = string.Empty;


        [Required(ErrorMessage = "Edition Number is required.")]
        //La validación de rando es unicamente para valores númericos
        [Range(1, 100, ErrorMessage = "Edition must be between 1 and 100.")]
        [Display(Name = "Edition number")]
        public int EditionNumber { get; set; }


        [Required(ErrorMessage = "ISBN is required.")]
        //Esta validación permite solo strings y limita su longitud
        [StringLength(15)]
        public string Isbn { get; set; } = string.Empty;


        [Required(ErrorMessage = "Creation Date is required.")]
        [Display(Name = "Creation date")]
        public DateTime CreationDate { get; set; }


        [Required(ErrorMessage = "Year of publication is required.")]
        [Display(Name = "Year of publication")]
        public int YearPublication { get; set; }


        [Required(ErrorMessage = "Publisher is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Publisher is required.")]
        public Publisher Publisher { get; set; } = new();


        [Required(ErrorMessage = "State of Conservation is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "State of conservation is required.")]
        public StateConservation StateConservation { get; set; } = new();


        
        [Display(Name = "Images")]
        public List<ImgComicDTO> ImgComic { get; set; } = new();



        [Display(Name = "Categories")]
        public List<CategoryDTO> Category { get; set; } = new List<CategoryDTO>();


        public List<AuctionDTO> Auction { get; set; } = new();


        //[Required(ErrorMessage = "A user (Seller) is required.")]
        //public virtual User Seller { get; set; } = null!;
        public  User? Seller { get; set; }


        public int SellerId { get; set; }
    }
}
