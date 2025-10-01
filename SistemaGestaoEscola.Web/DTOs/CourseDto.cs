namespace SistemaGestaoEscola.Web.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public int Duration { get; set; }
        public bool IsActive { get; set; }
        public List<SubjectDto> Subjects { get; set; } = new();
    }
}
