namespace KimerA
{
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = "SettingConfig", menuName = "Kimera/SettingConfig")]
    public sealed class SettingConfig : ScriptableObject
    {
        [ValueDropdown("GetFileNames")]
        public string? CurrentSettingFile;

        public List<TextAsset> SettingFiles = new();

        public IEnumerable<string> GetFileNames()
        {
            return SettingFiles.Select(file => file.name);
        }
    }
}