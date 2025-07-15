namespace SistemaGestaoEscola.Web.Models
{
    public class CourseDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Duration { get; set; }
        public bool IsActive { get; set; }
        public List<SubjectViewModel> AssignedSubjects { get; set; }
    }
}
