namespace BoutiqueEnLigne.DTOs
{
    public class DummyProductsResponse
    {
        public List<DummyProductDto> Products { get; set; } = new();
    }

    public class DummyProductDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public string Thumbnail { get; set; } = "";
    }
}