namespace MiniBoa.Data.Repositories;

/// <summary>log.device_journal tablosuna satır ekler (BOA'daki DeviceJournalManager.AppendLineToJournal).</summary>
public interface IJournalRepository
{
    Task AppendAsync(string traceId, long? customerNo, string methodName, string message);
}
