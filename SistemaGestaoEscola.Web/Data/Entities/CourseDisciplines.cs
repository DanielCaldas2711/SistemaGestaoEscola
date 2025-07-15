using SistemaGestaoEscola.Web.Data.Entities.Interfaces;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class CourseDisciplines : IEntity
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public int SubjectId { get; set; }

        public Subject Subject { get; set; }
    }
}
