using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.DTOs;

namespace SistemaGestaoEscola.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;

        public CoursesController(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        /// <summary>
        /// Retorna os detalhes de um curso específico pelo seu ID.
        /// Inclui as disciplinas (Subjects) relacionadas ao curso.
        /// </summary>
        /// <param name="id">Identificador único do curso.</param>
        /// <returns>
        /// 200 (OK) com os dados do curso e suas disciplinas;
        /// 404 (NotFound) se o curso não for encontrado.
        /// </returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseRepository.GetAll()
                .Where(co => co.Id == id)
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
                        .GroupBy(s => s.Id)
                        .Select(g => g.First())
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound(new { message = "Curso não encontrado." });

            return Ok(course);
        }
    }
}
