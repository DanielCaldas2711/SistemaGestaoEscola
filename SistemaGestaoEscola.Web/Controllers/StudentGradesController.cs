using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Repositories;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class StudentGradesController : Controller
    {
        private readonly IStudentGradesRepository _studentGradesRepository;
        private readonly IUserHelper _userHelper;
        private readonly IClassStudentsRepository _classStudentsRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IClassProfessorsRepository _classProfessorsRepository;
        private readonly IClassRepository _classRepository;

        public StudentGradesController(IStudentGradesRepository studentGradesRepository,
            IUserHelper userHelper,
            IClassStudentsRepository classStudentsRepository,
            ISubjectRepository subjectRepository,
            IClassProfessorsRepository classProfessorsRepository,
            IClassRepository classRepository)
        {
            _studentGradesRepository = studentGradesRepository;
            _userHelper = userHelper;
            _classStudentsRepository = classStudentsRepository;
            _subjectRepository = subjectRepository;
            _classProfessorsRepository = classProfessorsRepository;
            _classRepository = classRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var user = await _userHelper.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var professorSubjects = _classProfessorsRepository.GetAll()
                .AsTracking()
                .Where(cp => cp.ProfessorId == user.Id && cp.ClassId == id)
                .Include(cp => cp.Subject);

            if (!professorSubjects.Any())
            {
                TempData["ToastError"] = "Você não está atribuído a esta turma.";
                return RedirectToAction(nameof(Index));
            }

            var classInfo = _classRepository.GetAll()
                .Include(c => c.Course);

            var Class = classInfo.FirstOrDefault(c => c.Id == id);

            if (Class == null)
            {
                TempData["ToastError"] = "Erro ao buscar a turma.";
                return View();
            }

            var model = new ManageGradesViewModel
            {
                ClassId = Class.Id,
                ClassName = Class.Name,
                CourseName = Class.Course.Name,
                Subjects = professorSubjects.Select(ps => new SubjectInfoViewModel
                {
                    SubjectId = ps.Subject.Id,
                    SubjectName = ps.Subject.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageGrades(int classId, int subjectId)
        {
            var user = await _userHelper.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            
            var isAssigned = await _classProfessorsRepository.GetAll()
                .AnyAsync(cp => cp.ClassId == classId && cp.SubjectId == subjectId && cp.ProfessorId == user.Id);

            if (!isAssigned)
            {
                TempData["ToastError"] = "Você não está atribuído a esta disciplina nesta turma.";
                return RedirectToAction(nameof(Index));
            }
            
            var turma = await _classRepository.GetAll()
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classId);

            var subject = await _subjectRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (turma == null || subject == null)
            {
                TempData["ToastError"] = "Turma ou disciplina não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var students = await _classStudentsRepository.GetAll()
                .Where(cs => cs.ClassId == classId)
                .Include(cs => cs.Student)
                .ToListAsync();
            
            var grades = await _studentGradesRepository.GetAll()
                .Where(g => g.SubjectId == subjectId && students.Select(s => s.Id).Contains(g.ClassStudentsId))
                .ToListAsync();

            var model = new GradeEntryViewModel
            {
                ClassId = turma.Id,
                ClassName = turma.Name,
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                Students = students.Select(s => {
                    var grade = grades.FirstOrDefault(g => g.ClassStudentsId == s.Id);

                    return new GradeStudentViewModel
                    {
                        ClassStudentId = s.Id,
                        FullName = $"{s.Student.FirstName} {s.Student.LastName}",
                        Grade = grade?.Value,
                        UnexcusedAbsence = grade?.UnexcusedAbsence ?? 0
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageGrades(GradeEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Dados inválidos enviados.";
                return RedirectToAction(nameof(Manage), new { id = model.ClassId });
            }

            var user = await _userHelper.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            
            var isAuthorized = await _classProfessorsRepository.GetAll()
                .AnyAsync(cp => cp.ClassId == model.ClassId && cp.SubjectId == model.SubjectId && cp.ProfessorId == user.Id);

            if (!isAuthorized)
            {
                TempData["ToastError"] = "Você não está autorizado a lançar notas nesta disciplina.";
                return RedirectToAction(nameof(Index));
            }

            var subject = await _subjectRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.SubjectId);

            foreach (var student in model.Students)
            {
                if (subject != null)
                {
                    if (student.UnexcusedAbsence > subject.Absence)
                    {
                        TempData["ToastError"] = "Insira corretamente as horas de faltas dos alunos.";
                        return RedirectToAction(nameof(Index));
                    }
                }                
            }

            try
            {
                foreach (var student in model.Students)
                {
                    var grade = await _studentGradesRepository.GetAll()
                        .FirstOrDefaultAsync(g => g.ClassStudentsId == student.ClassStudentId && g.SubjectId == model.SubjectId);

                    if (grade == null)
                    {
                        await _studentGradesRepository.CreateAsync(new Data.Entities.StudentGrades
                        {
                            ClassStudentsId = student.ClassStudentId,
                            SubjectId = model.SubjectId,
                            Value = student.Grade ?? 0,
                            UnexcusedAbsence = student.UnexcusedAbsence
                        });
                    }
                    else
                    {                        
                        grade.Value = student.Grade ?? 0;
                        grade.UnexcusedAbsence = student.UnexcusedAbsence;
                        await _studentGradesRepository.UpdateAsync(grade);
                    }
                }

                TempData["ToastSuccess"] = "Notas salvas com sucesso!";
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Erro ao salvar as notas.";
            }

            return RedirectToAction(nameof(Manage), new { id = model.ClassId });
        }


        [HttpGet]
        public async Task<IActionResult> LoadProfessorClasses(string? searchTerm)
        {
            var user = await _userHelper.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var query = _classProfessorsRepository.GetAll()
                .Include(cp => cp.Class)
                .ThenInclude(c => c.Course)
                .Where(cp => cp.ProfessorId == user.Id)
                .Select(cp => cp.Class)
                .Distinct();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().StartsWith(term));
            }

            var classes = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            return PartialView("_ProfessorClassListPartial", classes);
        }
    }
}
