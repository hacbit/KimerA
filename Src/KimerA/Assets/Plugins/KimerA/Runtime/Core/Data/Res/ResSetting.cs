#if ODIN_INSPECTOR

namespace KimerA.Data.Res
{
    using System;
    using Sirenix.OdinInspector;

    [Serializable]
    public sealed class ResSetting
    {
        public const string DefaultResRefDataPath = "Assets/Res/RefData";

        [ValueDropdown("@KimerA.Utils.DirectoryUtil.GetFilePaths()")]
        public string ResRefDataPathRoot = DefaultResRefDataPath;

        public readonly string ResRefDataAddressableGroup = "ResRefData";

        public const string DefaultResRefCodePath = "Assets/Scripts/Generated/Res";

        [ValueDropdown("@KimerA.Utils.DirectoryUtil.GetFilePaths()")]
        public string CustomResRefCodePath = DefaultResRefCodePath;

        [Button("Reset Setting", ButtonSizes.Medium)]
        private void Reset()
        {
            ResRefDataPathRoot = DefaultResRefDataPath;
            CustomResRefCodePath = DefaultResRefCodePath;
        }
    }
}

#endif