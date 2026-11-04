using System.Reflection;

namespace MiniBoa.Communication.Routing;

/// <summary>Çağrılabilir tek bir orchestration metodunun tarifi.</summary>
/// <param name="Name">Dışarıya açık adı, örn. "Transfer.Transfer".</param>
/// <param name="HelperType">Metodun bulunduğu helper sınıfı; çağrı anında DI'dan alınır.</param>
/// <param name="Method">Çağrılacak metod.</param>
/// <param name="InputType">Girdi contract'ının tipi; girdisiz metodlarda null.</param>
/// <param name="AllowAnonymous">[AnonymousAccess] ile işaretli mi?</param>
public sealed record OperationDescriptor(
    string Name,
    Type HelperType,
    MethodInfo Method,
    Type? InputType,
    bool AllowAnonymous);
