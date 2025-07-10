using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Models
{
    public class EditProfileViewModel
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "Primeiro Nome")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Último Nome")]
        public string LastName { get; set; }

        [Display(Name = "Foto de Perfil")]
        public IFormFile? ProfilePicture { get; set; }

        public string? ExistingPicturePath { get; set; }
    }
}
