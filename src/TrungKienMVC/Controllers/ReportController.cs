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
            var start = model.StartDate!.Value.Date;
            var end = model.EndDate!.Value.Date;
            model.Report = _newsService.GetReport(start, end);

            // Same-length window immediately before this one, so the total can be read
            // against what "normal" looked like for the period before.
            var length = (end - start).Days + 1;
            var previousEnd = start.AddDays(-1);
            var previousStart = previousEnd.AddDays(-(length - 1));
            model.PreviousPeriodTotal = _newsService.CountNewsArticlesByPeriod(previousStart, previousEnd);
        }
        return View(model);
    }
}
