using BusinessObjects.Models;

namespace TrungKienMVC.ViewModels;

public class AccountIndexViewModel
{
    public string? Keyword { get; set; }
    public int? Role { get; set; }
    public List<SystemAccount> Accounts { get; set; } = new();
}
