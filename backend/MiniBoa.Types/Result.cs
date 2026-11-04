namespace MiniBoa.Types;

/// <summary>
/// Bir işlemin ürettiği tek bir mesaj (hata, uyarı veya bilgi).
/// </summary>
public sealed record Result(string ErrorCode, string Message, Severity Severity = Severity.Error);

public enum Severity
{
    Info,
    Warning,
    Error
}
