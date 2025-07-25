using SistemaGestaoEscola.Web.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class StudentGrades : IEntity
    {
        public int Id { get; set; }

        [Required]
        public int ClassStudentsId { get; set; }

        [Required]
        public ClassStudents ClassStudents { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public Subject Subject { get; set; }

        [Required]
        [Range(0, 20, ErrorMessage = "A nota deve estar entre 0 e 20.")]
        public int Value { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Faltas injustificadas não podem ser negativas.")]
        public int UnexcusedAbsence { get; set; }
    }
}
