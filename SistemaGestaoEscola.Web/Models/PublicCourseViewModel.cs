namespace SistemaGestaoEscola.Web.Models
{
    public class PublicCourseViewModel
    {
        public string CourseName { get; set; }

        public string CourseType { get; set; }
        public int Duration { get; set; }
        
        public List<PublicSubjectViewModel> Subjects { get; set; }
    }
}
