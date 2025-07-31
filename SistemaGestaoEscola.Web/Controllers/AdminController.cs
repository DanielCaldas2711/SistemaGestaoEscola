using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IClassRepository _classRepository;

        public AdminController(IUserHelper userHelper,
            ISubjectRepository subjectRepository,
            ICourseRepository courseRepository,
            IClassRepository classRepository)
        {
            _userHelper = userHelper;
            _subjectRepository = subjectRepository;
            _courseRepository = courseRepository;
            _classRepository = classRepository;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel
            {
                TotalUsers = _userHelper.GetAllUsersAsync().Result.Count(),
                TotalSubjects = _subjectRepository.GetAll().Count(),
                ActiveCourses = _courseRepository.GetAll().Count(c => c.IsActive),
                InactiveCourses = _courseRepository.GetAll().Count(c => !c.IsActive),
                TotalClasses = _classRepository.GetAll().Count(),

                CoursesChartData = new List<ChartData>
                {
                    new ChartData { Category = "Ativos", Value = _courseRepository.GetAll().Count(c => c.IsActive) },
                    new ChartData { Category = "Inativos", Value = _courseRepository.GetAll().Count(c => !c.IsActive) }
                }
            };

            return View(model);
        }
    }
}
