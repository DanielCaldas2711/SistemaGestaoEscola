using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SistemaGestaoEscola.Web.Models
{
    public class GradeEntryViewModel
    {
        public int ClassId { get; set; }

        [ValidateNever]
        public string ClassName { get; set; }

        public int SubjectId { get; set; }

        [ValidateNever]
        public string SubjectName { get; set; }

        public List<GradeStudentViewModel> Students { get; set; } = new();
    }
}
