#if UNITY_EDITOR

using System;
using System.IO;
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

    /// <summary>
    /// 配置资源路径
    /// </summary>
    public const string ConfigResPath = "Assets/ConfigRes";

    private static void InitRootPath()
    {
        if (File.Exists($"{Application.dataPath}/Plugins/KimerA/package.json"))
        {
            _libraryRootPath = "Assets/Plugins/KimerA";
            return;
        }

        var packagePath = $"Packages/{PACKAGE_IDENTIFY}/package.json";
        var json = AssetDatabase.LoadAssetAtPath<TextAsset>(packagePath);
        if (json is not null)
        {
            _libraryRootPath = $"Packages/{PACKAGE_IDENTIFY}";
            return;
        }

        Debug.Assert(string.IsNullOrEmpty(_libraryRootPath) is false, "KimerA 根目录加载失败");
    }

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

    public static string RelativeOf(string path, string basePath)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(basePath))
            return path;

        if (Path.IsPathRooted(path))
        {
            path = path.Replace("\\", "/");
            basePath = basePath.Replace("\\", "/");
            if (path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return path[basePath.Length..].TrimStart('/');
            }
        }
        return path;
    }
}

#endif