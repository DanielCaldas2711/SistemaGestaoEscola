using SistemaGestaoEscola.Web.Data.Entities;

namespace SistemaGestaoEscola.Web.Models
{
    public class SubjectListViewModel
    {
        public IEnumerable<Subject> Subjects { get; set; } = new List<Subject>();
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
