using SistemaGestaoEscola.Web.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    // ClassStudents.cs
    public class ClassStudents : IEntity
    {
        public int Id { get; set; }

        public int ClassId { get; set; }
        public Class Class { get; set; }

        public string StudentId { get; set; }
        public User Student { get; set; }

        public ICollection<StudentGrades> StudentGrades { get; set; } = new List<StudentGrades>();

        public bool HasNewGrades { get; set; } = false;
    }

}
