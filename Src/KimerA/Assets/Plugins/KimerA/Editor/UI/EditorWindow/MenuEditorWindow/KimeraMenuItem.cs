#if UNITY_EDITOR

namespace KimerA.Editor.UI
{
    public readonly struct KimeraMenuItemData
    {
        public readonly string Name;
        public readonly IKimeraUI Value;

        public KimeraMenuItemData(string name, IKimeraUI value)
        {
            Name = name;
            Value = value;
        }
    }
}

#endif