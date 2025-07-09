using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Models
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
