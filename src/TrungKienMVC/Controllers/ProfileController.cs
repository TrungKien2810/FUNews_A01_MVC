using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services;
using TrungKienMVC.Infrastructure;
using TrungKienMVC.ViewModels;

namespace TrungKienMVC.Controllers;

/// <summary>Staff: view and update his/her own profile.</summary>
[Authorize(Roles = AccountRoles.StaffName)]
public class ProfileController : Controller
{
    private readonly IAccountService _accountService;
    private readonly AdminAccountOptions _adminAccount;

    public ProfileController(IAccountService accountService, IOptions<AdminAccountOptions> adminAccount)
    {
        _accountService = accountService;
        _adminAccount = adminAccount.Value;
    }

    public IActionResult Index()
    {
        var account = _accountService.GetAccountById(User.GetAccountId());
        if (account == null) return NotFound();
        return View(account);
    }

    [HttpGet]
    public IActionResult Edit()
    {
        var account = _accountService.GetAccountById(User.GetAccountId());
        if (account == null) return NotFound();
        return PartialView("_ProfileForm", new ProfileViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? string.Empty,
            AccountEmail = account.AccountEmail ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel model)
    {
        // A staff can only edit his/her own profile.
        var account = _accountService.GetAccountById(User.GetAccountId());
        if (account == null) return NotFound();
        model.AccountId = account.AccountId;

        if (!string.IsNullOrEmpty(model.CurrentPassword) && model.CurrentPassword != account.AccountPassword)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
        }
        if (!string.IsNullOrWhiteSpace(model.AccountEmail))
        {
            var email = model.AccountEmail.Trim();
            if (string.Equals(email, _adminAccount.Email, StringComparison.OrdinalIgnoreCase)
                || _accountService.IsEmailExisted(email, account.AccountId))
            {
                ModelState.AddModelError(nameof(model.AccountEmail), "This email is already used by another account.");
            }
        }
        if (!ModelState.IsValid)
        {
            return PartialView("_ProfileForm", model);
        }

        account.AccountName = model.AccountName.Trim();
        account.AccountEmail = model.AccountEmail.Trim();
        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            account.AccountPassword = model.NewPassword;
        }
        _accountService.UpdateAccount(account);

        // Refresh the cookie so the new name/email are shown immediately.
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            AccountController.CreatePrincipal(account.AccountId, account.AccountName, account.AccountEmail, AccountRoles.StaffName));

        TempData["Success"] = "Your profile was updated.";
        return Json(new { success = true });
    }
}
