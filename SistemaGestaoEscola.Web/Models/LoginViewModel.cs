using Microsoft.AspNetCore.Antiforgery;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "É necessário preencher o email")]
        [EmailAddress(ErrorMessage = "Preencha o formato correto do email.")]
        [MaxLength(320)]
        public string UserName { get; set; }

        [Required(ErrorMessage ="A palavra-passe é obrigatória.")]
        [DataType(DataType.Password)]
        [MaxLength(20, ErrorMessage = "A passe pode ter até 20 caracteres")]
        [MinLength(8, ErrorMessage = "É preciso de no mínimo 8 caracteres")]
        public string Password { get; set; }

        public bool RemenberMe { get; set; }
    }
}
