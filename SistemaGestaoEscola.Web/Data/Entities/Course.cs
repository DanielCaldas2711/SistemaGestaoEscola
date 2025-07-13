using SistemaGestaoEscola.Web.Data.Entities.Interfaces;
using SistemaGestaoEscola.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class Course : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "The field {0} can contain {1} characters length")]
        [Display(Name = "Nome do curso")]
        public string Name { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "The field {0} can contain {1} characters length")]
        [Display(Name = "Área de Ensino")]
        public string Area { get; set; }

        [Required]
        [Display(Name = "Turno")]
        public Shift Shift { get; set; }

        [Display(Name = "Duração")]
        public int TotalTime { get; set; }

        [Required]
        [Display(Name = "Data de início")]
        public DateTime StartingDate { get; set; }

        [Required]
        [Display(Name = "Data de fim")]
        public DateTime EndingDate { get; set; }
    }
}
