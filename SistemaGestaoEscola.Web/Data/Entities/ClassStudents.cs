using SistemaGestaoEscola.Web.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class ClassStudents : IEntity
    {
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public Class Class { get; set; }

        [Required]
        public string StudentId { get; set; }

        [Required]
        public User Student { get; set; }

        public ICollection<StudentGrades> StudentGrades { get; set; } = new List<StudentGrades>();
    }
}
