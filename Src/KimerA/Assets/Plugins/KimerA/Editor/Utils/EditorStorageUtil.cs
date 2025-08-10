#if UNITY_EDITOR

using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace KimerA.Editor.Utils;

internal sealed class EditorStoragePack<T>
{
    private readonly T _defaultValue;
    public readonly string Key;

    public T Value
    {
        get => EditorStorageUtil.GetValueOrDefault(Key, _defaultValue);
        set => EditorStorageUtil.SetValue(Key, value);
    }

    private EditorStoragePack() { }

    public EditorStoragePack(string key, T defaultValue)
    {
        var value = EditorStorageUtil.GetValueOrDefault(key, defaultValue);
        EditorStorageUtil.SetValue(key, value);
        Key = key;
    }
}

public static class EditorStorageUtil
{
    static EditorStorageUtil()
    {
        LoadAllStorages();

        EditorApplication.update -= AutoRefreshStorages;
        EditorApplication.update += AutoRefreshStorages;
    }

    private static void LoadAllStorages()
    {
        var storages = ResUtil.ResolveConfigResAllAssets<TextAsset>();
        foreach (var (path, storage) in storages)
        {
            var content = storage.text;
            if (string.IsNullOrEmpty(content) is false)
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                if (json is not null && path.EndsWith(".json"))
                {
                    var key = path[..^5];
                    m_ConfigCaches[key] = json;
                }
            }
        }
    }

    private static void AutoRefreshStorages()
    {
        foreach (var (key, isDirty) in m_ConfigDirtyMask)
        {
            if (isDirty)
            {
                m_ConfigDirtyMask[key] = false;
                SaveStorage(key);
            }
        }
    }

    public const string k_StorageDefaultKey = "DefaultEditorStorage";

    private static readonly Dictionary<string, Dictionary<string, object>> m_ConfigCaches = new();

    private static readonly Dictionary<string, bool> m_ConfigDirtyMask = new();

    private static Dictionary<string, object> GetOrCreateStorage(string storageKey = k_StorageDefaultKey)
    {
        if (m_ConfigCaches.TryGetValue(storageKey, out var config))
        {
            return config;
        }
        var storage = ResUtil.ResolveConfigResContent($"{storageKey}.json");
        if (storage is null)
        {
            ResUtil.WriteConfigResFile($"{storageKey}.json", "{}");
            return null;
        }
        var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(storage);
        return json;
    }

    public static void SaveStorage(string storageKey = k_StorageDefaultKey)
    {
        if (m_ConfigCaches.TryGetValue(storageKey, out var config))
        {
            var content = JsonConvert.SerializeObject(config);
            ResUtil.WriteConfigResFile($"{storageKey}.json", content);
        }
    }

    public static void SaveAllStorages()
    {
        foreach (var (key, config) in m_ConfigCaches)
        {
            var content = JsonConvert.SerializeObject(config);
            ResUtil.WriteConfigResFile($"{key}.json", content);
        }
    }

    internal static IEnumerable<string> GetAllStorageKeys()
    {
        return m_ConfigCaches.Keys;
    }

    internal static Dictionary<string, object> GetStorage(string key)
    {
        if (m_ConfigCaches.TryGetValue(key, out var config))
        {
            return config;
        }
        return null;
    }

    public static T GetValue<T>(string valueKey, string storageKey = k_StorageDefaultKey)
    {
        var storage = GetOrCreateStorage(storageKey);
        if (storage.TryGetValue(valueKey, out var valueObj) && valueObj is T value)
        {
            return value;
        }
        else
        {
            return default;
        }
    }

    public static T GetValueOrDefault<T>(string valueKey, T defaultValue, string storageKey = k_StorageDefaultKey)
    {
        var storage = GetOrCreateStorage(storageKey);
        if (storage.TryGetValue(valueKey, out var valueObj) && valueObj is T value)
        {
            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public static T GetValueOrElse<T>(string valueKey, Func<T> defaultValueFunc, string storageKey = k_StorageDefaultKey)
    {
        var storage = GetOrCreateStorage(storageKey);
        if (storage.TryGetValue(valueKey, out var valueObj) && valueObj is T value)
        {
            return value;
        }
        else
        {
            return defaultValueFunc();
        }
    }

    public static void SetValue<T>(string valueKey, T value, string storageKey = k_StorageDefaultKey)
    {
        var storage = GetOrCreateStorage(storageKey);
        if (storage[valueKey] is T origin && EqualityComparer<T>.Default.Equals(origin, value) is false)
        {
            m_ConfigDirtyMask[valueKey] = true;
        }
        storage[valueKey] = value;
    }
}

#endif