namespace DualBid.Models
{
    public class ErrorMiddlewareViewModel
    {
        public string Path { get; set; } = string.Empty;
        public List<string> ListMessages { get; set; } = new();
        public string IdEvent { get; set; } = string.Empty;
    }
}
