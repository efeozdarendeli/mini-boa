using MiniBoa.Business.Kernel;
using MiniBoa.Data;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Orchestration;

/// <summary>Ana sayfa özeti, hesaplarım, hesap hareketleri ve hesap açma. Hepsi giriş gerektirir.</summary>
public sealed class AccountHelper(
    ObjectHelper objectHelper,
    IJournalRepository journal,
    IUnitOfWork unitOfWork,
    Customer customer,
    Person person,
    Account account) : OrchestrationBase(objectHelper, journal)
{
    /// <summary>Ana sayfa: "Hoş geldin {ad}" ve "Hesaplarım / Toplam bakiye" kartları.</summary>
    public async Task<GenericResponse<CustomerSummaryContract>> GetCustomerSummary()
    {
        var returnObject = InitializeGenericResponse<CustomerSummaryContract>();

        var customerResponse = await customer.GetByCustomerNoAsync(CurrentCustomerNo);
        if (!customerResponse.Success)
        {
            return returnObject.AddResults(customerResponse);
        }

        var personResponse = await person.GetByIdAsync(customerResponse.Value!.PersonId);
        if (!personResponse.Success)
        {
            return returnObject.AddResults(personResponse);
        }

        var accounts = (await account.GetAccountsAsync(CurrentCustomerNo)).Value!;

        returnObject.Value = new CustomerSummaryContract
        {
            CustomerNo = CurrentCustomerNo,
            FirstName = personResponse.Value!.FirstName,
            LastName = personResponse.Value.LastName,
            Accounts = accounts.Select(a => a.ToContract()).ToList(),
            TotalTryBalance = accounts.Where(a => a.CurrencyCode == "TRY").Sum(a => a.Balance),
            PreviousLoginAt = customerResponse.Value.PreviousLoginAt
        };
        return returnObject;
    }

    /// <summary>Ana sayfadaki "Son İşlemler": bütün hesaplardaki son 10 hareket, karşı taraf maskeli.</summary>
    public async Task<GenericResponse<List<RecentTransactionContract>>> GetRecentTransactions()
    {
        var returnObject = InitializeGenericResponse<List<RecentTransactionContract>>();

        var recent = await account.GetRecentTransactionsAsync(CurrentCustomerNo);
        returnObject.Value = recent.Value!.Select(t => t.ToContract()).ToList();
        return returnObject;
    }

    public async Task<GenericResponse<List<AccountContract>>> GetAccounts()
    {
        var returnObject = InitializeGenericResponse<List<AccountContract>>();

        var accounts = await account.GetAccountsAsync(CurrentCustomerNo);
        returnObject.Value = accounts.Value!.Select(a => a.ToContract()).ToList();
        return returnObject;
    }

    public async Task<GenericResponse<List<TransactionContract>>> GetAccountTransactions(
        AccountTransactionsInputContract contract)
    {
        var returnObject = InitializeGenericResponse<List<TransactionContract>>();

        var transactions = await account.GetTransactionsAsync(
            CurrentCustomerNo, contract.AccountId, contract.Days, contract.Direction);
        if (!transactions.Success)
        {
            return returnObject.AddResults(transactions);
        }

        returnObject.Value = transactions.Value!.Select(t => t.ToContract()).ToList();
        return returnObject;
    }

    /// <summary>"Cari Hesap Aç" / "Katılım Hesap" kutucukları.</summary>
    public async Task<GenericResponse<AccountContract>> OpenAccount(OpenAccountContract contract)
    {
        var returnObject = InitializeGenericResponse<AccountContract>();

        var opened = await unitOfWork.ExecuteInTransactionAsync(() =>
            account.OpenAccountAsync(CurrentCustomerNo, contract.CurrencyCode, contract.AccountType));
        if (!opened.Success)
        {
            return returnObject.AddResults(opened);
        }

        await AppendToJournalAsync($"Account opened: {opened.Value!.AccountNumber}");

        returnObject.Value = opened.Value.ToContract();
        return returnObject;
    }
}
