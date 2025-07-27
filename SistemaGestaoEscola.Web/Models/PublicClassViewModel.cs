namespace SistemaGestaoEscola.Web.Models
{
    public class PublicClassViewModel
    {
        public int Id { get; set; }
        public string ClassName { get; set; }

        public string CourseType { get; set; }
        public string CourseName { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public int DurationHours { get; set; }
        public string Shift { get; set; }
    }
}
