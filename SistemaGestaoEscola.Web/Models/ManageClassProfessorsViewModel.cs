namespace SistemaGestaoEscola.Web.Models
{
    public class ManageClassProfessorsViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }

        public List<ProfessorAssignmentViewModel> Assignments { get; set; } = new();
    }
}
