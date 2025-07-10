using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Primeiro Nome")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Último Nome")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Função")]
        public string Role { get; set; }
    }
}
