using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class CourseDisciplinesController : Controller
    {
        private readonly ICourseDisciplinesRepository _courseDisciplinesRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ISubjectRepository _subjectRepository;

        public CourseDisciplinesController(ICourseDisciplinesRepository courseDisciplinesRepository,
            ICourseRepository courseRepository,
            ISubjectRepository subjectRepository)
        {
            _courseDisciplinesRepository = courseDisciplinesRepository;
            _courseRepository = courseRepository;
            _subjectRepository = subjectRepository;
        }

        [Authorize(Roles = "Secretary")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Secretary")]
        [HttpGet]
        public async Task<IActionResult> ManageSubjects(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course == null)
            {
                TempData["ToastError"] = "Course not found.";
                return RedirectToAction("Index");
            }

            var allSubjects = await _subjectRepository.GetAll().OrderBy(s => s.Name).ToListAsync();

            var associatedSubjects = await _courseDisciplinesRepository.GetAll()
                .Where(cd => cd.CourseId == courseId)
                .Select(cd => cd.SubjectId)
                .ToListAsync();

            var model = new CourseSubjectsViewModel
            {
                CourseId = course.Id,
                CourseName = course.Name,
                CourseDuration = course.Duration,
                Subjects = allSubjects.Select(subject => new SubjectAssignmentViewModel
                {
                    Id = subject.Id,
                    Code = subject.Code,
                    Name = subject.Name,
                    Hours = subject.Hours,
                    IsAssigned = associatedSubjects.Contains(subject.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Secretary")]
        public async Task<IActionResult> UpdateSubjects(int courseId, List<int> SelectedSubjectIds)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                TempData["ToastError"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var existingAssociations = await _courseDisciplinesRepository.GetAll()
                .Where(cd => cd.CourseId == courseId)
                .ToListAsync();

            var existingSubjectIds = existingAssociations.Select(cd => cd.SubjectId).ToList();

            var toAdd = SelectedSubjectIds.Except(existingSubjectIds).ToList();

            var toRemove = existingSubjectIds.Except(SelectedSubjectIds).ToList();

            try
            {
                foreach (var subjectId in toAdd)
                {
                    var newRelation = new CourseDisciplines
                    {
                        CourseId = courseId,
                        SubjectId = subjectId
                    };

                    await _courseDisciplinesRepository.CreateAsync(newRelation);
                }

                foreach (var subjectId in toRemove)
                {
                    var relation = existingAssociations.FirstOrDefault(cd => cd.SubjectId == subjectId);
                    if (relation != null)
                    {
                        await _courseDisciplinesRepository.DeleteAsync(relation);
                    }
                }

                TempData["ToastSuccess"] = "Subjects updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "An error occurred while updating subjects.";
                return RedirectToAction(nameof(ManageSubjects), new { courseId });
            }
        }

        [Authorize(Roles = "Secretary")]
        [HttpGet]
        public async Task<IActionResult> Details(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course == null)
            {
                TempData["ToastError"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var assignedSubjectIds = await _courseDisciplinesRepository.GetAll()
                .Where(cd => cd.CourseId == courseId)
                .Select(cd => cd.SubjectId)
                .ToListAsync();

            var assignedSubjects = await _subjectRepository.GetAll()
                .Where(s => assignedSubjectIds.Contains(s.Id))
                .OrderBy(s => s.Name)
                .ToListAsync();

            var model = new CourseDetailsViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Type = course.Type,
                Duration = course.Duration,
                IsActive = course.IsActive,
                AssignedSubjects = assignedSubjects.Select(s => new SubjectViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    Hours = s.Hours
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LoadCourses(string? searchTerm, string? type)
        {
            var query = _courseRepository.GetAll().Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerTerm = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lowerTerm));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                var lowerType = type.Trim().ToLower();
                query = query.Where(c => c.Type.ToLower() == lowerType);
            }

            var courses = await query.OrderBy(c => c.Name).ToListAsync();

            return PartialView("_CourseListPartial", courses);
        }

        [HttpGet]
        public async Task<IActionResult> LoadSubjects(int courseId, string? searchTerm, string filterBy = "name")
        {
            var allSubjects = _subjectRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                if (filterBy == "code" && int.TryParse(term, out int code))
                {
                    allSubjects = allSubjects.Where(s => s.Code.ToString().StartsWith(term));
                }
                else if (filterBy == "name")
                {
                    allSubjects = allSubjects.Where(s => s.Name.ToLower().StartsWith(term));
                }
            }

            var subjectsList = await allSubjects
                .OrderBy(s => s.Name)
                .ToListAsync();

            var assignedSubjectIds = await _courseDisciplinesRepository
                .GetAll()
                .Where(cd => cd.CourseId == courseId)
                .Select(cd => cd.SubjectId)
                .ToListAsync();

            var viewModelList = subjectsList.Select(subject => new SubjectAssignmentViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Code = subject.Code,
                Hours = subject.Hours,
                IsAssigned = assignedSubjectIds.Contains(subject.Id)
            }).ToList();

            int maxHour = 0;

            foreach (int SubjectId in assignedSubjectIds)
            {
                var assignedSubject = await _subjectRepository.GetByIdAsync(SubjectId);

                maxHour += assignedSubject.Hours;
            }

            ViewBag.AssignedHours = maxHour;

            return PartialView("_SubjectListPartial", viewModelList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSubject([FromBody] ToggleSubjectViewModel model)
        {
            if (model == null || model.CourseId <= 0 || model.SubjectId <= 0)
            {
                return BadRequest("There is a problem with the model.");
            }

            var course = await _courseRepository.GetByIdAsync(model.CourseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var subject = await _subjectRepository.GetByIdAsync(model.SubjectId);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            var assignedSubjectIds = (_courseDisciplinesRepository.GetAll().Where(cd => cd.CourseId == model.CourseId))
                                        .Select(cd => cd.SubjectId)
                                        .ToHashSet();

            var existing = _courseDisciplinesRepository.GetAll().FirstOrDefault(cd =>
                cd.CourseId == model.CourseId && cd.SubjectId == model.SubjectId);

            int maxHour = 0;

            foreach (int SubjectId in assignedSubjectIds) //Checking if the max duration is being exceeded by adding a new subject
            {
                var assignedSubject = await _subjectRepository.GetByIdAsync(SubjectId);

                maxHour += assignedSubject.Hours;
            }

            if (model.Assign)
            {
                if (existing == null)
                {
                    maxHour += subject.Hours;

                    if (maxHour <= course.Duration)
                    {
                        await _courseDisciplinesRepository.CreateAsync(new CourseDisciplines
                        {
                            CourseId = model.CourseId,
                            SubjectId = model.SubjectId
                        });
                        ViewBag.AssignedHours = maxHour;
                    }
                    else
                    {
                        TempData["ToastError"] = "The maximum duration of the course has been exceeded";
                        ViewBag.AssignedHours = maxHour - subject.Hours;
                    }
                }
            }
            else
            {
                if (existing != null)
                {
                    ViewBag.AssignedHours = maxHour - subject.Hours;
                    await _courseDisciplinesRepository.DeleteAsync(existing);
                }
            }

            assignedSubjectIds = (_courseDisciplinesRepository.GetAll().Where(cd => cd.CourseId == model.CourseId))
                                        .Select(cd => cd.SubjectId)
                                        .ToHashSet();

            var updatedSubjects = await _subjectRepository.GetAll().ToListAsync();

            var subjectViewModels = updatedSubjects.Select(s => new SubjectAssignmentViewModel
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Hours = s.Hours,
                IsAssigned = assignedSubjectIds.Contains(s.Id)
            }).ToList();
            return PartialView("_SubjectListPartial", subjectViewModels);
        }


    }
}
