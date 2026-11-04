namespace MiniBoa.Framework.Messaging;

/// <summary>
/// Kullanıcıya gösterilecek metinleri dil dosyalarından getirir
/// (BOA'daki MessagingHelper.GetMessage("Xtm", key) karşılığı).
/// </summary>
public interface IMessagingHelper
{
    string GetMessage(string category, string key);
}
