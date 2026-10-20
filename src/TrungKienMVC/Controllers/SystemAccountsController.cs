using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services;
using TrungKienMVC.Infrastructure;
using TrungKienMVC.ViewModels;

namespace TrungKienMVC.Controllers;

/// <summary>Admin: manage system accounts (CRUD + search). Create/Edit/Delete run in popup dialogs.</summary>
[Authorize(Roles = AccountRoles.AdminName)]
public class SystemAccountsController : Controller
{
    private readonly IAccountService _accountService;
    private readonly AdminAccountOptions _adminAccount;

    public SystemAccountsController(IAccountService accountService, IOptions<AdminAccountOptions> adminAccount)
    {
        _accountService = accountService;
        _adminAccount = adminAccount.Value;
    }

    public IActionResult Index(string? keyword, int? role)
    {
        var model = new AccountIndexViewModel
        {
            Keyword = keyword,
            Role = role,
            Accounts = _accountService.SearchAccounts(keyword, role)
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Create() => PartialView("_AccountForm", new AccountFormViewModel { AccountRole = AccountRoles.Staff });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AccountFormViewModel model)
    {
        if (string.IsNullOrEmpty(model.AccountPassword))
        {
            ModelState.AddModelError(nameof(model.AccountPassword), "Password is required.");
        }
        ValidateEmail(model);
        if (!ModelState.IsValid)
        {
            return PartialView("_AccountForm", model);
        }

        _accountService.AddAccount(new SystemAccount
        {
            AccountName = model.AccountName.Trim(),
            AccountEmail = model.AccountEmail.Trim(),
            AccountRole = model.AccountRole,
            AccountPassword = model.AccountPassword
        });
        TempData["Success"] = $"Account \"{model.AccountName}\" was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult Edit(short id)
    {
        var account = _accountService.GetAccountById(id);
        if (account == null) return NotFound();
        return PartialView("_AccountForm", new AccountFormViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? string.Empty,
            AccountEmail = account.AccountEmail ?? string.Empty,
            AccountRole = account.AccountRole
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(AccountFormViewModel model)
    {
        var account = _accountService.GetAccountById(model.AccountId);
        if (account == null) return NotFound();

        ValidateEmail(model);
        if (!ModelState.IsValid)
        {
            return PartialView("_AccountForm", model);
        }

        account.AccountName = model.AccountName.Trim();
        account.AccountEmail = model.AccountEmail.Trim();
        account.AccountRole = model.AccountRole;
        if (!string.IsNullOrEmpty(model.AccountPassword))
        {
            account.AccountPassword = model.AccountPassword;
        }
        _accountService.UpdateAccount(account);
        TempData["Success"] = $"Account \"{model.AccountName}\" was updated.";
        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult Delete(short id)
    {
        var account = _accountService.GetAccountById(id);
        if (account == null) return NotFound();
        return PartialView("_DeleteAccount", new DeleteAccountViewModel
        {
            Account = account,
            CanDelete = _accountService.CanDeleteAccount(id)
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(short id)
    {
        var account = _accountService.GetAccountById(id);
        if (account == null) return NotFound();
        if (!_accountService.CanDeleteAccount(id))
        {
            return Json(new { success = false, message = "This account has created news articles and cannot be deleted." });
        }
        _accountService.DeleteAccount(id);
        TempData["Success"] = $"Account \"{account.AccountName}\" was deleted.";
        return Json(new { success = true });
    }

    private void ValidateEmail(AccountFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.AccountEmail)) return;
        var email = model.AccountEmail.Trim();
        if (string.Equals(email, _adminAccount.Email, StringComparison.OrdinalIgnoreCase)
            || _accountService.IsEmailExisted(email, model.IsEdit ? model.AccountId : null))
        {
            ModelState.AddModelError(nameof(model.AccountEmail), "This email is already used by another account.");
        }
    }
}
