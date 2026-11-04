using MiniBoa.Types;

namespace MiniBoa.Framework.Exceptions;

/// <summary>
/// Kod içinde bilerek fırlatılan iş hatası. Communication katmanındaki middleware
/// bunu yakalayıp GenericResponse hata formatına çevirir; kullanıcıya MessageKey'in çevirisi gösterilir.
/// </summary>
public class BoaException(string messageKey, string errorCode = ErrorCodes.BusinessRule)
    : Exception(messageKey)
{
    public string ErrorCode { get; } = errorCode;

    public string MessageKey { get; } = messageKey;
}
