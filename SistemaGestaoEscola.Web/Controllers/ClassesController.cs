using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Enums;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class ClassesController : Controller
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentsRepository _classStudentsRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IClassProfessorsRepository _classProfessorsRepository;
        private readonly IUserHelper _userHelper;

        public ClassesController(IClassRepository classRepository,
            IClassStudentsRepository classStudentsRepository,
            ICourseRepository courseRepository,
            IClassProfessorsRepository classProfessorsRepository,
            IUserHelper userHelper)
        {
            _classRepository = classRepository;
            _classStudentsRepository = classStudentsRepository;
            _courseRepository = courseRepository;
            _classProfessorsRepository = classProfessorsRepository;
            _userHelper = userHelper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Create()
        {
            var courses = await _courseRepository.GetAll()
                                .Where(c => c.IsActive)
                                .OrderBy(c => c.Name)
                                .ToListAsync();

            ViewBag.Courses = new SelectList(courses, "Id", "Name");
            ViewBag.Shifts = Enum.GetNames(typeof(Shift)).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Class model)
        {
            if (!ModelState.IsValid)
            {
                var courses = await _courseRepository.GetAll()
                                    .Where(c => c.IsActive)
                                    .OrderBy(c => c.Name)
                                    .ToListAsync();

                ViewBag.Courses = new SelectList(courses, "Id", "Name", model.CourseId);
                TempData["ToastError"] = "Houve um erro.";
                return View(model);
            }

            if (model.StartingDate > model.EndingDate)
            {
                TempData["ToastError"] = "A data de início deve ser antes da data de fim.";
                return View(model);
            }

            var course = await _courseRepository.GetByIdAsync(model.CourseId);

            int BusinessDays = CalcBusinessDays(model.StartingDate.Date, model.EndingDate);

            int hours = 0;

            switch (model.Shift)
            {
                case "Diurno":
                    hours = BusinessDays * 7;
                    break;

                case "Noturno":
                    hours = BusinessDays * 4;
                    break;
            }

            if (hours >= course.Duration)
            {
                try
                {
                    await _classRepository.CreateAsync(model);
                    TempData["ToastSuccess"] = "Turma criada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["ToastError"] = "Erro ao criar turma.";
                    return View(model);
                }
            }

            TempData["ToastError"] = "Tempo insuficiente para a duração do curso";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var turma = await _classRepository.GetAll()
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (turma == null)
            {
                TempData["ToastError"] = "Turma não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var courses = await _courseRepository.GetAll()
                                .Where(c => c.IsActive)
                                .OrderBy(c => c.Name)
                                .ToListAsync();

            ViewBag.Courses = new SelectList(courses, "Id", "Name", turma.CourseId);
            ViewBag.Shifts = Enum.GetNames(typeof(Shift)).ToList();

            return View(turma);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Class model)
        {
            var courses = await _courseRepository.GetAll()
                                .Where(c => c.IsActive)
                                .OrderBy(c => c.Name)
                                .ToListAsync();

            ViewBag.Courses = new SelectList(courses, "Id", "Name", model.CourseId);
            ViewBag.Shifts = Enum.GetNames(typeof(Shift)).ToList();

            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Verifique os campos antes de salvar.";
                return View(model);
            }

            if (model.EndingDate < model.StartingDate)
            {
                TempData["ToastError"] = "A data de início deve ser antes da data de fim.";
                return View(model);
            }

            var course = await _courseRepository.GetByIdAsync(model.CourseId);

            int BusinessDays = CalcBusinessDays(model.StartingDate.Date, model.EndingDate);

            int hours = 0;

            switch (model.Shift)
            {
                case "Diurno":
                    hours = BusinessDays * 7;
                    break;

                case "Noturno":
                    hours = BusinessDays * 4;
                    break;
            }

            if (hours >= course.Duration)
            {
                try
                {
                    await _classRepository.UpdateAsync(model);
                    TempData["ToastSuccess"] = "Turma atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    TempData["ToastError"] = "Erro ao atualizar turma.";
                    return View(model);
                }
            }
            TempData["ToastError"] = "Tempo insuficiente para a duração do curso";
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classEntity = await _classRepository.GetByIdAsync(id);

            if (classEntity == null)
            {
                TempData["ToastError"] = "Turma não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _classRepository.DeleteAsync(classEntity);
                TempData["ToastSuccess"] = "Turma excluída com sucesso!";
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Erro ao excluir turma.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageStudents(int id)
        {
            var turma = await _classRepository.GetAll()
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (turma == null)
            {
                TempData["ToastError"] = "Turma não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var studentsInOtherClasses = await _classStudentsRepository.GetAll()
                .Where(cs => cs.ClassId != id)
                .Select(cs => cs.StudentId)
                .Distinct()
                .ToListAsync();

            var allStudents = await _userHelper.GetAllUsersByRoleAsync(UserRole.Student.ToString());

            var assignedStudentIds = await _classStudentsRepository.GetAll()
                .Where(cs => cs.ClassId == id)
                .Select(cs => cs.StudentId)
                .ToListAsync();

            var availableStudents = allStudents
                .Where(u => !studentsInOtherClasses.Contains(u.Id))
                .ToList();

            var model = new ManageClassStudentsViewModel
            {
                ClassId = turma.Id,
                ClassName = turma.Name,
                AvailableStudents = availableStudents.Select(u => new StudentAssignmentViewModel
                {
                    StudentId = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    IsAssigned = assignedStudentIds.Contains(u.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageStudents(int classId, List<string> selectedStudentIds)
        {
            var turma = await _classRepository.GetByIdAsync(classId);
            if (turma == null)
            {
                TempData["ToastError"] = "Turma não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var existingAssignments = await _classStudentsRepository.GetAll()
                .Where(cs => cs.ClassId == classId)
                .ToListAsync();

            var currentStudentIds = existingAssignments.Select(cs => cs.StudentId).ToList();

            var toAdd = selectedStudentIds.Except(currentStudentIds).ToList();
            var toRemove = currentStudentIds.Except(selectedStudentIds).ToList();

            try
            {
                foreach (var studentId in toAdd)
                {
                    await _classStudentsRepository.CreateAsync(new ClassStudents
                    {
                        ClassId = classId,
                        StudentId = studentId 
                    });
                }

                foreach (var studentId in toRemove)
                {
                    var assignment = existingAssignments.FirstOrDefault(cs => cs.StudentId.ToString() == studentId);
                    if (assignment != null)
                    {
                        await _classStudentsRepository.DeleteAsync(assignment);
                    }
                }

                TempData["ToastSuccess"] = "Alunos atualizados com sucesso!";
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Erro ao atualizar alunos da turma.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageProfessors(int id)
        {
            var Class = await _classRepository.GetAll()
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Class == null)
            {
                TempData["ToastError"] = "Turma não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var courseDisciplines = await _courseRepository.GetAll()
                .Where(c => c.Id == Class.CourseId)
                .SelectMany(c => c.CourseDisciplines)
                .Include(cd => cd.Subject)
                .ToListAsync();

            var professors = await _userHelper.GetAllUsersByRoleAsync(UserRole.Professor.ToString());

            var existingAssignments = await _classProfessorsRepository.GetAllClassProfessors(Class.Id);

            var model = new ManageClassProfessorsViewModel
            {
                ClassId = Class.Id,
                ClassName = Class.Name,
                Assignments = courseDisciplines.Select(cd =>
                {
                    var currentAssignment = existingAssignments
                        .FirstOrDefault(cp => cp.SubjectId == cd.SubjectId);

                    return new ProfessorAssignmentViewModel
                    {
                        SubjectId = cd.SubjectId,
                        SubjectName = cd.Subject.Name,
                        AssignedProfessorId = currentAssignment?.ProfessorId,
                        AvailableProfessors = professors.Select(p => new SelectListItem
                        {
                            Value = p.Id,
                            Text = $"{p.FirstName} {p.LastName}"
                        }).ToList()
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageProfessors(ManageClassProfessorsViewModel model)
        {
            var turma = await _classRepository.GetByIdAsync(model.ClassId);
            if (turma == null)
            {
                TempData["ToastError"] = "Turma não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var existingAssignments = await _classProfessorsRepository.GetAll()
                .Where(cp => cp.ClassId == model.ClassId)
                .ToListAsync();

            try
            {
                foreach (var assignment in model.Assignments)
                {
                    var existing = existingAssignments.FirstOrDefault(cp => cp.SubjectId == assignment.SubjectId);

                    if (!string.IsNullOrWhiteSpace(assignment.AssignedProfessorId))
                    {
                        if (existing != null)
                        {
                            existing.ProfessorId = assignment.AssignedProfessorId;
                            await _classProfessorsRepository.UpdateAsync(existing);
                        }
                        else
                        {
                            await _classProfessorsRepository.CreateAsync(new ClassProfessors
                            {
                                ClassId = model.ClassId,
                                SubjectId = assignment.SubjectId,
                                ProfessorId = assignment.AssignedProfessorId
                            });
                        }
                    }
                    else
                    {
                        if (existing != null)
                        {
                            await _classProfessorsRepository.DeleteAsync(existing);
                        }
                    }
                }

                TempData["ToastSuccess"] = "Professores atualizados com sucesso!";
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Erro ao salvar os professores.";
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> LoadClasses(string? searchTerm)
        {
            var query = _classRepository.GetAll().Include(c => c.Course);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerTerm = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lowerTerm)).Include(c => c.Course);
            }

            var classes = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            return PartialView("_ClassListPartial", classes);
        }

        [HttpGet]
        public async Task<IActionResult> LoadStudents(int classId, string? searchTerm)
        {
            var students = await GetFilteredStudents(classId, searchTerm);
            return PartialView("_StudentListPartial", students);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStudent([FromBody] ToggleStudentViewModel model)
        {
            if (model == null || model.ClassId <= 0 || string.IsNullOrWhiteSpace(model.StudentId))
                return BadRequest("Dados inválidos.");

            var classEntity = await _classRepository.GetByIdAsync(model.ClassId);
            if (classEntity == null)
                return NotFound("Turma não encontrada.");

            var student = await _userHelper.GetUserByIdAsync(model.StudentId);
            if (student == null)
                return NotFound("Aluno não encontrado.");

            var existingAssignment = await _classStudentsRepository.GetAll()
                .FirstOrDefaultAsync(cs => cs.ClassId == model.ClassId && cs.StudentId == model.StudentId);

            if (model.Assign && existingAssignment == null)
            {
                await _classStudentsRepository.CreateAsync(new ClassStudents
                {
                    ClassId = model.ClassId,
                    StudentId = model.StudentId
                });
            }
            else if (!model.Assign && existingAssignment != null)
            {
                await _classStudentsRepository.DeleteAsync(existingAssignment);
            }

            var updatedStudents = await GetFilteredStudents(model.ClassId);

            return PartialView("_StudentListPartial", updatedStudents);
        }

        public static int CalcBusinessDays(DateTime StartDate, DateTime EndDate)
        {
            if (EndDate < StartDate)
                return 0;

            int busyDays = 0;

            for (var data = StartDate.Date; data <= EndDate.Date; data = data.AddDays(1))
            {
                if (data.DayOfWeek != DayOfWeek.Saturday &&
                    data.DayOfWeek != DayOfWeek.Sunday)
                {
                    busyDays++;
                }
            }

            return busyDays;
        }

        private async Task<List<StudentAssignmentViewModel>> GetFilteredStudents(int classId, string? searchTerm = null)
        {
            var studentsInOtherClasses = await _classStudentsRepository.GetAll()
                .Where(cs => cs.ClassId != classId)
                .Select(cs => cs.StudentId)
                .Distinct()
                .ToListAsync();

            var allStudents = await _userHelper.GetAllUsersByRoleAsync(UserRole.Student.ToString());

            var assignedStudentIds = await _classStudentsRepository.GetAll()
                .Where(cs => cs.ClassId == classId)
                .Select(cs => cs.StudentId)
                .ToListAsync();

            var availableStudents = allStudents
                .Where(u => !studentsInOtherClasses.Contains(u.Id))
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                availableStudents = availableStudents
                    .Where(s => $"{s.FirstName} {s.LastName}".ToLower().Contains(term))
                    .ToList();
            }

            return availableStudents.Select(s => new StudentAssignmentViewModel
            {
                StudentId = s.Id,
                FullName = $"{s.FirstName} {s.LastName}",
                IsAssigned = assignedStudentIds.Contains(s.Id)
            }).ToList();
        }

    }
}
