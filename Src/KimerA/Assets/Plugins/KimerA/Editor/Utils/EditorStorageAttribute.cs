#if UNITY_EDITOR

using System;
using System.Diagnostics;

namespace KimerA.Editor.Utils;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
internal sealed class EditorStorageAttribute : Attribute
{
    public string StorageKey { get; }

    public EditorStorageAttribute(string storageKey)
    {
        StorageKey = storageKey;
    }
}

#endif