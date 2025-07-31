using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;
using System.Diagnostics;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IClassRepository _classRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ITimeZoneHelper _timeZoneHelper;

    public HomeController(
        ILogger<HomeController> logger,
        IClassRepository classRepository,
        ICourseRepository courseRepository,
        ITimeZoneHelper timeZoneHelper)
    {
        _logger = logger;
        _classRepository = classRepository;
        _courseRepository = courseRepository;
        _timeZoneHelper = timeZoneHelper;
    }

    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [HttpGet]
    public async Task<IActionResult> Courses(int page = 1, int pageSize = 6)
    {
        var query = _courseRepository.GetAll()
            .Where(c => c.IsActive)
            .Include(c => c.CourseDisciplines)
                .ThenInclude(cd => cd.Subject)
            .OrderBy(c => c.Name);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var courses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PaginatedListViewModel<PublicCourseViewModel>
        {
            PageIndex = page,
            TotalPages = totalPages,
            Items = courses.Select(c => new PublicCourseViewModel
            {
                CourseName = c.Name,
                CourseType = c.Type,
                Duration = c.Duration,
                Subjects = c.CourseDisciplines
                    .Select(cd => new PublicSubjectViewModel
                    {
                        SubjectName = cd.Subject.Name,
                        Hours = cd.Subject.Hours
                    }).ToList()
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Classes(int page = 1, int pageSize = 6)
    {
        var now = DateTime.UtcNow;

        var query = _classRepository.GetAll()
            .Where(c => c.StartingDate > now)
            .Include(c => c.Course)
            .OrderBy(c => c.Name);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var classes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PaginatedListViewModel<PublicClassViewModel>
        {
            PageIndex = page,
            TotalPages = totalPages,
            Items = classes.Select(c => new PublicClassViewModel
            {
                Id = c.Id,
                ClassName = c.Name,
                CourseType = c.Course.Type,
                CourseName = c.Course.Name,
                StartingDate = _timeZoneHelper.ConvertUtcToLisbon(c.StartingDate),
                EndingDate = _timeZoneHelper.ConvertUtcToLisbon(c.EndingDate),
                DurationHours = c.Course.Duration,
                Shift = c.Shift
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ClassDetails(int id)
    {
        var turma = await _classRepository.GetAll()
            .Include(c => c.Course)
                .ThenInclude(c => c.CourseDisciplines)
                    .ThenInclude(cd => cd.Subject)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (turma == null)
        {
            TempData["ToastError"] = "Turma não encontrada.";
            return RedirectToAction(nameof(Classes));
        }

        var model = new PublicClassDetailsViewModel
        {
            ClassName = turma.Name,
            CourseName = turma.Course.Name,
            StartingDate = _timeZoneHelper.ConvertUtcToLisbon(turma.StartingDate),
            EndingDate = _timeZoneHelper.ConvertUtcToLisbon(turma.EndingDate),
            Duration = turma.Course.Duration,
            Shift = turma.Shift,
            Subjects = turma.Course.CourseDisciplines
                .Select(cd => new SubjectInfo
                {
                    Name = cd.Subject.Name,
                    Hours = cd.Subject.Hours
                }).ToList()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
