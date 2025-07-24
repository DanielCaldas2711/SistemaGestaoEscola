namespace SistemaGestaoEscola.Web.Models
{
    public class ToggleStudentViewModel
    {
        public int ClassId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public bool Assign { get; set; }
    }
}
