using System.Security.Claims;
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

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly AdminAccountOptions _adminAccount;

    public AccountController(IAccountService accountService, IOptions<AdminAccountOptions> adminAccount)
    {
        _accountService = accountService;
        _adminAccount = adminAccount.Value;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        short accountId;
        string name;
        string role;

        if (string.Equals(model.Email.Trim(), _adminAccount.Email, StringComparison.OrdinalIgnoreCase)
            && model.Password == _adminAccount.Password)
        {
            accountId = 0;
            name = "Administrator";
            role = AccountRoles.AdminName;
        }
        else
        {
            SystemAccount? account;
            try
            {
                account = _accountService.Login(model.Email, model.Password);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Cannot connect to the database: " + ex.GetBaseException().Message);
                return View(model);
            }

            if (account == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }
            if (account.AccountRole != AccountRoles.Staff && account.AccountRole != AccountRoles.Lecturer)
            {
                ModelState.AddModelError(string.Empty, "You do not have permission to access this system.");
                return View(model);
            }

            accountId = account.AccountId;
            name = account.AccountName ?? account.AccountEmail ?? "User";
            role = AccountRoles.GetRoleName(account.AccountRole);
        }

        await SignInAsync(accountId, name, model.Email.Trim(), role);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    internal static ClaimsPrincipal CreatePrincipal(short accountId, string name, string email, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, accountId.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    private Task SignInAsync(short accountId, string name, string email, string role)
        => HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            CreatePrincipal(accountId, name, email, role));
}
