namespace SistemaGestaoEscola.Web.Models
{
    public class ManageGradesViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string CourseName { get; set; }
        public List<SubjectInfoViewModel> Subjects { get; set; } = new();
    }
}
