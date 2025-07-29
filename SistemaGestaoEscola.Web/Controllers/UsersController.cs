using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Enums;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public UsersController(IUserHelper userHelper, IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? selectedRole, int page = 1)
        {
            int pageSize = 10;

            var users = await _userHelper.GetAllUsersAsync();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var role = (await _userHelper.GetRolesAsync(user)).FirstOrDefault() ?? "Sem função";
                if ((string.IsNullOrWhiteSpace(searchTerm) || user.FullName.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(selectedRole) || role == selectedRole))
                {
                    userRoles.Add(new UserRoleViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = role
                    });
                }
            }

            int totalUsers = userRoles.Count;
            var pagedUsers = userRoles.Skip((page - 1) * pageSize).Take(pageSize);
            var allRoles = Enum.GetNames(typeof(UserRole)).ToList();

            var model = new UserListViewModel
            {
                Users = pagedUsers,
                SearchTerm = searchTerm,
                SelectedRole = selectedRole,
                Roles = allRoles,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize)
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (model.Role == UserRole.Student.ToString() && model.RegistrationPhoto == null)
            {
                ModelState.AddModelError("RegistrationPhoto", "Foto de registro é obrigatória para estudantes.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Erro ao criar usuário.";
                ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
                return View(model);
            }

            try
            {
                var existingUser = await _userHelper.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    TempData["ToastError"] = "Este e-mail já está registrado.";
                    ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email
                };

                var generatedPassword = GenerateRandomPassword();
                var result = await _userHelper.AddUserAsync(user, generatedPassword);

                if (!result.Succeeded)
                {
                    TempData["ToastError"] = result.Errors.FirstOrDefault()?.Description ?? "Erro desconhecido.";
                    ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
                    return View(model);
                }

                await _userHelper.AddUserToRoleAsync(user, model.Role);

                if (model.Role == UserRole.Student.ToString() && model.RegistrationPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.RegistrationPhoto.FileName)}";
                    var folder = Path.Combine("wwwroot", "images", "registration");
                    Directory.CreateDirectory(folder);
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.RegistrationPhoto.CopyToAsync(stream);
                    }

                    user.RegistrationPhotoPath = $"/images/registration/{fileName}";
                    await _userHelper.UpdateUserAsync(user);
                }

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { email = user.Email, token }, protocol: HttpContext.Request.Scheme);

                var emailBody = $@"<h3>Welcome to ETEMB</h3>
                    <p>Hello {user.FirstName},</p>
                    <p>Your password is: {generatedPassword}</p>
                    <p>Your account was created successfully. Click below to activate it:</p>
                    <p><a href='{confirmationLink}'>Confirm Account</a></p>";

                var emailResult = _mailHelper.SendEmail(user.Email, "Account Activation", emailBody);

                TempData["ToastSuccess"] = emailResult.IsSuccess
                    ? "Usuário criado e e-mail enviado."
                    : "Usuário criado, mas erro ao enviar e-mail.";
            }
            catch (Exception ex)
            {
                TempData["ToastError"] = $"Erro inesperado: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userHelper.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? "";

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = currentRole,
                RegistrationPhotoPath = user.RegistrationPhotoPath
            };

            ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (model.Role == UserRole.Student.ToString() && model.RegistrationPhoto == null && string.IsNullOrEmpty(model.RegistrationPhotoPath))
            {
                ModelState.AddModelError("RegistrationPhoto", "Foto de registro é obrigatória para estudantes.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Erro ao editar usuário.";
                ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
                return View(model);
            }

            try
            {
                var user = await _userHelper.GetUserByIdAsync(model.Id);
                if (user == null)
                {
                    TempData["ToastError"] = "Usuário não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;

                var currentRoles = await _userHelper.GetRolesAsync(user);
                var currentRole = currentRoles.FirstOrDefault();

                if (currentRole != model.Role)
                {
                    if (!string.IsNullOrEmpty(currentRole))
                    {
                        await _userHelper.RemoveFromRoleAsync(user, currentRole);
                    }
                    await _userHelper.AddUserToRoleAsync(user, model.Role);
                }

                if (model.Role == UserRole.Student.ToString() && model.RegistrationPhoto != null)
                {
                    if (!string.IsNullOrEmpty(user.RegistrationPhotoPath))
                    {
                        var oldPath = Path.Combine("wwwroot", user.RegistrationPhotoPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.RegistrationPhoto.FileName)}";
                    var folder = Path.Combine("wwwroot", "images", "registration");
                    Directory.CreateDirectory(folder);
                    var filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await model.RegistrationPhoto.CopyToAsync(stream);

                    user.RegistrationPhotoPath = $"/images/registration/{fileName}";
                }

                var result = await _userHelper.UpdateUserAsync(user);

                TempData["ToastSuccess"] = result.Succeeded
                    ? "Usuário atualizado com sucesso."
                    : "Erro ao atualizar usuário.";
            }
            catch (Exception ex)
            {
                TempData["ToastError"] = $"Erro inesperado ao editar: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ToastError"] = "Usuário não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (!string.IsNullOrEmpty(user.RegistrationPhotoPath))
                {
                    var registrationPath = Path.Combine("wwwroot", user.RegistrationPhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(registrationPath))
                    {
                        System.IO.File.Delete(registrationPath);
                    }
                }

                if (!string.IsNullOrEmpty(user.DisplayProfilePicturePath))
                {
                    var profilePath = Path.Combine("wwwroot", user.DisplayProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(profilePath))
                    {
                        System.IO.File.Delete(profilePath);
                    }
                }

                await _userHelper.DeleteUserAsync(user);
                TempData["ToastSuccess"] = "Usuário excluído com sucesso.";
            }
            catch
            {
                TempData["ToastError"] = "Erro ao excluir usuário, retire o usuário da turma e tente novamente.";
            }

            return RedirectToAction(nameof(Index));
        }
        private string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
