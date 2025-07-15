namespace SistemaGestaoEscola.Web.Models
{
    public class CourseSubjectsViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }

        public int CourseDuration { get; set; }

        public List<SubjectAssignmentViewModel> Subjects { get; set; }
    }
}
