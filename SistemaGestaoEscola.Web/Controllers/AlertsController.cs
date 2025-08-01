﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

[Authorize]
public class AlertsController : Controller
{
    private readonly IAlertRepository _alertRepository;
    private readonly IUserHelper _userHelper;
    private readonly ITimeZoneHelper _timeZoneHelper;

    public AlertsController(
        IAlertRepository alertRepository,
        IUserHelper userHelper,
        ITimeZoneHelper timeZoneHelper)
    {
        _alertRepository = alertRepository;
        _userHelper = userHelper;
        _timeZoneHelper = timeZoneHelper;
    }

    [Authorize(Roles = "Secretary")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Secretary")]
    public async Task<IActionResult> Create(AlertViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var fromUser = await _userHelper.GetUserAsync(User);
        var admin = await _userHelper.GetAllUsersByRoleAsync("Admin");
        var adminUser = admin.FirstOrDefault();

        if (adminUser == null)
        {
            TempData["ToastError"] = "Nenhum administrador encontrado.";
            return View(model);
        }

        var alert = new Alert
        {
            Title = model.Title,
            Description = model.Description,
            FromUserId = fromUser.Id,
            ToUserId = adminUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _alertRepository.CreateAsync(alert);
        }
        catch (Exception)
        {
            TempData["ToastError"] = "Ocorreu um problema ao criar o alerta.";
            return RedirectToAction("Index", "Home");
        }

        TempData["ToastSuccess"] = "Alerta enviado ao administrador.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MyAlerts(string filter = "all", string sort = "desc", int page = 1, int pageSize = 10)
    {
        var user = await _userHelper.GetUserAsync(User);
        var alertsQuery = (await _alertRepository.GetForUserAsync(user.Id)).AsQueryable();

        alertsQuery = filter switch
        {
            "unread" => alertsQuery.Where(a => !a.IsRead),
            "read" => alertsQuery.Where(a => a.IsRead),
            _ => alertsQuery
        };

        alertsQuery = sort == "asc"
            ? alertsQuery.OrderBy(a => a.CreatedAt)
            : alertsQuery.OrderByDescending(a => a.CreatedAt);

        var totalItems = alertsQuery.Count();
        var alerts = alertsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        foreach (var alert in alerts)
        {
            alert.CreatedAt = _timeZoneHelper.ConvertUtcToLisbon(alert.CreatedAt);
        }

        ViewBag.CurrentFilter = filter;
        ViewBag.CurrentSort = sort;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return View(alerts);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var alert = await _alertRepository.GetByIdAsync(id);
        if (alert != null)
        {
            try
            {
                alert.IsRead = true;
                await _alertRepository.UpdateAsync(alert);
            }
            catch (Exception)
            {
                TempData["ToastError"] = "Ocorreu um prolema ao marcar como lido.";
            }          
        }
        return RedirectToAction("MyAlerts");
    }

    [Authorize(Roles = "Secretary")]
    [HttpGet]
    public async Task<IActionResult> CreateFromPopup()
    {
        var admins = await _userHelper.GetAllUsersByRoleAsync("Admin");
        var model = new AlertViewModel
        {
            Admins = admins.Select(a => new SelectListItem
            {
                Value = a.Id,
                Text = a.FullName
            }).ToList()
        };

        return PartialView("_AlertPopupForm", model);
    }

    [Authorize(Roles = "Secretary")]
    [HttpPost]
    public async Task<IActionResult> CreateFromPopup([FromBody] AlertViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Dados inválidos. Preencha todos os campos." });
        }

        var sender = await _userHelper.GetUserAsync(User);
        var receiver = await _userHelper.GetUserByIdAsync(model.ReceiverId);

        if (receiver == null)
        {
            return NotFound(new { message = "Administrador não encontrado." });
        }

        var alert = new Alert
        {
            Title = model.Title,
            Description = model.Description,
            CreatedAt = DateTime.UtcNow,
            FromUserId = sender.Id,
            ToUserId = receiver.Id,
            IsRead = false
        };

        try
        {
            await _alertRepository.CreateAsync(alert);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Ocorreu um problema ao enviar o alerta." });
        }
        return Ok(new { message = "Alerta enviado com sucesso." });
    }
}
