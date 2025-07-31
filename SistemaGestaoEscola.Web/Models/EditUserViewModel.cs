using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s]+$", ErrorMessage = "O nome deve conter apenas letras e espaços.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s]+$", ErrorMessage = "O nome deve conter apenas letras e espaços.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public IFormFile? RegistrationPhoto { get; set; }
        public string? RegistrationPhotoPath { get; set; }
    }
}
