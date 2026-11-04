using System.Runtime.CompilerServices;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Framework.Exceptions;
using MiniBoa.Types;

namespace MiniBoa.Orchestration;

/// <summary>
/// Orchestration helper'larının ortak tabanı (BOA'daki BOA.Orchestration.SelfService.*Helper sınıfları).
/// İzleme (trace), günlük (journal) ve giriş yapmış müşteriye erişimi tek yerde toplar.
/// </summary>
public abstract class OrchestrationBase(ObjectHelper objectHelper, IJournalRepository journal)
{
    protected RequestContext Context => objectHelper.Context;

    /// <summary>
    /// Giriş yapmış müşterinin numarası. Router zaten kontrol ediyor; burada tekrar kontrol etmek
    /// bir metodun yanlışlıkla anonim açılması durumuna karşı ikinci bir güvencedir.
    /// </summary>
    protected long CurrentCustomerNo =>
        Context.CustomerNo ?? throw new BoaException("Unauthorized", ErrorCodes.Unauthorized);

    /// <summary>
    /// BOA'daki objectHelper.InitializeGenericResponse&lt;T&gt;("...Helper.Method") karşılığı;
    /// metod adı derleyici tarafından otomatik verilir.
    /// </summary>
    protected GenericResponse<T> InitializeGenericResponse<T>([CallerMemberName] string methodName = "") =>
        objectHelper.InitializeGenericResponse<T>($"{GetType().Name}.{methodName}");

    /// <summary>BOA'daki DeviceJournalManager.AppendLineToJournal karşılığı.</summary>
    protected Task AppendToJournalAsync(string message, [CallerMemberName] string methodName = "") =>
        journal.AppendAsync(Context.TraceId, Context.CustomerNo, $"{GetType().Name}.{methodName}", message);
}
