#if KIMERA_ADDRESSABLES_SUPPORT

namespace KimerA.Utils
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEditor;
    using UnityEngine.AddressableAssets;
    using System.Linq;
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.AddressableAssets;
    using System.IO;

    public static class ResUtil
    {
        private static readonly Dictionary<string, Object> resCacheDict = new();

        public static async UniTask<T?> LoadAssetAsync<T>(string name) where T : Object
        {
            if (resCacheDict.TryGetValue(name, out var res))
            {
                return res as T;
            }
            else
            {
                resCacheDict.Add(name, await Addressables.LoadAssetAsync<T>(name).ToUniTask());
                return resCacheDict[name] as T;
            }
        }

        public static SettingConfig? SettingConfig;

        public static async UniTask LoadSettingConfigAsync()
        {
            SettingConfig = await LoadAssetAsync<SettingConfig>(nameof(SettingConfig));
        }

        #if UNITY_EDITOR
        public static IEnumerable<string> GetPrefabPaths()
        {
            return AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath);
        }

        public static T? GetAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).FullName}");
            if (guids.Length <= 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        #endif

        public static void LoadByResource<T>(string path, Action<T?> callback) where T : Object
        {
            var request = Resources.LoadAsync<T>(path);
            request.completed += res => callback?.Invoke(request.asset as T);
        }

        #if UNITY_EDITOR
        public static T GetOrCreateAsset<T>(string path) where T : Object
        {
            var data = GetAsset<T>();
            DirectoryUtil.ApplyDirectory(DirectoryUtil.ExtractFolder(path));
            if (data != null) return data;
            data = Activator.CreateInstance<T>();
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return data;
        }

        public static IEnumerable<T> GetAssetsInFolder<T>(string folder) where T : Object
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(path => path.Contains(folder))
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null);
        }

        public static IEnumerable<T> GetAssets<T>(IEnumerable<string> folders) where T : Object
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(path => folders.Any(path.Contains))
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null);
        }

        public static IEnumerable<T> GetAssets<T>() where T : Object
        {
            return AssetDatabase.GetAllAssetPaths()
                .Select(path => AssetDatabase.LoadAssetAtPath<T>(path))
                .Where(asset => asset != null);
        }

        public static IEnumerable<string> GetCustomAssetPaths<T>()
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).FullName}")
                .Select(AssetDatabase.GUIDToAssetPath);
        }

        public static IEnumerable<string> GetAssetPaths<T>()
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(T));
        }

        public static IEnumerable<string> GetAssetPaths<T>(IEnumerable<string> folders)
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(path => folders.Any(path.Contains))
                .Where(path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(T));
        }

        public static IEnumerable<string> GetAssetFolderPaths<T>()
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(T))
                .Select(path => path[..path.LastIndexOf("/", StringComparison.Ordinal)]);
        }

        private static AddressableAssetSettings? cachedSettingInternal;

        private static AddressableAssetSettings GetAddressableSetting()
        {
            if (cachedSettingInternal != null) return cachedSettingInternal;
            cachedSettingInternal = AddressableAssetSettingsDefaultObject.Settings;
            if (cachedSettingInternal == null)
            {
                Debug.LogError("Addressable Asset Setting not found. Please open Addressable Group window and create a group first.");
            }
            return cachedSettingInternal!;
        }

        private static void SaveAddressableSetting()
        {
            EditorUtility.SetDirty(cachedSettingInternal);
            AssetDatabase.SaveAssets();
            AddressableAssetSettingsDefaultObject.Settings = cachedSettingInternal!;
        }

        public static void AddAssetToGroup(string groupName, string path, bool simplifyName = false, IEnumerable<string>? labels = null)
        {
            var setting = GetAddressableSetting();

            var group = setting?.FindGroup(groupName) ?? setting!.CreateGroup(groupName, false, false, true, new());
            if (File.Exists(path))
            {
                var entry = setting.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group, false, false);
                if (entry == null)
                {
                    Debug.LogError($"Failed to add {path} to group {groupName}");
                    return;
                }
                if (simplifyName)
                {
                    entry.address = DirectoryUtil.ExtractName(path);
                    if (labels != null)
                    {
                        foreach (var label in labels)
                        {
                            entry.labels.Add(label);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"File not found: {path}");
            }

            SaveAddressableSetting();
        }

        public static void ClearAssetGroup(string groupName)
        {
            var setting = GetAddressableSetting();

            var group = setting.FindGroup(groupName);
            if (group == null)
            {
                var list = group!.entries.ToList();
                foreach (var entry in list)
                {
                    group.RemoveAssetEntry(entry);
                }
            }

            SaveAddressableSetting();
        }

        public static void RemoveAssetFromGroup(string groupName, string path)
        {
            var setting = GetAddressableSetting();

            var group = setting.FindGroup(groupName);
            if (group == null)
            {
                var entry = group!.GetAssetEntry(AssetDatabase.AssetPathToGUID(path));
                if (entry != null)
                {
                    group.RemoveAssetEntry(entry);
                }
            }

            SaveAddressableSetting();
        }

        public static void RemoveAssetsFromGroup(string groupName, IEnumerable<string> paths)
        {
            var setting = GetAddressableSetting();

            var group = setting.FindGroup(groupName);
            if (group == null)
            {
                foreach (var path in paths)
                {
                    var entry = group!.GetAssetEntry(AssetDatabase.AssetPathToGUID(path));
                    if (entry != null)
                    {
                        group.RemoveAssetEntry(entry);
                    }
                }
            }

            SaveAddressableSetting();
        }

        public static void AddAssetsToGroup(string groupName, IEnumerable<string> paths, bool simplifyName = false, IEnumerable<string>? labels = null)
        {
            var setting = GetAddressableSetting();

            var group = setting.FindGroup(groupName) ?? setting.CreateGroup(groupName, false, false, true, new());
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    var entry = setting.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group, false, false);
                    if (entry == null)
                    {
                        Debug.LogError($"Failed to add {path} to group {groupName}");
                        return;
                    }
                    if (simplifyName)
                    {
                        entry.address = DirectoryUtil.ExtractName(path);
                        if (labels != null)
                        {
                            foreach (var label in labels)
                            {
                                entry.labels.Add(label);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"File not found: {path}");
                }
            }

            SaveAddressableSetting();
        }
        #endif
    }
}

#endif