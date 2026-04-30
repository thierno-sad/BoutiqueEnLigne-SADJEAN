using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BoutiqueEnLigne.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]

        public string Fullname {  get; set; }
        [Required]
        [EmailAddress]

        public string Email { get; set; }
        [Required]

        public string PasswordHash { get; set; }
        [Required]

        public string Role {  get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
