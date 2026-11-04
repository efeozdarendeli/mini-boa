namespace MiniBoa.Framework;

/// <summary>
/// Giriş yapmadan çağrılabilen orchestration metodlarını işaretler (Müşteri Ol, Giriş, il/ilçe listesi...).
/// İşaretsiz her metod varsayılan olarak giriş ister; Communication katmanındaki router buna bakar.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AnonymousAccessAttribute : Attribute;
