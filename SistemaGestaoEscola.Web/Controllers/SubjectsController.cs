using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly ISubjectRepository _subjectRepository;

        public SubjectsController(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            const int pageSize = 10;

            var allSubjects = await _subjectRepository.GetAll().ToListAsync();

            var filteredSubjects = string.IsNullOrWhiteSpace(searchTerm)
                ? allSubjects
                : allSubjects.Where(s => s.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase));

            var totalSubjects = filteredSubjects.Count();

            var pagedSubjects = filteredSubjects
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new SubjectListViewModel
            {
                Subjects = pagedSubjects,
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalSubjects / (double)pageSize)
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
        public async Task<IActionResult> Create(Subject model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "There was an error in the form.";
                return View(model);
            }

            var exists = await _subjectRepository.GetByCodeAsync(model.Code);
            if (exists != null)
            {
                TempData["ToastError"] = $"A subject with this code already exists: \n {exists.Code} - {exists.Name}";
                return View(model);
            }

            try
            {
                await _subjectRepository.CreateAsync(model);   
                
                TempData["ToastSuccess"] = "Subject created successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ToastError"] = "Failed to create subject.";
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
            {
                TempData["ToastError"] = "Subject not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _subjectRepository.DeleteAsync(subject);
                TempData["ToastSuccess"] = "Subject deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ToastError"] = "An error occurred while trying to delete the subject.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);

            if (subject == null)
            {
                TempData["ToastError"] = "Subject not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subject model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Please correct the errors in the form.";
                return View(model);
            }

            var existingSubject = await _subjectRepository.GetByIdAsync(model.Id);
            if (existingSubject == null)
            {
                TempData["ToastError"] = "Subject not found.";
                return RedirectToAction(nameof(Index));
            }

            if (existingSubject.Code != model.Code)
            {
                var subjectWithSameCode = await _subjectRepository.GetByCodeAsync(model.Code);
                if (subjectWithSameCode != null)
                {
                    TempData["ToastError"] = $"Another subject with this code already exists: {subjectWithSameCode.Code} - {subjectWithSameCode.Name}";
                    return View(model);
                }
            }

            existingSubject.Code = model.Code;
            existingSubject.Name = model.Name;
            existingSubject.Hours = model.Hours;
            existingSubject.Absence = model.Absence;

            try
            {
                await _subjectRepository.UpdateAsync(existingSubject);
                TempData["ToastSuccess"] = "Subject updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "An error occurred while updating the subject.";
                return View(model);
            }
        }

    }
}
