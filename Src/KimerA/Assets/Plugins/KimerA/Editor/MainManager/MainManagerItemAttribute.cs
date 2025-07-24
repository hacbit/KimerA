#if UNITY_EDITOR

using System;

namespace KimerA.Editor;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MainManagerItemAttribute : Attribute
{
    public string ItemPath { get; }

    public int Priority { get; set; }

    public MainManagerItemAttribute(string path)
    {
        ItemPath = path;
    }
}


#endif