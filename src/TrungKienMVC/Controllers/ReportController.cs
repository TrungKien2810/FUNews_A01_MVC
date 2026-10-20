using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using TrungKienMVC.ViewModels;

namespace TrungKienMVC.Controllers;

/// <summary>Admin: statistic report of news articles by created date in a period, sorted descending.</summary>
[Authorize(Roles = AccountRoles.AdminName)]
public class ReportController : Controller
{
    private readonly INewsArticleService _newsService;

    public ReportController(INewsArticleService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    public IActionResult Index(ReportViewModel model)
    {
        if (!model.StartDate.HasValue && !model.EndDate.HasValue)
        {
            // First visit: show the current month without validation errors.
            ModelState.Clear();
            var today = DateTime.Today;
            model.StartDate = new DateTime(today.Year, today.Month, 1);
            model.EndDate = today;
            return View(model);
        }

        if (ModelState.IsValid)
        {
            model.Report = _newsService.GetReport(model.StartDate!.Value, model.EndDate!.Value);
        }
        return View(model);
    }
}
