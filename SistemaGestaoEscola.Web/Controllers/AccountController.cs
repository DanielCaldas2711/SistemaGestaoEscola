using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IBlobHelper _blobHelper;

        public AccountController(IUserHelper userHelper,
            IMailHelper mailHelper,
            IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _blobHelper = blobHelper;
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
                try
                {
                    var result = await _userHelper.LoginAsync(model);

                    if (result.Succeeded)
                    {
                        TempData["ToastSuccess"] = "Bem vindo!";
                        if (User.IsInRole("Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Exception)
                {
                    TempData["ToastError"] = "Houve um problema ao fazer login.";
                    return View(model);
                }           
            }
            TempData["ToastError"] = "Credencias erradas.";
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _userHelper.LogOutAsync();

                TempData["ToastSuccess"] = "Até a próxima!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Ocorreu um erro ao fazer login.";
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Houve um problema.";
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ToastError"] = "Email inválido.";
                return View(model);
            }

            try
            {
                var recoverToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                var link = Url.Action(
                    "PasswordReset", "Account",
                    new { token = recoverToken, email = user.Email },
                    protocol: HttpContext.Request.Scheme);

                var response = _mailHelper.SendEmail(user.Email, "Redefinição de palavra passe", $"<h1>Redefinição de palavra passe</h1>" +
                    $"Para redefinir a palavra passe, acesse esse link:</br></br>" +
                    $"<a href = \"{link}\">Reset Password</a>");

                if (response.IsSuccess)
                {
                    TempData["ToastSuccess"] = "Instruções para redifinir a sua senha foram enviadas para o email.";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Ocorreu um erro ao definir nova palavra passe.";
            }
            return View();
        }

        [HttpGet]
        public IActionResult PasswordReset(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ToastError"] = "Houve um problema com o Token e Email.";
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
                TempData["ToastError"] = "Houve um erro.";
            }

            var user = await _userHelper.GetUserByEmailAsync(model.UserName);
            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    TempData["ToastSuccess"] = "Palavra passe alterada com sucesso.";
                    return View();
                }

                TempData["ToastError"] = "Ocorreu um erro ao alterar a palavra passe.";
                return View(model);
            }

            TempData["ToastError"] = "Usuário não encontrado.";
            return View(model);
        }

        public async Task<IActionResult> EditProfile()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExistingPicturePath = user.DisplayProfilePicturePath
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastError"] = "Houve um erro.";
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                TempData["ToastError"] = "Usuário não encontrado.";
                return RedirectToAction("EditProfile");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                if (model.ProfilePicture.Length > 2 * 1024 * 1024)
                {
                    TempData["ToastError"] = "A foto de perfil precisa ter 2MB ou menos.";
                    return View(model);
                }

                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedContentTypes.Contains(model.ProfilePicture.ContentType))
                {                    
                    TempData["ToastError"] = "Apenas são permitidas imagens nos formatos: JPEG, PNG ou WEBP.";
                    return View(model);
                }

                try
                {
                    if (!string.IsNullOrEmpty(user.ProfilePicturePath) &&
                                !user.ProfilePicturePath.Contains("/images/defaultProfilePicture/"))
                    {
                        var oldBlobName = Path.GetFileName(user.ProfilePicturePath);
                        await _blobHelper.DeleteBlobAsync(oldBlobName, "profilepictures");
                    }

                    var blobName = await _blobHelper.UploadBlobAsync(model.ProfilePicture, "profilepictures");
                    user.ProfilePicturePath = $"https://sistemagestaoescola.blob.core.windows.net/profilepictures/{blobName}";
                }
                catch (Exception)
                {
                    TempData["ToastError"] = "Ocorreu um problema ao atualizar foto de perfil.";
                }
            }

            await _userHelper.UpdateUserAsync(user);

            TempData["ToastSuccess"] = "Perfil atualizado com sucesso.";
            return RedirectToAction("EditProfile");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ToastError"] = "Token de ativação e link inválidos.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                TempData["ToastError"] = "Usuário não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                var recoverToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                TempData["ToastSuccess"] = "Email confirmado! Favor definir uma nova password.";

                return RedirectToAction("PasswordReset", "Account", new { token = recoverToken, email = user.Email });
            }
            else
            {
                TempData["ToastError"] = "Ocorreu um erro ao confirmar o email.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
