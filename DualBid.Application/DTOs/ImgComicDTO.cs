using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualBid.Application.DTOs
{
    public record ImgComicDTO
    {
        public int Id { get; set; }

        public byte[] Img { get; set; } = null!;
    }
}
