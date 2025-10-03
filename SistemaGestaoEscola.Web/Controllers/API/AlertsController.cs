using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.DTOs;
using System.Security.Claims;

namespace SistemaGestaoEscola.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertRepository _alertRepository;
        private readonly UserManager<User> _userManager;

        public AlertsController(IAlertRepository alertRepository, UserManager<User> userManager)
        {
            _alertRepository = alertRepository;
            _userManager = userManager;
        }

        private string? GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        /// <summary>
        /// Retorna possíveis destinatários de alertas para o usuário atual.
        /// Se o usuário estiver na role "Student", retorna todos os Admins (UserId, FullName).
        /// Caso contrário, retorna lista vazia (ajuste para Forbid se preferir).
        /// </summary>
        [HttpGet("recipients")]
        public async Task<ActionResult<IEnumerable<AlertUserDto>>> GetRecipients(CancellationToken ct)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser is null)
                return Unauthorized();

            var isStudent = await _userManager.IsInRoleAsync(currentUser, "Student");
            if (!isStudent)
            {
                return Ok(new { message = "Ainda não implementado para sua função." });
            }

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var result = admins
                .Select(u => new AlertUserDto
                {
                    UserId = u.Id,
                    FullName = string.IsNullOrWhiteSpace(u.FullName) ? u.UserName ?? u.Id : u.FullName
                })
                .OrderBy(x => x.FullName)
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Cria um alerta do usuário autenticado (remetente) para um destinatário.
        /// Ignora o FromUserId enviado no corpo e usa o ID do usuário logado.
        /// </summary>
        /// <remarks>
        /// Body esperado (exemplo):
        /// {
        ///   "title": "Atualização de notas",
        ///   "description": "Suas notas foram atualizadas.",
        ///   "toUserId": "GUID-ou-id-do-destinatário"
        /// }
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AlertDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var fromUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(fromUserId))
                return Unauthorized();

            var entity = new Alert
            {
                Title = dto.Title?.Trim() ?? string.Empty,
                Description = dto.Description?.Trim() ?? string.Empty,
                FromUserId = fromUserId,
                ToUserId = dto.ToUserId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.CreateAsync(entity);

            return Created(string.Empty, null);
        }
    }
}
