using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;

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
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Course model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Favor corrigir erros do formulário.";
                return View(model);
            }

            var exists = await _courseRepository.GetAll()
                .AnyAsync(c => c.Name.ToLower() == model.Name.ToLower());

            if (exists)
            {
                TempData["ToastError"] = $"O curso de nome \"{model.Name}\" já existe.";
                return View(model);
            }

            model.IsActive = true; // When a course is created, it is always active

            try
            {
                await _courseRepository.CreateAsync(model);
                TempData["ToastSuccess"] = "Curso criado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Ocorreu um erro ao criar o curso.";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                TempData["ToastError"] = "Curso não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _courseRepository.DeleteAsync(course);
                TempData["ToastSuccess"] = "Curso apagado com sucesso.";
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Ocorreu um erro ao apagar o curso.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                TempData["ToastError"] = "Curso não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Course model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Existem erros de validção no formulário.";
                return View(model);
            }

            var existing = await _courseRepository.GetByIdAsync(model.Id);
            if (existing == null)
            {
                TempData["ToastError"] = "Curso não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var duplicate = await _courseRepository.GetAll()
                .Where(c => c.Id != model.Id && c.Name.ToLower() == model.Name.ToLower())
                .FirstOrDefaultAsync();

            if (duplicate != null)
            {
                TempData["ToastError"] = $"O curso com o nome \"{model.Name}\" já existe.";
                return View(model);
            }

            try
            {
                existing.Name = model.Name;
                existing.Type = model.Type;
                existing.Duration = model.Duration;
                existing.IsActive = model.IsActive;

                await _courseRepository.UpdateAsync(existing);
                TempData["ToastSuccess"] = "Curso atualizado com sucesso";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Ocorreu um erro ao atualizar o curso.";
                return View(model);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LoadCourses(string? searchTerm, string? typeFilter, bool? isActiveFilter, int page = 1)
        {
            const int pageSize = 10;
            var query = _courseRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().StartsWith(term));
            }

            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                query = query.Where(c => c.Type == typeFilter);
            }

            if (isActiveFilter.HasValue)
            {
                query = query.Where(c => c.IsActive == isActiveFilter.Value);
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
                TypeFilter = typeFilter,
                IsActiveFilter = isActiveFilter,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCourses / (double)pageSize)
            };

            return PartialView("_CourseTablePartial", model);
        }

    }
}
