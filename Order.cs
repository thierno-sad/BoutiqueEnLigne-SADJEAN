using System.ComponentModel.DataAnnotations;

namespace BoutiqueEnLigne.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public User? Client { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public string? StripePaymentIntentId { get; set; }
        public string PaymentStatus { get; set; } = "Paid";

        public List<OrderItem> Items { get; set; } = new();
    }
}