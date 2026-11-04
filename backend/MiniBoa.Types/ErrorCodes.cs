namespace MiniBoa.Types;

/// <summary>
/// UI'ın hatayı türüne göre ele alabilmesi için sabit hata kodları.
/// </summary>
public static class ErrorCodes
{
    public const string Unexpected = "SYS-0001";
    public const string MethodNotFound = "SYS-0002";
    public const string Unauthorized = "SEC-0001";
    public const string Validation = "VAL-0001";
    public const string NotFound = "BUS-0001";
    public const string BusinessRule = "BUS-0002";
}
