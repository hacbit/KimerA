#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KimerA.Editor;

internal static class TypeUtil
{
    private static List<Type> m_AllTypes;

    public static IEnumerable<Type> AllTypes
    {
        get
        {
            m_AllTypes ??= AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes()).ToList();
            return m_AllTypes;
        }
    }

    private static Dictionary<Type, List<Type>> m_InterfaceCache = new();

    private static Dictionary<Type, List<Type>> m_DerivedTypeCache = new();

    private static Dictionary<Type, List<Type>> m_AttributeCache = new();

    private enum CacheType
    {
        Interface,
        Derived,
        Attribute,
    }

    private static Dictionary<Type, List<Type>> ResolveCache(CacheType cacheType)
    {
        return cacheType switch
        {
            CacheType.Interface => m_InterfaceCache,
            CacheType.Derived => m_DerivedTypeCache,
            CacheType.Attribute => m_AttributeCache,
            _ => throw new NotSupportedException($"{cacheType}")
        };
    }

    private static Func<Type, Func<Type, bool>> ResolveTypeQueryFunc(CacheType cacheType)
    {
        return cacheType switch
        {
            CacheType.Interface => static tar => cur => cur.IsAbstract is false && tar.IsAssignableFrom(cur),
            CacheType.Derived => static tar => cur => cur.IsClass && cur.IsAbstract is false && tar.IsAssignableFrom(cur),
            CacheType.Attribute => static tar => cur => cur.GetCustomAttribute(tar) is not null,
            _ => throw new NotSupportedException($"{cacheType}")
        };
    }

    private static void EnsureTypeCacheWith(Type baseType, CacheType cacheType)
    {
        var cache = ResolveCache(cacheType);
        if (cache.ContainsKey(baseType) is false)
        {
            var queryFunc = ResolveTypeQueryFunc(cacheType)(baseType);
            cache.Add(baseType, AllTypes.Where(queryFunc).ToList());
        }
    }

    public static IEnumerable<Type> QueryTypesImplementing<TInterface>() where TInterface : class
    {
        return QueryTypesImplementing(typeof(TInterface));
    }

    public static IEnumerable<Type> QueryTypesImplementing(Type interfaceType)
    {
        EnsureTypeCacheWith(interfaceType, CacheType.Interface);
        return ResolveCache(CacheType.Interface)[interfaceType];
    }

    public static IEnumerable<Type> QueryTypesDerived<TBaseClass>() where TBaseClass : class
    {
        return QueryTypesDerived(typeof(TBaseClass));
    }

    public static IEnumerable<Type> QueryTypesDerived(Type baseType)
    {
        EnsureTypeCacheWith(baseType, CacheType.Derived);
        return ResolveCache(CacheType.Derived)[baseType];
    }

    public static IEnumerable<Type> QueryTypesWithAttribute<TAttr>() where TAttr : Attribute
    {
        return QueryTypesWithAttribute(typeof(TAttr));
    }

    public static IEnumerable<Type> QueryTypesWithAttribute(Type attribute)
    {
        EnsureTypeCacheWith(attribute, CacheType.Attribute);
        return ResolveCache(CacheType.Attribute)[attribute];
    }
}

#endif