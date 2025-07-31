namespace SistemaGestaoEscola.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSubjects { get; set; }
        public int ActiveCourses { get; set; }
        public int InactiveCourses { get; set; }
        public int TotalClasses { get; set; }

        public List<ChartData> CoursesChartData { get; set; } = new();
    }
}
