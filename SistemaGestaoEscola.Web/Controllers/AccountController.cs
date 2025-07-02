using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;

        public AccountController(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
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
                    return RedirectToAction("Index", "Home");
                }                
            }

            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}
