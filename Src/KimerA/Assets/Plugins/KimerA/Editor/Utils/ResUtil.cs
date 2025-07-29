#if UNITY_EDITOR

using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System;

namespace KimerA.Editor.Utils;

internal static class ResUtil
{
    public static T ResolveAsset<T>(string relativeOfRoot) where T : Object
    {
        var path = PathUtil.Combine(PathUtil.RootPath, relativeOfRoot);
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        return asset;
    }

    public static T ResolveEditorAsset<T>(string relativeOfEditor) where T : Object
    {
        var path = PathUtil.Combine(PathUtil.EditorPath, relativeOfEditor);
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        return asset;
    }

    public static IEnumerable<string> ResolveAllAssetPaths(string relativeDirPath)
    {
        var path = PathUtil.Combine(PathUtil.RootPath, relativeDirPath);
        return ResolveAllAssetPaths_Internal(path);
    }

    public static IEnumerable<string> ResolveEditorAllAssetPaths(string relativeDirPath)
    {
        var path = PathUtil.Combine(PathUtil.EditorPath, relativeDirPath);
        return ResolveAllAssetPaths_Internal(path);
    }

    private static IEnumerable<string> ResolveAllAssetPaths_Internal(string path)
    {
        var guids = AssetDatabase.FindAssets("t:Object", new[] { path });
        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath) is false)
            {
                yield return assetPath;
            }
        }
    }

    public static IEnumerable<T> ResolveAllAssets<T>(string relativeDirPath) where T : Object
    {
        return ResolveAllAssets_Internal<T>(ResolveAllAssetPaths(relativeDirPath));
    }

    public static IEnumerable<T> ResolveEditorAllAssets<T>(string relativeDirPath) where T : Object
    {
        return ResolveAllAssets_Internal<T>(ResolveEditorAllAssetPaths(relativeDirPath));
    }

    private static IEnumerable<T> ResolveAllAssets_Internal<T>(IEnumerable<string> paths) where T : Object
    {
        foreach (var path in paths)
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset is not null)
            {
                yield return asset;
            }
        }
    }

    public static T ResolveConfigResAsset<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(PathUtil.Combine(PathUtil.ConfigResPath, path));
    }

    public static IEnumerable<T> ResolveConfigResAllAssets<T>() where T : Object
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { PathUtil.ConfigResPath });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset is not null)
            {
                yield return asset;
            }
        }
    }
}


#endif