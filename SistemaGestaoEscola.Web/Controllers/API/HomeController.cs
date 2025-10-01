using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.DTOs;

namespace SistemaGestaoEscola.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IClassRepository _classRepository;

        public HomeController(ICourseRepository courseRepository,
            IClassRepository classRepository)
        {
            _courseRepository = courseRepository;
            _classRepository = classRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var courses = await _courseRepository.GetAll()
                .Where(co => co.IsActive)
                .AsNoTracking()
                .Select(co => new CourseDto
                {
                    Id = co.Id,
                    Type = co.Type,
                    Name = co.Name,
                    Duration = co.Duration,
                    IsActive = co.IsActive,
                    Subjects = co.CourseDisciplines
                        .Where(cd => cd.Subject != null)
                        .Select(cd => new SubjectDto
                        {
                            Id = cd.Subject.Id,
                            Code = cd.Subject.Code,
                            Name = cd.Subject.Name,
                            Hours = cd.Subject.Hours,
                            Absence = cd.Subject.Absence
                        })
                        .ToList()
                })
                .ToListAsync();

            var classesRaw = await _classRepository.GetAll()
                .Where(cl => cl.StartingDate > DateTime.Today)
                .AsNoTracking()
                .Select(cl => new
                {
                    cl.Id,
                    cl.Name,
                    cl.StartingDate,
                    cl.EndingDate,
                    cl.Shift,
                    cl.CourseId,
                    CourseName = cl.Course.Name,
                    Professors = cl.Professors
                        .Select(p => new
                        {
                            p.Id,
                            p.Professor.FullName,
                            p.Professor.Email
                        })
                        .ToList()
                })
                .ToListAsync();

            var classes = classesRaw.Select(cl => new ClassDto
            {
                Id = cl.Id,
                Name = cl.Name,
                StartingDate = cl.StartingDate,
                EndingDate = cl.EndingDate,
                Shift = cl.Shift,
                CourseId = cl.CourseId,
                CourseName = cl.CourseName,
                Professors = cl.Professors
                    .GroupBy(p => p.FullName)
                    .Select(g => new ProfessorDto
                    {
                        Id = g.First().Id,
                        FullName = g.Key,
                        Email = g.First().Email
                    })
                    .ToList()
            }).ToList();

            return Ok(new { courses, classes });
        }

    }
}
