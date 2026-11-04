using System.Reflection;
using MiniBoa.Framework;
using MiniBoa.Orchestration;
using MiniBoa.Types;

namespace MiniBoa.Communication.Routing;

/// <summary>
/// Dışarıdan çağrılabilecek işlemlerin listesi (Messaging &amp; Routing katmanı).
/// Uygulama açılırken bir kez oluşturulur: OrchestrationBase'den türeyen her helper'ın
/// GenericResponse dönen public metodları kaydedilir. Liste sonradan değişmez; bu listede
/// olmayan hiçbir metod, isteğe ne yazılırsa yazılsın çağrılamaz.
/// </summary>
public sealed class OperationRegistry
{
    private const string HelperSuffix = "Helper";

    private readonly Dictionary<string, OperationDescriptor> _operations;

    private OperationRegistry(IEnumerable<OperationDescriptor> operations)
    {
        _operations = operations.ToDictionary(o => o.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<OperationDescriptor> All => _operations.Values.OrderBy(o => o.Name);

    public bool TryGet(string name, out OperationDescriptor operation) =>
        _operations.TryGetValue(name, out operation!);

    public static OperationRegistry FromOrchestrationAssembly()
    {
        var helperTypes = typeof(OrchestrationBase).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(OrchestrationBase)));

        return new OperationRegistry(helperTypes.SelectMany(DescribeOperations));
    }

    private static IEnumerable<OperationDescriptor> DescribeOperations(Type helperType)
    {
        var helperName = helperType.Name.EndsWith(HelperSuffix)
            ? helperType.Name[..^HelperSuffix.Length]
            : helperType.Name;

        // DeclaredOnly: taban sınıftan veya object'ten gelen metodlar (ToString vb.) dışarı açılmaz.
        var methods = helperType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var method in methods.Where(IsOperation))
        {
            var parameters = method.GetParameters();

            yield return new OperationDescriptor(
                Name: $"{helperName}.{method.Name}",
                HelperType: helperType,
                Method: method,
                InputType: parameters.Length == 1 ? parameters[0].ParameterType : null,
                AllowAnonymous: method.GetCustomAttribute<AnonymousAccessAttribute>() is not null);
        }
    }

    /// <summary>İşlem sayılması için: Task&lt;GenericResponse&lt;T&gt;&gt; dönmeli ve en fazla bir parametre almalı.</summary>
    private static bool IsOperation(MethodInfo method)
    {
        var returnType = method.ReturnType;

        return returnType.IsGenericType
               && returnType.GetGenericTypeDefinition() == typeof(Task<>)
               && returnType.GetGenericArguments()[0] is { IsGenericType: true } inner
               && inner.GetGenericTypeDefinition() == typeof(GenericResponse<>)
               && method.GetParameters().Length <= 1;
    }
}
