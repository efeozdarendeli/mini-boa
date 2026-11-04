namespace MiniBoa.Types.Contracts;

public sealed class TransferContract
{
    public long FromAccountId { get; init; }
    public string ToIban { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public sealed class TransferResultContract
{
    public Guid ReferenceId { get; init; }
    public decimal SenderBalanceAfter { get; init; }
    public DateTime TransactionDate { get; init; }
}

public sealed class ReceiverNameInputContract
{
    public string Iban { get; init; } = string.Empty;
}

/// <summary>Alıcının maskeli adı, örn. "Ay** Ka**".</summary>
public sealed class ReceiverNameContract
{
    public string MaskedName { get; init; } = string.Empty;
}
