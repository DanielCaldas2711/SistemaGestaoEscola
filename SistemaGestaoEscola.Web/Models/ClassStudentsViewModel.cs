namespace SistemaGestaoEscola.Web.Models
{
    public class ClassStudentsViewModel
    {        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string CourseName { get; set; }
        public string Shift { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public List<StudentAssignmentViewModel> Students { get; set; }
    }
}
