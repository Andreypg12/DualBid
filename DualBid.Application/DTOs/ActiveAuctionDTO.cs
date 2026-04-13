namespace DualBid.Application.DTOs
{
    /// <summary>
    /// Proyección mínima de una subasta activa. Solo los campos
    /// necesarios para el monitor (evita traer todo el objeto).
    /// </summary>
    public class ActiveAuctionDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Corresponde a ExpectedEndDate en tu modelo/DTO.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Corresponde a CreatorUserId en tu modelo/DTO.
        /// </summary>
        public int? OwnerUserId { get; set; }
    }
}
