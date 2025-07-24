using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaGestaoEscola.Web.Models
{
    public class ProfessorAssignmentViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }

        public string? AssignedProfessorId { get; set; }

        public List<SelectListItem> AvailableProfessors { get; set; } = new();
    }
}
