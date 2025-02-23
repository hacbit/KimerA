namespace KimerA.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using KimerA.Data.Res;

    public static class TypeUtil
    {
        public static IEnumerable<string> ResTypes
        {
            get
            {
                var types = GetDerivedClassesFromGenericType(typeof(ResRefData<>));
                return types.Select(type => type!.BaseType!.GenericTypeArguments[0].Name);
            }
        }

        public static IEnumerable<string> DefaultTypes => new[]
        {
            nameof(SByte),
            nameof(Byte),
            nameof(Int16),
            nameof(UInt16),
            nameof(Int32),
            nameof(UInt32),
            nameof(Int64),
            nameof(UInt64),
            nameof(IntPtr),
            nameof(UIntPtr),
            nameof(Decimal),
            nameof(Char),
            nameof(Boolean),
            nameof(Single),
            nameof(Double),
            nameof(String),
            nameof(DateTime),
            nameof(TimeSpan),
            nameof(Guid),
        };

        public static IEnumerable<PropertyInfo> GetSerializableProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static IEnumerable<FieldInfo> GetSerializableFields(this Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        private static Lazy<IEnumerable<Type>> allTypes = new(() => 
                Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => type.IsClass || type.IsValueType)
                    .ToList());

        public static IEnumerable<Type> AllTypes => allTypes.Value;

        public static List<Assembly> CurrentAssemblies
        {
            get
            {
                return currentAssembliesInternal
                    .GetOrInit(() => AppDomain.CurrentDomain.GetAssemblies().ToList());
            }
        }

        private static readonly OnceCell<List<Assembly>> currentAssembliesInternal = new();

        public static List<Type> GetDerivedClassesFromGenericType(Type baseType)
        {
            var types = new List<Type>();
            foreach (var assembly in CurrentAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && type.IsAbstract == false && type.BaseType is { IsGenericType: true }
                        && baseType.IsAssignableFrom(type.BaseType.GetGenericTypeDefinition()))
                    {
                        types.Add(type);
                    }
                }
            }
            return types;
        }

        public static List<Type> GetDerivedClasses(Type baseType)
        {
            var types = new List<Type>();
            foreach (var assembly in CurrentAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && type.IsAbstract == false && baseType.IsAssignableFrom(type))
                    {
                        types.Add(type);
                    }
                }
            }
            return types;
        }

        public static List<Type> GetDerivedInterface(Type baseType)
        {
            var types = new List<Type>();
            foreach (var assembly in CurrentAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && type.IsAbstract == false && baseType.IsAssignableFrom(type))
                    {
                        types.Add(type);
                    }
                }
            }
            return types;
        }

        public static List<Type> GetDerivedStructs(Type baseType)
        {
            var types = new List<Type>();
            foreach (var assembly in CurrentAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsValueType && type.IsAbstract == false && baseType.IsAssignableFrom(type))
                    {
                        types.Add(type);
                    }
                }
            }
            return types;
        }
    }
}