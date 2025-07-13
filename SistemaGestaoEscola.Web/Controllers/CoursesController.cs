using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Enums;
using SistemaGestaoEscola.Web.Data.Repositories;
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
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            const int pageSize = 10;

            var allCourses = await _courseRepository.GetAll().ToListAsync();

            var filtered = string.IsNullOrWhiteSpace(searchTerm)
                ? allCourses
                : allCourses.Where(c =>
                       c.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase));

            int totalCourses = filtered.Count();
            var pagedCourses = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new CourseListViewModel
            {
                Courses = pagedCourses,
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCourses / (double)pageSize)
            };

            return View(model);
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
                TempData["ToastError"] = "Invalid form data.";
                ViewBag.Shifts = Enum.GetNames(typeof(Shift)).ToList();
                return View(model);
            }

            var existing = await _courseRepository.GetAll()
                .FirstOrDefaultAsync(c => c.Name == model.Name);

            if (existing != null)
            {
                TempData["ToastError"] = $"A course with the name \"{model.Name}\" already exists.";
                ViewBag.Shifts = Enum.GetNames(typeof(Shift)).ToList();
                return View(model);
            }
        
            model.TotalTime = CalculateWorkingHours(model.StartingDate, model.EndingDate, model.Shift);

            try
            {
                await _courseRepository.CreateAsync(model);
                TempData["ToastSuccess"] = "Course created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "An error occurred while creating the course.";
                ViewBag.Shifts = Enum.GetNames(typeof(Shift)).ToList();
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

            ViewBag.Shifts = Enum.GetNames(typeof(Shift)).ToList();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Course model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Invalid form data.";
                return View(model);
            }

            var existing = await _courseRepository.GetByIdAsync(model.Id);

            if (existing == null)
            {
                TempData["ToastError"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var duplicate = await _courseRepository.GetAll()
                .AnyAsync(c => c.Name == model.Name && c.Id != model.Id);

            if (duplicate)
            {
                TempData["ToastError"] = $"Another course with the name \"{model.Name}\" already exists.";
                return View(model);
            }

            try
            {
                existing.Name = model.Name;
                existing.Area = model.Area;
                existing.Shift = model.Shift;
                existing.StartingDate = model.StartingDate;
                existing.EndingDate = model.EndingDate;
                existing.TotalTime = CalculateWorkingHours(model.StartingDate, model.EndingDate, model.Shift);

                await _courseRepository.UpdateAsync(existing);

                TempData["ToastSuccess"] = "Course updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ToastError"] = "An error occurred while updating the course.";
                return View(model);
            }
        }


        private static int CalculateWorkingHours(DateTime start, DateTime end, Shift shift)
        {
            if (start >= end)
                return 0;

            int dailyHours = shift == Shift.Diurno ? 7 : 4;
            int totalHours = 0;

            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    totalHours += dailyHours;
                }
            }

            return totalHours;
        }
    }
}
