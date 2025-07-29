#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEditor.AddressableAssets.Settings;

namespace KimerA.Editor.Utils
{
    [CreateAssetMenu(fileName = k_AssetName, menuName = "KimerA/Res/Addressable Asset Settings")]
    public class KimeraAddressableAssetSettingSO : ScriptableObject
    {
        public const string k_AssetName = "KimeraDefaultAssetSettings";

        public const string k_AssetDir = PathUtil.ConfigResPath;

        public const string k_AddressIdent = "com.kimera.addressableassets";

        [FormerlySerializedAs("m_KimeraAddressableAssetSettingsGuid")]
        [SerializeField]
        internal string m_AddressableAssetSettingsGuid;

        private bool m_LoadingSettingsObject = false;

        private static AddressableAssetSettings s_DefaultSettingsObject;

        public static bool SettingsExists
        {
            get
            {
                if (EditorBuildSettings.TryGetConfigObject<KimeraAddressableAssetSettingSO>(k_AddressIdent, out var result))
                {
                    return !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(result.m_AddressableAssetSettingsGuid));
                }

                return false;
            }
        }

        public static AddressableAssetSettings Settings
        {
            get
            {
                if (s_DefaultSettingsObject == null)
                {
                    if (EditorBuildSettings.TryGetConfigObject<KimeraAddressableAssetSettingSO>(k_AddressIdent, out var result))
                    {
                        s_DefaultSettingsObject = result.LoadSettingsObject();
                    }
                    else if (EditorBuildSettings.TryGetConfigObject<AddressableAssetSettings>(k_AssetName, out s_DefaultSettingsObject))
                    {
                        EditorBuildSettings.RemoveConfigObject(k_AssetName);
                        result = ScriptableObject.CreateInstance<KimeraAddressableAssetSettingSO>();
                        result.SetSettingsObject(s_DefaultSettingsObject);
                        AssetDatabase.CreateAsset(result, $"{k_AssetDir}/{k_AssetName}.asset");
                        EditorUtility.SetDirty(result);
                        // AddressableAssetUtility.OpenAssetIfUsingVCIntegration($"{k_AssetDir}/{k_AssetName}.asset");
                        AssetDatabase.SaveAssets();
                        EditorBuildSettings.AddConfigObject(k_AddressIdent, result, overwrite: true);
                    }
                }

                return s_DefaultSettingsObject;
            }
            set
            {
                if (value != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(value);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        Debug.LogErrorFormat("AddressableAssetSettings object must be saved to an asset before it can be set as the default.");
                        return;
                    }
                }

                s_DefaultSettingsObject = value;
                if (!EditorBuildSettings.TryGetConfigObject<KimeraAddressableAssetSettingSO>(k_AddressIdent, out var result))
                {
                    result = ScriptableObject.CreateInstance<KimeraAddressableAssetSettingSO>();
                    AssetDatabase.CreateAsset(result, $"{k_AssetDir}/{k_AssetName}.asset");
                    AssetDatabase.SaveAssets();
                    EditorBuildSettings.AddConfigObject(k_AddressIdent, result, overwrite: true);
                }

                result.SetSettingsObject(s_DefaultSettingsObject);
                EditorUtility.SetDirty(result);
                // AddressableAssetUtility.OpenAssetIfUsingVCIntegration($"{k_AssetDir}/{k_AssetName}.asset");
                AssetDatabase.SaveAssets();
            }
        }

        internal AddressableAssetSettings LoadSettingsObject()
        {
            if (m_LoadingSettingsObject)
            {
                Debug.LogWarning("Detected stack overflow when accessing AddressableAssetSettingsDefaultObject.Settings object.");
                return null;
            }

            if (string.IsNullOrEmpty(m_AddressableAssetSettingsGuid))
            {
                Debug.LogError("Invalid guid for default AddressableAssetSettings object.");
                return null;
            }

            string text = AssetDatabase.GUIDToAssetPath(m_AddressableAssetSettingsGuid);
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogErrorFormat("Unable to determine path for default AddressableAssetSettings object with guid {0}.", m_AddressableAssetSettingsGuid);
                return null;
            }

            m_LoadingSettingsObject = true;
            AddressableAssetSettings addressableAssetSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(text);
            if (addressableAssetSettings != null)
            {
                // AddressablesAssetPostProcessor.OnPostProcess.Register(addressableAssetSettings.OnPostprocessAllAssets, 0);
            }

            m_LoadingSettingsObject = false;
            return addressableAssetSettings;
        }

        private void SetSettingsObject(AddressableAssetSettings settings)
        {
            if (settings == null)
            {
                m_AddressableAssetSettingsGuid = null;
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(settings);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogErrorFormat("Unable to determine path for default AddressableAssetSettings object with guid {0}.", m_AddressableAssetSettingsGuid);
            }
            else
            {
                // AddressablesAssetPostProcessor.OnPostProcess.Register(settings.OnPostprocessAllAssets, 0);
                m_AddressableAssetSettingsGuid = AssetDatabase.AssetPathToGUID(assetPath);
            }
        }

        public static AddressableAssetSettings GetSettings(bool create)
        {
            if (Settings == null && create)
            {
                Settings = AddressableAssetSettings.Create(k_AssetDir, k_AssetName, createDefaultGroups: true, isPersisted: true);
            }

            return Settings;
        }
    }
}

#endif