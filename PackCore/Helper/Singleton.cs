using System.Collections.Concurrent;

namespace IconPack.Helper;

internal static class Singleton<T> where T : new()
{
    private static ConcurrentDictionary<Type, T> _instances = new ConcurrentDictionary<Type, T>();

    public static T Instance => _instances.GetOrAdd(typeof(T), (t) => new T());

    public static bool HasInitialize => _instances.ContainsKey(typeof(T));
}