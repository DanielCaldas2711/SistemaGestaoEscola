using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data;
using SistemaGestaoEscola.Web.Models;
using System.Diagnostics;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext _context;

    public HomeController(ILogger<HomeController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Courses()
    {
        var courses = await _context.Courses
            .Where(c => c.IsActive)
            .Include(c => c.CourseDisciplines)
                .ThenInclude(cd => cd.Subject)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var model = courses.Select(c => new PublicCourseViewModel
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
        }).ToList();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Classes()
    {
        var classes = await _context.Classes
            .Where(c => c.StartingDate > DateTime.UtcNow)
            .Include(c => c.Course)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var model = classes.Select(c => new PublicClassViewModel
        {
            Id = c.Id,
            ClassName = c.Name,
            CourseType = c.Course.Type,
            CourseName = c.Course.Name,
            StartingDate = c.StartingDate,
            EndingDate = c.EndingDate,
            DurationHours = c.Course.Duration,
            Shift = c.Shift
        }).ToList();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ClassDetails(int id)
    {
        var turma = await _context.Classes
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
            StartingDate = turma.StartingDate,
            EndingDate = turma.EndingDate,
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
