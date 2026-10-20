using BusinessObjects.Models;
using Repositories;

namespace Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public List<SystemAccount> GetAccounts() => _accountRepository.GetAccounts();

    public List<SystemAccount> SearchAccounts(string? keyword, int? role) => _accountRepository.SearchAccounts(keyword, role);

    public SystemAccount? GetAccountById(short id) => _accountRepository.GetAccountById(id);

    public SystemAccount? Login(string email, string password) => _accountRepository.Login(email.Trim(), password);

    public bool IsEmailExisted(string email, short? excludeAccountId = null) => _accountRepository.IsEmailExisted(email.Trim(), excludeAccountId);

    /// <summary>An account that already created news articles cannot be deleted.</summary>
    public bool CanDeleteAccount(short id) => !_accountRepository.HasNewsArticles(id);

    public void AddAccount(SystemAccount account) => _accountRepository.AddAccount(account);

    public void UpdateAccount(SystemAccount account) => _accountRepository.UpdateAccount(account);

    public void DeleteAccount(short id)
    {
        if (!CanDeleteAccount(id))
        {
            throw new InvalidOperationException("This account has created news articles and cannot be deleted.");
        }
        _accountRepository.DeleteAccount(id);
    }
}
