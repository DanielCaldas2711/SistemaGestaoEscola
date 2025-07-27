namespace SistemaGestaoEscola.Web.Models
{
    public class PublicClassDetailsViewModel
    {
        public string ClassName { get; set; }
        public string CourseName { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public int Duration { get; set; }
        public string Shift { get; set; }
        public List<SubjectInfo> Subjects { get; set; } = new();
    }
}
