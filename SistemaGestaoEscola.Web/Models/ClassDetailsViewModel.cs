namespace SistemaGestaoEscola.Web.Models
{
    public class ClassDetailsViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string Shift { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public string CourseName { get; set; }
        public List<SubjectProfessorViewModel> Subjects { get; set; } = new();
        public List<StudentViewModel> Students { get; set; } = new();
    }
}
