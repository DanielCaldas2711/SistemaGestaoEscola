using SistemaGestaoEscola.Web.Data.Entities.Interfaces;
using SistemaGestaoEscola.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class Course : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        public int Duration { get; set; }

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; }

        public ICollection<CourseDisciplines> CourseDisciplines { get; set; } = new List<CourseDisciplines>();
    }
}
