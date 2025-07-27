namespace SistemaGestaoEscola.Web.Models
{
    public class MyGradesViewModel
    {
        public string ClassName { get; set; }
        public string CourseName { get; set; }
        public List<GradeDisplayViewModel> Grades { get; set; } = new();
    }
}
