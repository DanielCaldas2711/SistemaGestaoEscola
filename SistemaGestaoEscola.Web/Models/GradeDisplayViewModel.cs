namespace SistemaGestaoEscola.Web.Models
{
    public class GradeDisplayViewModel
    {
        public string SubjectName { get; set; }

        public int SubjectCode { get; set; }
        public int Grade { get; set; }
        public int Absences { get; set; }
        public int SubjectHours { get; set; }
    }
}
