using BusinessObjects.Models;

namespace Services;

public interface IAccountService
{
    List<SystemAccount> GetAccounts();
    List<SystemAccount> SearchAccounts(string? keyword, int? role);
    SystemAccount? GetAccountById(short id);
    SystemAccount? Login(string email, string password);
    bool IsEmailExisted(string email, short? excludeAccountId = null);
    bool CanDeleteAccount(short id);
    void AddAccount(SystemAccount account);
    void UpdateAccount(SystemAccount account);
    void DeleteAccount(short id);
}
