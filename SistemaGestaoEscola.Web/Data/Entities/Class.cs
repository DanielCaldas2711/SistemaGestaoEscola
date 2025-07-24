using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SistemaGestaoEscola.Web.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class Class : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public string Shift { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ValidateNever]
        public Course Course { get; set; }

        [Required]
        public DateTime StartingDate { get; set; }

        [Required]
        public DateTime EndingDate { get; set; }

        public IEnumerable<ClassStudents> Students { get; set; } = Enumerable.Empty<ClassStudents>();

        public IEnumerable<ClassProfessors> Professors { get; set; } = Enumerable.Empty<ClassProfessors>();
    }
}
