using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;

        public CoursesController(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Please correct the form errors.";
                return View(model);
            }

            var exists = await _courseRepository.GetAll()
                .AnyAsync(c => c.Name.ToLower() == model.Name.ToLower());

            if (exists)
            {
                TempData["ToastError"] = $"A course named \"{model.Name}\" already exists.";
                return View(model);
            }

            try
            {
                await _courseRepository.CreateAsync(model);
                TempData["ToastSuccess"] = "Course created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "An error occurred while creating the course.";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                TempData["ToastError"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _courseRepository.DeleteAsync(course);
                TempData["ToastSuccess"] = "Course deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ToastError"] = "An error occurred while trying to delete the course.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                TempData["ToastError"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Course model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "There are validation errors in the form.";
                return View(model);
            }

            var existing = await _courseRepository.GetByIdAsync(model.Id);
            if (existing == null)
            {
                TempData["ToastError"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var duplicate = await _courseRepository.GetAll()
                .Where(c => c.Id != model.Id && c.Name.ToLower() == model.Name.ToLower())
                .FirstOrDefaultAsync();

            if (duplicate != null)
            {
                TempData["ToastError"] = $"A course with the name \"{model.Name}\" already exists.";
                return View(model);
            }

            try
            {
                existing.Name = model.Name;
                existing.Type = model.Type;
                existing.Duration = model.Duration;

                await _courseRepository.UpdateAsync(existing);
                TempData["ToastSuccess"] = "Course updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "An error occurred while updating the course.";
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> LoadCourses(string? searchTerm, int page = 1)
        {
            const int pageSize = 10;

            var query = _courseRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().StartsWith(term));
            }

            var totalCourses = await query.CountAsync();

            var pagedCourses = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new CourseListViewModel
            {
                Courses = pagedCourses,
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCourses / (double)pageSize)
            };

            return PartialView("_CourseTablePartial", model);
        }
    }
}
