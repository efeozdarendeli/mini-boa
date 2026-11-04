namespace MiniBoa.Types;

/// <summary>
/// Tüm katmanların ortak dönüş tipi (BOA'daki GenericResponse&lt;T&gt; karşılığı).
/// Beklenen hatalar exception fırlatılarak değil, Results listesine eklenerek taşınır;
/// böylece UI her zaman aynı şekilde "Success mi, değil mi?" diye kontrol eder.
/// </summary>
public sealed class GenericResponse<T>
{
    public T? Value { get; set; }

    public List<Result> Results { get; init; } = [];

    public bool Success => Results.All(r => r.Severity != Severity.Error);

    public GenericResponse<T> AddError(string errorCode, string message)
    {
        Results.Add(new Result(errorCode, message));
        return this;
    }

    /// <summary>
    /// Alt katmandan gelen bir cevabın mesajlarını bu cevaba taşır.
    /// Orchestration, Business'tan gelen hatayı UI'a iletirken kullanır.
    /// </summary>
    public GenericResponse<T> AddResults<TOther>(GenericResponse<TOther> other)
    {
        Results.AddRange(other.Results);
        return this;
    }

    public static GenericResponse<T> Ok(T value) => new() { Value = value };

    public static GenericResponse<T> Fail(string errorCode, string message) =>
        new GenericResponse<T>().AddError(errorCode, message);
}
