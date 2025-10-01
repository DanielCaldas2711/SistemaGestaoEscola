using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.DTOs;

namespace SistemaGestaoEscola.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassesController : ControllerBase
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentsRepository _classStudentsRepository;

        public ClassesController(IClassRepository classRepository,
            IClassStudentsRepository classStudentsRepository)
        {
            _classRepository = classRepository;
            _classStudentsRepository = classStudentsRepository;
        }

        /// <summary>
        /// Retorna os detalhes de uma turma (classe) específica pelo seu ID.
        /// Inclui informações do curso associado e uma lista de professores (sem duplicados).
        /// </summary>
        /// <param name="id">ID da turma (Class.Id)</param>
        /// <returns>
        /// 200 OK com um objeto <see cref="ClassDto"/> contendo:
        /// <list type="bullet">
        ///   <item><description>Id, Nome, Datas (início/fim), Turno e Curso associado</description></item>
        ///   <item><description>Professores distintos (agrupados por FullName)</description></item>
        /// </list>
        /// 404 NotFound caso a turma não seja encontrada.
        /// </returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var classDetails = await _classRepository.GetAll()
                .AsNoTracking()
                .Where(cl => cl.Id == id)
                .Select(cl => new ClassDto
                {
                    Id = cl.Id,
                    Name = cl.Name,
                    StartingDate = cl.StartingDate,
                    EndingDate = cl.EndingDate,
                    Shift = cl.Shift,
                    CourseId = cl.CourseId,
                    CourseName = cl.Course != null ? cl.Course.Name : string.Empty,

                    Professors = cl.Professors
                        .Where(p => p.Professor != null)
                        .GroupBy(p => p.Professor.Email)
                        .Select(g => new ProfessorDto
                        {
                            Id = g.First().Id,
                            FullName = (g.First().Professor.FirstName ?? "") + " " + (g.First().Professor.LastName ?? ""),
                            Email = g.Key
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (classDetails == null)
                return NotFound(new { message = "Turma não encontrada." });

            return Ok(classDetails);
        }

        /// <summary>
        /// Retorna todos os alunos de uma turma pelo ID da turma.
        /// </summary>
        /// <param name="classId">ID da turma</param>
        /// <returns>Lista de alunos</returns>
        [HttpGet("{classId}/students")]
        public async Task<IActionResult> GetStudents(int classId)
        {
            var classEntity = await _classRepository.GetByIdAsync(classId);

            if (classEntity == null)
            {
                return NotFound(new { message = "Turma não encontrada." });
            }

            var studentEntity = await _classStudentsRepository.GetAllStudentsFromClass(classEntity.Id);

            var students = studentEntity
                .Select(cs => new
                {
                    cs.Id,
                    cs.FullName,
                    cs.Email
                });

            if (!students.Any())
            {
                return NoContent();
            }

            return Ok(students);
        }
    }
}
