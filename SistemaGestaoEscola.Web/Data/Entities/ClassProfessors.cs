using SistemaGestaoEscola.Web.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class ClassProfessors : IEntity
    {
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public Class Class { get; set; }

        [Required]
        public string ProfessorId { get; set; }

        [Required]
        public User Professor { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public Subject Subject { get; set; }
    }
}
