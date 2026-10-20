using System.Diagnostics;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;

namespace TrungKienMVC.Controllers;

public class HomeController : Controller
{
    /// <summary>Sends the user to the start page of his/her role.</summary>
    public IActionResult Index()
    {
        if (User.IsInRole(AccountRoles.AdminName)) return RedirectToAction("Index", "SystemAccounts");
        if (User.IsInRole(AccountRoles.StaffName)) return RedirectToAction("Index", "NewsArticles");
        if (User.IsInRole(AccountRoles.LecturerName)) return RedirectToAction("Index", "News");
        return RedirectToAction("Login", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
