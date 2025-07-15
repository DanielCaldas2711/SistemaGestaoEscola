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

        public UsersController(IUserHelper userHelper,
            IMailHelper mailHelper)
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
                if (
                    (string.IsNullOrWhiteSpace(searchTerm) || user.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(selectedRole) || role == selectedRole)
                )
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

            int totalUsers = userRoles.Count();
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
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "There was an error.";
                ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
                return View(model);
            }

            var existingUser = await _userHelper.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                TempData["ToastError"] = "This email is already registered";
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

            var result = await _userHelper.AddUserAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    TempData["ToastError"] = $"Error: {error.Description}"; //TODO Change to a single error
                }

                ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
                return View(model);
            }

            await _userHelper.AddUserToRoleAsync(user, model.Role);

            var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new { email = user.Email, token = token },
                protocol: HttpContext.Request.Scheme);

            var emailBody = $@"
            <h3>Welcome to ETEMB</h3>
            <p>Hello {user.FirstName},</p>
            <p>Your account was created  successfully. To activate it, click in the link down bellow:</p>
            <p><a href='{confirmationLink}'>Confirm Account</a></p>";

            var response = _mailHelper.SendEmail(user.Email, "Account Activation", emailBody);

            if (response.IsSuccess)
            {
                TempData["ToastSuccess"] = "User created successfully and activation email sent!";
            }
            else
            {
                TempData["ToastError"] = "User created successfully but there was a problem with the email.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userHelper.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? "";

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = currentRole
            };

            ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Invalid form submission.";
                ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
                return View(model);
            }

            var user = await _userHelper.GetUserByIdAsync(model.Id);
            if (user == null)
            {
                TempData["ToastError"] = "User not found.";
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
                    var response = await _userHelper.RemoveFromRoleAsync(user, currentRole);

                    if (!response.Succeeded) 
                    {
                        TempData["ToastError"] = "Error when removing from role.";
                    }
                }

                await _userHelper.AddUserToRoleAsync(user, model.Role);
            }

            var result = await _userHelper.UpdateUserAsync(user);

            if (result.Succeeded)
            {
                TempData["ToastSuccess"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ToastError"] = "There was a problem updating the user.";
                ViewBag.Roles = Enum.GetNames(typeof(UserRole)).ToList();
                return View(model);
            }
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

            var result = await _userHelper.DeleteUserAsync(user);

            if (result.Succeeded)
            {
                TempData["ToastSuccess"] = "Usuário excluído com sucesso.";
            }
            else
            {
                TempData["ToastError"] = "Erro ao excluir usuário.";
            }                

            return RedirectToAction(nameof(Index));
        }     
    }
}
