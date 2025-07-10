using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {                
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);
                
                if (result.Succeeded) 
                {
                    TempData["ToastSuccess"] = "Login feito com sucesso.";
                    return RedirectToAction("Index", "Home");
                }                
            }

            TempData["ToastError"] = "Erro ao efetuar o login.";
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogOutAsync();

            TempData["ToastSuccess"] = "Logout feito com sucesso.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Erro ao redefinar a palavra passe.";
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ToastError"] = "Email inválido.";
                return View(model);
            }

            var recoverToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

            var link = Url.Action(
                "PasswordReset", "Account",
                new {token = recoverToken, email = user.Email}, 
                protocol: HttpContext.Request.Scheme);

            var response = _mailHelper.SendEmail(user.Email, "Password Reset",$"<h1>Password Reset</h1>" +
                $"To reset the password click in this link:</br></br>" +
                $"<a href = \"{link}\">Reset Password</a>");

            if (response.IsSuccess)
            {
                TempData["ToastSuccess"] = "Email para mudar a palavra passe foi enviado.";
                return RedirectToAction("Index","Home");
            }

            TempData["ToastError"]= "Erro ao redefinir palavra passe.";

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PasswordReset(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ToastError"] = "Problemas no token e email";
            }

            var model = new ResetPasswordViewModel
            {
                UserName = email,
                Token = token                
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PasswordReset(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                TempData["ToastError"] = "Erro com o ViewModel";
            }

            var user = await _userHelper.GetUserByEmailAsync(model.UserName);
            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    TempData["ToastSuccess"] = "Palavra passe mudada com sucesso!";
                    return View();
                }

                TempData["ToastError"] = "Erro ao mudar a palavra passe.";
                return View(model);
            }

            TempData["ToastError"] = "Usuário não encontrado.";
            return View(model);
        }

        public async Task<IActionResult> EditProfile()
        {
            return View();
        }

    }
}
