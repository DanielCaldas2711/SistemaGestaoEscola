using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class User : IdentityUser
    {
        [MaxLength(50, ErrorMessage = "The field {0} can only contain {1} character lenght.")]
        public string FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "The field {0} can only contain {1} character lenght.")]
        public string LastName { get; set; }

        [Display(Name="Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        public string? ProfilePicturePath { get; set; }

        public string DisplayProfilePicturePath =>
            string.IsNullOrEmpty(ProfilePicturePath)
            ? "/images/defaultProfilePicture/default.jpg"
            : ProfilePicturePath;

        public string? RegistrationPhotoPath { get; set; }
    }
}
