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

    }
}
