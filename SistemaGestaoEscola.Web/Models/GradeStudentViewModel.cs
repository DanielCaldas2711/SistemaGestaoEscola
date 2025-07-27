using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Models
{
    public class GradeStudentViewModel
    {
        public int ClassStudentId { get; set; }

        [ValidateNever]
        public string FullName { get; set; }

        public int? Grade { get; set; }

        [Range(0, 20, ErrorMessage = "O valor precisa ser entre 0 e 20 horas.")]
        public int UnexcusedAbsence { get; set; }
    }
}
