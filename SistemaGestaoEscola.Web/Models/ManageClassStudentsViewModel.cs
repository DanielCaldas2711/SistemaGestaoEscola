namespace SistemaGestaoEscola.Web.Models
{
    public class ManageClassStudentsViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public List<StudentAssignmentViewModel> AvailableStudents { get; set; }
    }
}
