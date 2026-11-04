using MiniBoa.Business.Kernel;
using MiniBoa.Data;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Orchestration;

/// <summary>"Para Transferi" akışı.</summary>
public sealed class TransferHelper(
    ObjectHelper objectHelper,
    IJournalRepository journal,
    IUnitOfWork unitOfWork,
    Account account) : OrchestrationBase(objectHelper, journal)
{
    public async Task<GenericResponse<TransferResultContract>> Transfer(TransferContract contract)
    {
        var returnObject = InitializeGenericResponse<TransferResultContract>();

        // Transaction burada açılır; Account.TransferAsync içindeki kilit, iki bakiye güncellemesi
        // ve iki hareket kaydı bu transaction'ın parçasıdır.
        var transfer = await unitOfWork.ExecuteInTransactionAsync(() => account.TransferAsync(
            CurrentCustomerNo, contract.FromAccountId, contract.ToIban, contract.Amount, contract.Description));

        // Günlük transaction dışında yazılır; başarısız denemeler de kayıt altında kalır.
        await AppendToJournalAsync(transfer.Success
            ? $"Transfer {contract.Amount} from account {contract.FromAccountId} succeeded, ref {transfer.Value!.ReferenceId}"
            : $"Transfer {contract.Amount} from account {contract.FromAccountId} failed: {transfer.Results[0].ErrorCode}");

        if (!transfer.Success)
        {
            return returnObject.AddResults(transfer);
        }

        returnObject.Value = new TransferResultContract
        {
            ReferenceId = transfer.Value!.ReferenceId,
            SenderBalanceAfter = transfer.Value.SenderBalanceAfter,
            TransactionDate = transfer.Value.TransactionDate
        };
        return returnObject;
    }

    /// <summary>Transfer formunda IBAN girilince alıcının maskeli adını gösterir.</summary>
    public async Task<GenericResponse<ReceiverNameContract>> GetReceiverName(ReceiverNameInputContract contract)
    {
        var returnObject = InitializeGenericResponse<ReceiverNameContract>();

        var name = await account.GetReceiverNameAsync(contract.Iban);
        if (!name.Success)
        {
            return returnObject.AddResults(name);
        }

        returnObject.Value = new ReceiverNameContract { MaskedName = name.Value! };
        return returnObject;
    }
}
