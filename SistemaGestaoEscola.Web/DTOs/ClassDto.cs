namespace SistemaGestaoEscola.Web.DTOs
{
    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime StartingDate { get; set; }
        public DateTime? EndingDate { get; set; }
        public string? Shift { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public List<ProfessorDto> Professors { get; set; } = new();
    }
}
