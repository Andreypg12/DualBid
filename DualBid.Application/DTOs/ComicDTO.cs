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
        public int Id { get; set; }

        public String Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Display(Name = "Edition number")]
        public int EditionNumber { get; set; }
        public string Isbn { get; set; } = string.Empty;

        [Display(Name = "Creation date")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Year of publication")]
        public int YearPublication { get; set; }

        public Publisher Publisher { get; set; } = new();

        public StateConservation StateConservation { get; set; } = new();

        [Display(Name = "Images")]
        public List<ImgComicDTO> ImgComic { get; set; } = new();

        [Display(Name = "Categories")]
        public List<CategoryDTO> Category { get; set; } = new();

        public List<AuctionDTO> Auction { get; set; } = new();
    }
}
