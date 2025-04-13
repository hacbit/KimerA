using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace KimerA
{
    /// <summary>
    /// Attribute to mark a field or property to be ignored during serialization/deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute { }

    public enum BinaryType : byte
    {
        Byte = (byte)'b',
        SByte = (byte)'B',
        Short = (byte)'h',
        UShort = (byte)'H',
        Int32 = (byte)'i',
        UInt32 = (byte)'I',
        Int64 = (byte)'l',
        UInt64 = (byte)'L',
        Float = (byte)'f',
        Double = (byte)'d',
        Enum = (byte)'e',
        String = (byte)'s',
        /// <summary>
        /// Array-like, such as List<T>, T[], IEnumerable<T>, etc.
        /// </summary>
        Array = (byte)'a',
        /// <summary>
        /// Dictionary-like, such as Dictionary<TKey, TValue>, etc.
        /// </summary>
        Table = (byte)'t',
        DateTime = (byte)'D',
        True = (byte)'T',
        False = (byte)'F',
        Object = (byte)'o',
    }

    public static class BinarySerde
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

#region Accessor Caches
        private static readonly Dictionary<Type, Func<object, Dictionary<string, object>>> m_AccessorCache = new();

        private static Func<object, Dictionary<string, object>> CreateAccessor(Type type)
        {
            var parameter = Expression.Parameter(typeof(object), "obj");
            var castedObj = Expression.Convert(parameter, type);

            var bindings = type.GetMembers(Flags)
                .Where(m => m.MemberType is MemberTypes.Field or MemberTypes.Property &&
                        m.GetCustomAttributes(typeof(IgnoreAttribute), false).Length == 0)
                .Select(m =>
                {
                    var memberAccess = m.MemberType switch
                    {
                        MemberTypes.Field => Expression.Field(castedObj, (FieldInfo)m),
                        MemberTypes.Property => Expression.Property(castedObj, (PropertyInfo)m),
                        _ => throw new InvalidOperationException()
                    };
                    return Expression.ElementInit(
                        typeof(Dictionary<string, object>).GetMethod("Add"),
                        Expression.Constant(m.Name),
                        Expression.Convert(memberAccess, typeof(object))
                    );
                });

            var body = Expression.ListInit(Expression.New(typeof(Dictionary<string, object>)), bindings);
            var lambda = Expression.Lambda<Func<object, Dictionary<string, object>>>(body, parameter);
            return lambda.Compile();
        }
#endregion

#region Deserialize
        /// <summary>
        /// Deserializes a byte array into an object of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public static T? Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }
            int offset = 0;
            var result = Parse(bytes, ref offset, typeof(T));
            if (result is T tResult)
            {
                return tResult;
            }
            throw new InvalidCastException($"Cannot cast {result?.GetType()} to {typeof(T)}.");
        }

        private static object? Parse(byte[] bytes, ref int offset, Type type)
        {
            var marker = ParseMarker(bytes, ref offset);
            return marker switch
            {
                BinaryType.Byte => ParseByte(bytes, ref offset),
                BinaryType.SByte => ParseSByte(bytes, ref offset),
                BinaryType.Short => ParseNumber<short>(bytes, ref offset),
                BinaryType.UShort => ParseNumber<ushort>(bytes, ref offset),
                BinaryType.Int32 => ParseNumber<int>(bytes, ref offset),
                BinaryType.UInt32 => ParseNumber<uint>(bytes, ref offset),
                BinaryType.Int64 => ParseNumber<long>(bytes, ref offset),
                BinaryType.UInt64 => ParseNumber<ulong>(bytes, ref offset),
                BinaryType.Float => ParseNumber<float>(bytes, ref offset),
                BinaryType.Double => ParseNumber<double>(bytes, ref offset),
                BinaryType.String => ParseString(bytes, ref offset),
                BinaryType.Enum => ParseNumber<int>(bytes, ref offset),
                BinaryType.True => true,
                BinaryType.False => false,
                BinaryType.DateTime => new DateTime(ParseNumber<long>(bytes, ref offset)),
                _ => ParseComplexType(bytes, ref offset, marker, type),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ParseByte(byte[] bytes, ref int offset)
        {
            return bytes[offset++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static sbyte ParseSByte(byte[] bytes, ref int offset)
        {
            return (sbyte)bytes[offset++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T ParseNumber<T>(byte[] bytes, ref int offset) where T : unmanaged
        {
            var value = CopyByteFrom<T>(bytes, offset);
            offset += Marshal.SizeOf<T>();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ParseString(byte[] bytes, ref int offset)
        {
            var length = ParseNumber<int>(bytes, ref offset);
            var str = Encoding.UTF8.GetString(bytes, offset, length);
            offset += length;
            return str;
        }

        private static BinaryType ParseMarker(byte[] bytes, ref int offset)
        {
            if (offset >= bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset is out of range.");
            }
            return (BinaryType)bytes[offset++];
        }

        private static object ParseComplexType(byte[] bytes, ref int offset, BinaryType marker, Type type)
        {
            if (marker is BinaryType.Object)
            {
                return ParseObject(bytes, ref offset, type);
            }
            else
            {
                // Handle array-like and dictionary-like types
                if (type.IsArray)
                {
                    return ParseArray(bytes, ref offset, type);
                }
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(List<>) || genericType == typeof(IList<>) || genericType == typeof(IEnumerable<>))
                    {
                        return ParseList(bytes, ref offset, type);
                    }
                    if (genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>))
                    {
                        return ParseDict(bytes, ref offset, type);
                    }
                }

                return ParseObject(bytes, ref offset, type);
            }
        }

        private static Array ParseArray(byte[] bytes, ref int offset, Type type)
        {
            var count = ParseNumber<int>(bytes, ref offset);
            var elementType = type.GetElementType()
                ?? throw new InvalidOperationException($"Cannot get element type of array {type}.");
            var array = Array.CreateInstance(elementType, count);
            for (var i = 0; i < count; i++)
            {
                var item = Parse(bytes, ref offset, elementType);
                if (item != null)
                {
                    array.SetValue(item, i);
                }
            }
            return array;
        }

        private static IList ParseList(byte[] bytes, ref int offset, Type type)
        {
            var count = ParseNumber<int>(bytes, ref offset);
            var elementType = type.GetGenericArguments()[0]
                ?? throw new InvalidOperationException($"Cannot get element type of list {type}.");
            var list = (IList)Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Cannot create instance of type {type}.");
            for (var i = 0; i < count; i++)
            {
                var item = Parse(bytes, ref offset, elementType);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        private static IDictionary ParseDict(byte[] bytes, ref int offset, Type type)
        {
            var count = ParseNumber<int>(bytes, ref offset);
            var keyType = type.GetGenericArguments()[0]
                ?? throw new InvalidOperationException($"Cannot get key type of dictionary {type}.");
            var valueType = type.GetGenericArguments()[1]
                ?? throw new InvalidOperationException($"Cannot get value type of dictionary {type}.");
            var dict = (IDictionary)Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Cannot create instance of type {type}.");
            for (var i = 0; i < count; i++)
            {
                var key = Parse(bytes, ref offset, keyType);
                var value = Parse(bytes, ref offset, valueType);
                if (key != null && value != null)
                {
                    dict.Add(key, value);
                }
            }
            return dict;
        }

        private static object ParseObject(byte[] bytes, ref int offset, Type type)
        {
            var obj = Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Cannot create instance of type {type}.");
            var count = ParseNumber<int>(bytes, ref offset);
            var membersMap = new Dictionary<string, MemberInfo>();
            var members = type.GetMembers(Flags)
                .Where(m => m.MemberType is MemberTypes.Field or MemberTypes.Property);
            foreach (var member in members)
            {
                if (member.GetCustomAttributes(typeof(IgnoreAttribute), false).Length > 0)
                {
                    continue;
                }
                var name = member.Name;
                membersMap[name] = member;
            }
            for (var i = 0; i < count; i++)
            {
                var marker = ParseMarker(bytes, ref offset);
                Debug.Assert(BinaryType.String == marker,
                    $"Expected a string marker, but got {(char)bytes[offset - 1]} at offset {offset - 1}.");
                // Read the member name
                var name = ParseString(bytes, ref offset);
                if (membersMap.TryGetValue(name, out var member))
                {
                    var memberType = member switch
                    {
                        FieldInfo f => f.FieldType,
                        PropertyInfo p => p.PropertyType,
                        _ => throw new InvalidOperationException($"Unsupported member type: {member.MemberType}.")
                    };
                    // Read the member value
                    var value = Parse(bytes, ref offset, memberType);
                    if (member is FieldInfo field)
                    {
                        field.SetValue(obj, value);
                    }
                    else if (member is PropertyInfo property)
                    {
                        property.SetValue(obj, value);
                    }
                }
            }
            return obj;
        }

        private static T CopyByteFrom<T>(byte[] bytes, int offset) where T : unmanaged
        {
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    return *(T*)(ptr + offset);
                }
            }
        }
#endregion

#region Serialize
        public static byte[] Serialize(object? obj)
        {
            if (obj is byte b)
            {
                return new[] { (byte)BinaryType.Byte, b };
            }
            if (obj is sbyte sb)
            {
                return new[] { (byte)BinaryType.SByte, (byte)sb };
            }
            if (obj is short s)
            {
                var buffer = new byte[sizeof(short) + 1];
                buffer[0] = (byte)BinaryType.Short;
                CopyByteTo(buffer, s, 1);
                return buffer;
            }
            if (obj is ushort us)
            {
                var buffer = new byte[sizeof(ushort) + 1];
                buffer[0] = (byte)BinaryType.UShort;
                CopyByteTo(buffer, us, 1);
                return buffer;
            }
            if (obj is int i)
            {
                var buffer = new byte[sizeof(int) + 1];
                buffer[0] = (byte)BinaryType.Int32;
                CopyByteTo(buffer, i, 1);
                return buffer;
            }
            if (obj is uint ui)
            {
                var buffer = new byte[sizeof(uint) + 1];
                buffer[0] = (byte)BinaryType.UInt32;
                CopyByteTo(buffer, ui, 1);
                return buffer;
            }
            if (obj is long l)
            {
                var buffer = new byte[sizeof(long) + 1];
                buffer[0] = (byte)BinaryType.Int64;
                CopyByteTo(buffer, l, 1);
                return buffer;
            }
            if (obj is ulong ul)
            {
                var buffer = new byte[sizeof(ulong) + 1];
                buffer[0] = (byte)BinaryType.UInt64;
                CopyByteTo(buffer, ul, 1);
                return buffer;
            }
            if (obj is float f)
            {
                var buffer = new byte[sizeof(float) + 1];
                buffer[0] = (byte)BinaryType.Float;
                CopyByteTo(buffer, f, 1);
                return buffer;
            }
            if (obj is double d)
            {
                var buffer = new byte[sizeof(double) + 1];
                buffer[0] = (byte)BinaryType.Double;
                CopyByteTo(buffer, d, 1);
                return buffer;
            }
            if (obj is true)
            {
                return new[] { (byte)BinaryType.True };
            }
            if (obj is false)
            {
                return new[] { (byte)BinaryType.False };
            }
            if (obj is Enum)
            {
                return SerializeEnum(obj);
            }
            if (obj is string str)
            {
                var strBytes = Encoding.UTF8.GetBytes(str);
                var buffer = new byte[strBytes.Length + 5];
                buffer[0] = (byte)BinaryType.String;
                CopyByteTo(buffer, strBytes.Length, 1);
                Buffer.BlockCopy(strBytes, 0, buffer, 5, strBytes.Length);
                return buffer;
            }
            if (obj is DateTime dt)
            {
                var buffer = new byte[sizeof(long) + 5];
                buffer[0] = (byte)BinaryType.DateTime;
                CopyByteTo(buffer, dt.Ticks, 1);
                return buffer;
            }
            var type = (obj?.GetType()) ?? throw new ArgumentNullException(nameof(obj), "Object cannot be null.");
            if (type.IsArray)
            {
                var array = (IEnumerable)obj;
                return SerializeListLike(array);
            }
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(List<>) || genericType == typeof(IList<>) || genericType == typeof(IEnumerable<>))
                {
                    return SerializeListLike((IEnumerable)obj);
                }
                if (genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>))
                {
                    return SerializeDictLike((IDictionary)obj);
                }
                return SerializeObject(obj, type);
            }
            if (type.IsClass || type.IsValueType)
            {
                return SerializeObject(obj, type);
            }
            throw new NotSupportedException($"Type {type} is not supported for serialization.");
        }

        private static byte[] SerializeListLike(IEnumerable enumerable)
        {
            using var stream = new MemoryStream();
            stream.Write(new byte[] { (byte)BinaryType.Array, 0, 0, 0, 0 });
            var count = 0;
            foreach (var item in enumerable)
            {
                var itemBuffer = Serialize(item);
                stream.Write(itemBuffer, 0, itemBuffer.Length);
                count++;
            }

            var arr = stream.ToArray();
            CopyByteTo(arr, count, 1);
            return arr;
        }

        private static byte[] SerializeDictLike(IDictionary dict)
        {
            using var stream = new MemoryStream();
            stream.Write(new byte[] { (byte)BinaryType.Table, 0, 0, 0, 0 });
            var count = 0;
            foreach (DictionaryEntry entry in dict)
            {
                var keyBuffer = Serialize(entry.Key);
                var valueBuffer = Serialize(entry.Value);
                stream.Write(keyBuffer, 0, keyBuffer.Length);
                stream.Write(valueBuffer, 0, valueBuffer.Length);
                count++;
            }

            var arr = stream.ToArray();
            CopyByteTo(arr, count, 1);
            return arr;
        }

        private static byte[] SerializeEnum(object obj)
        {
            var buffer = Serialize((int)obj);
            buffer[0] = (byte)BinaryType.Enum;
            return buffer;
        }

        private static byte[] SerializeObject(object obj, Type type)
        {
            if (!m_AccessorCache.TryGetValue(type, out var accessor))
            {
                accessor = CreateAccessor(type);
                m_AccessorCache[type] = accessor;
            }

            var members = accessor(obj);
            using var stream = new MemoryStream();
            stream.Write(new byte[] { (byte)BinaryType.Object, 0, 0, 0, 0 });

            var count = 0;
            foreach (var (name, value) in members)
            {
                var nameBytes = Serialize(name);
                var valueBytes = Serialize(value);
                stream.Write(nameBytes, 0, nameBytes.Length);
                stream.Write(valueBytes, 0, valueBytes.Length);
                count++;
            }

            var arr = stream.ToArray();
            CopyByteTo(arr, count, 1);
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyByteTo<T>(byte[] bytes, T obj, int offset) where T : unmanaged
        {
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    *(T*)(ptr + offset) = obj;
                }
            }
        }
    }
#endregion
}