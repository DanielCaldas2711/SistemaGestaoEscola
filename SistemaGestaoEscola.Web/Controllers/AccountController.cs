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
                    TempData["ToastSuccess"] = "Welcome!";
                    return RedirectToAction("Index", "Home");
                }                
            }

            TempData["ToastError"] = "Login error.";
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogOutAsync();

            TempData["ToastSuccess"] = "Logged out!";
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
                TempData["ToastError"] = "There was a problem.";
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ToastError"] = "Invalid email.";
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
                TempData["ToastSuccess"] = "Password reset insctructions sent to email.";
                return RedirectToAction("Index","Home");
            }

            TempData["ToastError"]= "Error defining new password.";

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PasswordReset(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ToastError"] = "Problems with token and email.";
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
                TempData["ToastError"] = "There was an error.";
            }

            var user = await _userHelper.GetUserByEmailAsync(model.UserName);
            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    TempData["ToastSuccess"] = "Password changed successfully.";
                    return View();
                }

                TempData["ToastError"] = "Error changing the password.";
                return View(model);
            }

            TempData["ToastError"] = "User not found.";
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
                TempData["ToastError"] = "There was an error.";
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                if (model.ProfilePicture.Length > 2 * 1024 * 1024) //Checks the size of the file
                {
                    TempData["ToastError"] = "The profile picture must be 2MB or less.";
                    return View(model);
                }

                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedContentTypes.Contains(model.ProfilePicture.ContentType)) //Makes sure that the file is a image
                {
                    TempData["ToastError"] = "Only JPEG, PNG or WEBP images are allowed.";
                    return View(model);
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profilePictures");
                Directory.CreateDirectory(uploadsFolder); // Makes sure the directory exists

                if (!string.IsNullOrEmpty(user.ProfilePicturePath) &&
                    !user.ProfilePicturePath.Contains("/images/defaultProfilePicture/"))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string uniqueFileName = $"{Guid.NewGuid()}_{model.ProfilePicture.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create)) //TODO Upload images to blob
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePicturePath = $"/images/profilePictures/{uniqueFileName}";
            }

            await _userHelper.UpdateUserAsync(user);

            TempData["ToastSuccess"] = "Profile updated successfully";
            return RedirectToAction("EditProfile");
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ToastError"] = "Invalid activation link and token.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
            {
                TempData["ToastError"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["ToastSuccess"] = "Email successfully confirmed!";
            }
            else
            {
                TempData["ToastError"] = "Error confirming email.";
            }

            return RedirectToAction("Index", "Home");
        }


    }
}
