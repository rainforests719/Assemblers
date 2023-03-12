﻿namespace Assemblers;

internal static class DeepClonerCache
{
    private static readonly ConcurrentDictionary<Type, object> _typeCache = new();

    private static readonly ConcurrentDictionary<Type, object> _typeCacheDeepTo = new();

    private static readonly ConcurrentDictionary<Type, object> _typeCacheShallowTo = new();

    private static readonly ConcurrentDictionary<Type, object> _structAsObjectCache = new();

    private static readonly ConcurrentDictionary<Tuple<Type, Type>, object> _typeConvertCache = new();

    public static object GetOrAddClass<T>(Type type, Func<Type, T> adder)
    {
        object value;
        if (_typeCache.TryGetValue(type, out value)) return value;

        lock (type)
        {
            value = _typeCache.GetOrAdd(type, t => adder(t));
        }

        return value;
    }

    public static object GetOrAddDeepClassTo<T>(Type type, Func<Type, T> adder)
    {
        object value;
        if (_typeCacheDeepTo.TryGetValue(type, out value)) return value;
        lock (type)
        {
            value = _typeCacheDeepTo.GetOrAdd(type, t => adder(t));
        }

        return value;
    }

    public static object GetOrAddShallowClassTo<T>(Type type, Func<Type, T> adder)
    {
        object value;
        if (_typeCacheShallowTo.TryGetValue(type, out value)) return value;
        lock (type)
        {
            value = _typeCacheShallowTo.GetOrAdd(type, t => adder(t));
        }

        return value;
    }

    public static object GetOrAddStructAsObject<T>(Type type, Func<Type, T> adder)
    {
        object value;
        if (_structAsObjectCache.TryGetValue(type, out value)) return value;
        lock (type)
        {
            value = _structAsObjectCache.GetOrAdd(type, t => adder(t));
        }

        return value;
    }

    public static T GetOrAddConvertor<T>(Type from, Type to, Func<Type, Type, T> adder)
    {
        return (T)_typeConvertCache.GetOrAdd(new Tuple<Type, Type>(from, to), (tuple) => adder(tuple.Item1, tuple.Item2));
    }

    public static void ClearCache()
    {
        _typeCache.Clear();
        _typeCacheDeepTo.Clear();
        _typeCacheShallowTo.Clear();
        _structAsObjectCache.Clear();
        _typeConvertCache.Clear();
    }
}