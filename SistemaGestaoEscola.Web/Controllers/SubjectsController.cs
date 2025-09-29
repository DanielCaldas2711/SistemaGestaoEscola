using Microsoft.AspNetCore.Mvc;
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
                TempData["ToastError"] = "Houve um erro no formulário.";
                return View(model);
            }

            var exists = await _subjectRepository.GetByCodeAsync(model.Code);
            if (exists != null)
            {
                TempData["ToastError"] = $"Uma disciplina com esse código já existe: \n {exists.Code} - {exists.Name}";
                return View(model);
            }

            try
            {
                await _subjectRepository.CreateAsync(model);

                TempData["ToastSuccess"] = "Disciplina criada com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Falha ao criar disciplina.";
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
                TempData["ToastError"] = "Disciplina não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _subjectRepository.DeleteAsync(subject);
                TempData["ToastSuccess"] = "Disciplina apagada com sucesso.";
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Ocorreu um erro ao apagar a disciplina.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);

            if (subject == null)
            {
                TempData["ToastError"] = "Disciplina não encontrada.";
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
                TempData["ToastError"] = "Favor corrigir os error do formulário.";
                return View(model);
            }

            var existingSubject = await _subjectRepository.GetByIdAsync(model.Id);
            if (existingSubject == null)
            {
                TempData["ToastError"] = "Disciplina não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (existingSubject.Code != model.Code)
            {
                var subjectWithSameCode = await _subjectRepository.GetByCodeAsync(model.Code);
                if (subjectWithSameCode != null)
                {
                    TempData["ToastError"] = $"Outra disciplina com esse código já existe: {subjectWithSameCode.Code} - {subjectWithSameCode.Name}";
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
                TempData["ToastSuccess"] = "Disciplina atualizada com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Ocorreu um erro ao atualizar a disciplina.";
                return View(model);
            }
        }

    }
}
