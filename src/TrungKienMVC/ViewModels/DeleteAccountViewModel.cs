using BusinessObjects.Models;

namespace TrungKienMVC.ViewModels;

public class DeleteAccountViewModel
{
    public SystemAccount Account { get; set; } = null!;
    public bool CanDelete { get; set; }
}
