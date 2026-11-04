using System.Globalization;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;

namespace MiniBoa.Business;

/// <summary>
/// İş nesnelerinin ortak tabanı. Kural ihlallerinde kullanıcının dilinde mesaj içeren
/// başarısız bir GenericResponse üretmeyi tek satıra indirir.
/// </summary>
public abstract class BusinessObjectBase(IMessagingHelper messaging)
{
    private const string MessageCategory = "Business";

    protected GenericResponse<T> Fail<T>(string errorCode, string messageKey) =>
        GenericResponse<T>.Fail(errorCode, messaging.GetMessage(MessageCategory, messageKey));

    /// <summary>"{0} dakika sonra tekrar deneyiniz" gibi yer tutuculu mesajlar için.</summary>
    protected GenericResponse<T> Fail<T>(string errorCode, string messageKey, params object[] args) =>
        GenericResponse<T>.Fail(
            errorCode,
            string.Format(CultureInfo.InvariantCulture, messaging.GetMessage(MessageCategory, messageKey), args));
}
