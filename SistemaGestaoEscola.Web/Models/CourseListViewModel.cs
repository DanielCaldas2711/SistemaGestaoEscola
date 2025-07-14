using SistemaGestaoEscola.Web.Data.Entities;

namespace SistemaGestaoEscola.Web.Models
{
    public class CourseListViewModel
    {
        public IEnumerable<Course> Courses { get; set; } = new List<Course>();

        public string? SearchTerm { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }
}
