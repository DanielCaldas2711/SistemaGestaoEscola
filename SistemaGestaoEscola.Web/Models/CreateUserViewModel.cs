using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Primeiro Nome")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s]+$", ErrorMessage = "O nome deve conter apenas letras e espaços.")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Último Nome")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s]+$", ErrorMessage = "O nome deve conter apenas letras e espaços.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Função")]
        public string Role { get; set; }

        public IFormFile? RegistrationPhoto { get; set; }
    }
}
