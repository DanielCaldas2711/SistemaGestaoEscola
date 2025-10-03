using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.DTOs;

namespace SistemaGestaoEscola.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly IClassStudentsRepository _classStudentsRepository;

        public GradesController(IClassStudentsRepository classStudentsRepository)
        {
            _classStudentsRepository = classStudentsRepository;
        }

        /// <summary>
        /// Retorna todas as notas (StudentGrades) de um aluno por StudentId,
        /// incluindo o status calculado:
        /// - "Reprovado por falta" se UnexcusedAbsence > Subject.Absence;
        /// - senão, "Aprovado" se Value >= 10; "Reprovado" se Value < 10.
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetSubjectsWithGrades(Guid studentId)
        {
            var studentIdStr = studentId.ToString();

            // 1) Verifica se está matriculado em alguma turma
            var isEnrolled = await _classStudentsRepository.GetAll()
                .AsNoTracking()
                .AnyAsync(cs => cs.StudentId == studentIdStr);

            if (!isEnrolled)
                return NotFound(new { message = "O aluno não está em nenhuma turma." });

            // 2) Projeta somente o necessário (subjects da(s) turma(s) + grades do aluno)
            var data = await _classStudentsRepository.GetAll()
                .Where(cs => cs.StudentId == studentIdStr)
                .AsNoTracking()
                .Select(cs => new
                {
                    Subjects = cs.Class.Course.CourseDisciplines
                        .Select(cd => new
                        {
                            SubjectId = cd.Subject.Id,
                            cd.Subject.Code,
                            cd.Subject.Name,
                            cd.Subject.Hours,
                            cd.Subject.Absence
                        })
                        .ToList(),

                    Grades = cs.StudentGrades
                        .Select(g => new
                        {
                            g.SubjectId,
                            g.Value,
                            g.UnexcusedAbsence
                        })
                        .ToList()
                })
                .ToListAsync();

            if (data.Count == 0)
                return NotFound(new { message = "Nenhum dado de curso/turma encontrado para este aluno." });

            // 3) Distinct de disciplinas por SubjectId
            var allSubjects = data
                .SelectMany(d => d.Subjects)
                .GroupBy(s => s.SubjectId)
                .Select(g => g.First())
                .ToList();

            // 4) Indexa notas por SubjectId (se houver várias, pegue a primeira; ajuste se quiser a mais recente)
            var gradesBySubject = data
                .SelectMany(d => d.Grades)
                .GroupBy(g => g.SubjectId)
                .ToDictionary(g => g.Key, g => g.First());

            // 5) Monta a lista tipada (DTO) com o status
            var result = allSubjects
                .Select(s =>
                {
                    gradesBySubject.TryGetValue(s.SubjectId, out var grade); // pode ser null

                    if (grade == null)
                    {
                        return new SubjectGradeSummaryDto
                        {
                            SubjectId = s.SubjectId,
                            Code = s.Code,
                            Name = s.Name,
                            Hours = s.Hours,
                            AbsenceLimit = s.Absence,
                            HasGrade = false,
                            Value = null,
                            UnexcusedAbsence = null,
                            FailedByAbsence = false,
                            Status = "Sem nota"
                        };
                    }

                    var failedByAbsence = grade.UnexcusedAbsence >= s.Absence;
                    var status = failedByAbsence
                        ? "Reprovado por falta"
                        : (grade.Value >= 10 ? "Aprovado" : "Reprovado");

                    return new SubjectGradeSummaryDto
                    {
                        SubjectId = s.SubjectId,
                        Code = s.Code,
                        Name = s.Name,
                        Hours = s.Hours,
                        AbsenceLimit = s.Absence,
                        HasGrade = true,
                        Value = grade.Value,
                        UnexcusedAbsence = grade.UnexcusedAbsence,
                        FailedByAbsence = failedByAbsence,
                        Status = status
                    };
                })
                .OrderBy(x => x.Name)
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Verifica se o aluno possui novas notas (HasNewGrades == true).
        /// </summary>
        [HttpGet("notifications/{studentId}")]
        public async Task<IActionResult> CheckNewGrades(Guid studentId)
        {
            var hasNewGrades = await _classStudentsRepository.GetAll()
                .AsNoTracking()
                .AnyAsync(cs => cs.StudentId == studentId.ToString() && cs.HasNewGrades);

            return Ok(new { hasNewGrades });
        }

        /// <summary>
        /// Marca como lidas as novas notas (HasNewGrades = false).
        /// </summary>
        [HttpPost("notifications/{studentId}/clear")]
        public async Task<IActionResult> ClearNewGrades(Guid studentId)
        {
            var entries = await _classStudentsRepository.GetAll()
                .Where(cs => cs.StudentId == studentId.ToString() && cs.HasNewGrades)
                .ToListAsync();

            if (!entries.Any())
                return Ok(new { updated = false });

            foreach (var cs in entries)
            {
                cs.HasNewGrades = false;
                await _classStudentsRepository.UpdateAsync(cs);
            }

            return Ok(new { updated = true });
        }
    }
}
