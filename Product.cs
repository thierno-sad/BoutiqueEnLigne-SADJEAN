using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoutiqueEnLigne.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]

        public string Title { get; set; }
        [Required]

        public string Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]

        public decimal Price { get; set; }

        public string Category { get; set; }

        public string ImageUrl { get; set; }

        public int SellerId {  get; set; }

        public User? Seller { get; set; }
    }
}
