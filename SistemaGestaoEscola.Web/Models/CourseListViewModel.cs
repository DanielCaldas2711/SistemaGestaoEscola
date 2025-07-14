using SistemaGestaoEscola.Web.Data.Entities;

public class CourseListViewModel
{
    public List<Course> Courses { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? TypeFilter { get; set; }
    public bool? IsActiveFilter { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
