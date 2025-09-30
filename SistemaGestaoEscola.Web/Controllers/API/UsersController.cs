using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models.API;
using System;
using System.Security.Claims;

namespace SistemaGestaoEscola.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IBlobHelper _blobHelper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            IBlobHelper blobHelper,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _blobHelper = blobHelper;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "Sessão inválida." });

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                profilePicturePath = user.ProfilePicturePath
            });
        }

        [HttpPut("me/profile")]
        [RequestSizeLimit(20_000_000)] // 20 MB
        public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateProfileRequest req)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "Sessão inválida." });

            var changed = false;

            // 1) Nome
            if (!string.IsNullOrWhiteSpace(req.FullName))
            {
                var parts = req.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

                user.FirstName = parts[0];
                user.LastName = parts.Length > 1 ? parts[1] : "";
                changed = true;
            }

            // 2) Foto
            if (req.ProfileImage is not null && req.ProfileImage.Length > 0)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(user.ProfilePicturePath))
                    {
                        var uri = new Uri(user.ProfilePicturePath);
                        var blobName = Path.GetFileName(uri.LocalPath);
                        await _blobHelper.DeleteBlobAsync(blobName, "profilepictures");
                    }                       

                    var newUrl = await _blobHelper.UploadBlobAsync(req.ProfileImage, "profilepictures");
                    user.ProfilePicturePath = $"https://blobgestaoescola.blob.core.windows.net/profilepictures/{newUrl}";
                    changed = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar nova foto de perfil para o usuário {UserId}", user.Id);
                    return BadRequest(new { message = "Falha ao enviar a imagem." });
                }
            }

            // 3) Senha
            if (!string.IsNullOrWhiteSpace(req.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var reset = await _userManager.ResetPasswordAsync(user, token, req.NewPassword);
                if (!reset.Succeeded)
                    return BadRequest(new { message = string.Join("; ", reset.Errors.Select(e => e.Description)) });

                await _userManager.UpdateSecurityStampAsync(user);
                changed = true;
            }

            // 4) Persiste
            if (changed)
            {
                var update = await _userManager.UpdateAsync(user);
                if (!update.Succeeded)
                    return BadRequest(new { message = string.Join("; ", update.Errors.Select(e => e.Description)) });
            }

            return Ok(new
            {
                fullName = user.FullName ?? string.Empty,
                profilePicturePath = user.ProfilePicturePath ?? string.Empty,
                message = "Perfil atualizado."
            });
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
                return await _userManager.FindByIdAsync(userId);

            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(email))
                return await _userManager.FindByEmailAsync(email);

            return null;
        }
    }
}
