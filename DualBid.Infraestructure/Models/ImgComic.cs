using System;
using System.Collections.Generic;

namespace DualBid.Infraestructure.Models;

public partial class ImgComic
{
    public int Id { get; set; }

    public byte[] Img { get; set; } = null!;

    public int ComicId { get; set; }

    public virtual Comic Comic { get; set; } = null!;
}
