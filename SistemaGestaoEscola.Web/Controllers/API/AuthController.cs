using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;
using SistemaGestaoEscola.Web.Models.API;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SistemaGestaoEscola.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly UserManager<User> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthController> _logger;
        private readonly IMemoryCache _cache;

        private static string CacheKey(string email) => $"pwdreset:{email.ToLowerInvariant()}";

        public AuthController(
            IOptions<JwtSettings> jwtOptions,
            IUserHelper userHelper,
            IMailHelper mailHelper,
            UserManager<User> userManager,
            ILogger<AuthController> logger,
            IMemoryCache cache)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _userManager = userManager;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
            _cache = cache;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email e senha são obrigatórios." });

            var user = await _userHelper.GetUserByEmailAsync(request.Email);
            if (user == null || !await _userHelper.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new { message = "Credenciais inválidas." });

            if (!await _userHelper.IsEmailConfirmedAsync(user))
                return Unauthorized(new { message = "Confirme seu email antes de fazer login." });

            var roles = await _userHelper.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email)
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiration = token.ValidTo,
                user = new { user.Id, user.FullName, user.Email, roles, user.ProfilePicturePath}
            });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            var email = (req?.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Informe um email." });

            var user = await _userHelper.GetUserByEmailAsync(email);

            if (user == null)
                return Ok(new { message = "Se o email existir, enviaremos um código." });

            var code = GenerateSixDigitCode();

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            _cache.Set(CacheKey(email), code, cacheEntryOptions);

            var body = $@"
                <p>Olá{(string.IsNullOrWhiteSpace(user.FullName) ? "" : ", " + user.FullName)}!</p>
                <p>Seu código de redefinição de senha é:</p>
                <p style=""font-size:20px;font-weight:bold"">{code}</p>
                <p>Ele expira em 10 minutos.</p>
                <p>Se não foi você, ignore este e-mail.</p>";

            var mailResp = _mailHelper.SendEmail(email, "Código para redefinição de senha", body);
            if (!mailResp.IsSuccess)
                _logger.LogWarning("Falha ao enviar email de reset para {Email}: {Erro}", email, mailResp.Message);

            return Ok(new { message = "Se o email existir, enviaremos um código." });
        }

        private static string GenerateSixDigitCode()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes) % 1_000_000u;
            return value.ToString("D6");
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            var email = (req?.Email ?? "").Trim().ToLowerInvariant();
            var code = (req?.Code ?? "").Trim();
            var newPassword = (req?.NewPassword ?? "").Trim();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(newPassword))
                return BadRequest(new { message = "Email, código e nova senha são obrigatórios." });

            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
                return BadRequest(new { message = "Requisição inválida." });

            if (!_cache.TryGetValue(CacheKey(email), out string? cachedCode) || !string.Equals(cachedCode, code, StringComparison.Ordinal))
                return BadRequest(new { message = "Código inválido ou expirado." });

            var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, identityToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            _cache.Remove(CacheKey(email));

            await _userManager.UpdateSecurityStampAsync(user);

            return Ok(new { message = "Senha redefinida com sucesso." });
        }
    }
}
