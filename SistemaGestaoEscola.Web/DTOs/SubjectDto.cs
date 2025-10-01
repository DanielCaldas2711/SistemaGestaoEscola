namespace SistemaGestaoEscola.Web.DTOs
{
    public class SubjectDto
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = "";
        public int Hours { get; set; }
        public int Absence { get; set; }
    }
}
