namespace KimerA.Utils
{
    /* public static class StorageUtil
    {
        #if UNITY_EDITOR
        private static SettingConfig settingConfigInternal = new();

        private static SettingConfig SettingConfig
        {
            get
            {
                if (settingConfigInternal != null) return settingConfigInternal;
                settingConfigInternal = ResUtil.GetAsset<SettingConfig>();
                if (settingConfigInternal != null) return settingConfigInternal;
                settingConfigInternal = SettingEditorWindow.InitSettingFile();
                return settingConfigInternal;
            }
        }
        #endif
    } */
}