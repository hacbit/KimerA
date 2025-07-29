#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KimerA.Editor.Utils;

public static class EditorStorageUtil
{
    public const string k_StorageDefaultKey = "DefaultEditorStorage";

    private static EditorStorageSO GetOrCreateStorage(string storageKey = k_StorageDefaultKey)
    {
        var storage = ResUtil.ResolveConfigResAsset<EditorStorageSO>($"{storageKey}.asset");
        if (storage is null)
        {
            storage = ScriptableObject.CreateInstance<EditorStorageSO>();
            if (Directory.Exists(PathUtil.ConfigResPath) is false)
            {
                Directory.CreateDirectory(PathUtil.ConfigResPath);
            }
            AssetDatabase.CreateAsset(storage, PathUtil.Combine(PathUtil.ConfigResPath, $"{storageKey}.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return storage;
    }

    /// <summary>
    /// <inheritdoc cref="EditorStorageSO.GetValue{T}(string)"/>
    /// <para>找不到储存会自动创建 SO 文件</para>
    /// </summary>
    public static T GetValue<T>(string valueKey, string storageKey = k_StorageDefaultKey)
    {
        var storage = GetOrCreateStorage(storageKey);
        return storage.GetValue<T>(valueKey);
    }

    /// <summary>
    /// <inheritdoc cref="EditorStorageSO.GetValueOrDefault{T}(string, T))"/>
    /// <para>找不到储存会自动创建 SO 文件</para>
    /// </summary>
    public static T GetValueOrDefault<T>(string valueKey, T defaultValue, string storageKey = k_StorageDefaultKey)
    {
        var storage = GetOrCreateStorage(storageKey);
        return storage.GetValueOrDefault<T>(valueKey, defaultValue);
    }

    /// <summary>
    /// <inheritdoc cref="EditorStorageSO.GetValueOrElse{T}(string, Func{T})"/>
    /// <para>找不到储存会自动创建 SO 文件</para>
    /// </summary>
    public static T GetValueOrElse<T>(string valueKey, Func<T> defaultValueFunc, string storageKey = k_StorageDefaultKey)
    {
        var storage = GetOrCreateStorage(storageKey);
        return storage.GetValueOrElse(valueKey, defaultValueFunc);
    }

    /// <summary>
    /// <inheritdoc cref="EditorStorageSO.SetValue{T}(string, T)"/>
    /// <para>找不到储存会自动创建 SO 文件</para>
    /// </summary>
    public static void SetValue<T>(string valueKey, T value, string storageKey = k_StorageDefaultKey)
    {
        var storage = GetOrCreateStorage(storageKey);
        storage.SetValue(valueKey, value);
    }
}

#endif