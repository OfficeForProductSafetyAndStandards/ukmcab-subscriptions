using ConcurrentCollections;

namespace UKMCAB.Subscriptions.Core.Common;

public static class Memory
{
    private static readonly ConcurrentHashSet<string> _strings = new();

    /// <summary>
    /// Sets a string to be remembered
    /// </summary>
    /// <param name="name"></param>
    public static bool Set(string name) => _strings.Add(name);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool Has(string name) => _strings.Contains(name);


    /// <summary>
    /// Sets a string to be remembered
    /// </summary>
    /// <param name="name"></param>
    public static bool Set(Type type, string name) => _strings.Add(Keyify(type.FullName ?? string.Empty, name));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool Has(Type type, string name) => _strings.Contains(Keyify(type.FullName ?? string.Empty, name));

    /// <summary>
    /// Sets a string to be remembered
    /// </summary>
    /// <param name="name"></param>
    public static bool Set(string scope, string name) => _strings.Add(Keyify(scope, name));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool Has(string scope, string name) => _strings.Contains(Keyify(scope, name));

    private static string Keyify(string scope, string name) => string.Concat(scope, '.', name);
}