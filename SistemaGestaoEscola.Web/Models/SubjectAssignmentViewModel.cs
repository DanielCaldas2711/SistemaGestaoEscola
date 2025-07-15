namespace SistemaGestaoEscola.Web.Models
{
    public class SubjectAssignmentViewModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }

        public int Hours { get; set; }

        public bool IsAssigned { get; set; }
    }
}
