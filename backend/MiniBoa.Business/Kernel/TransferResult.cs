namespace MiniBoa.Business.Kernel;

/// <param name="ReferenceId">Transferin iki hareket satırını birbirine bağlayan referans (dekont no).</param>
/// <param name="SenderBalanceAfter">Gönderen hesabın transfer sonrası bakiyesi.</param>
/// <param name="TransactionDate">İşlem zamanı (UTC); dekontta gösterilir.</param>
public sealed record TransferResult(Guid ReferenceId, decimal SenderBalanceAfter, DateTime TransactionDate);
