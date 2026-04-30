using System.ComponentModel.DataAnnotations;
namespace BoutiqueEnLigne.ViewModels
{
    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        public string Fullname { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
