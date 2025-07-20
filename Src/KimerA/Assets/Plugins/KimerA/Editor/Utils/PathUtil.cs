#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace KimerA.Editor.Utils;

internal static class PathUtil
{
    private const string PACKAGE_IDENTIFY = "com.kimera.framework";

    private static string _libraryRootPath;

    private static string _editorPath;

    /// <summary>
    /// 获取库的根目录路径
    /// </summary>
    public static string RootPath
    {
        get
        {
            if (string.IsNullOrEmpty(_libraryRootPath))
            {
                InitRootPath();
            }
            return _libraryRootPath;
        }
    }

    /// <summary>
    /// 获取库的 Editor 目录路径
    /// </summary>
    public static string EditorPath
    {
        get
        {
            if (string.IsNullOrEmpty(_editorPath))
            {
                _editorPath = Combine(RootPath, "Editor");
            }
            return _editorPath;
        }
    }

    private static void InitRootPath()
    {
        if (TryFindPackageJson(out var packageJsonPath))
        {
            _libraryRootPath = Path.GetDirectoryName(packageJsonPath).Replace("\\", "/");

            return;
        }

        Debug.LogWarning("Failed to locate package.json. Using Assets as fallback.");
        _libraryRootPath = "Assets";
    }

    private static bool TryFindPackageJson(out string packageJsonPath)
    {
        var guids = AssetDatabase.FindAssets("package");
        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith("/package.json") is false)
            {
                continue;
            }
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset is null)
            {
                continue;
            }
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(asset.text);
            if (json.TryGetValue("name", out var name) && name.ToString() == PACKAGE_IDENTIFY)
            {
                packageJsonPath = assetPath;
                return true;
            }
        }

        packageJsonPath = null;
        return false;
    }

    /// <summary>
    /// 组合路径（跨平台兼容）
    /// </summary>
    public static string Combine(params string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return string.Empty;

        string result = paths[0];
        for (int i = 1; i < paths.Length; i++)
        {
            result = Path.Combine(result, paths[i]).Replace("\\", "/");
        }
        return result;
    }

    /// <summary>
    /// 使用相对于 Editor 的路径
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public static T ResolveEditorAsset<T>(string relativePath) where T : Object
    {
        var path = Combine(EditorPath, relativePath);
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        return asset;
    }
}

#endif