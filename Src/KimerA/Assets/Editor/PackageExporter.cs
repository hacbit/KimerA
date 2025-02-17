#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

public static class PackageExporter
{
    [MenuItem("KimerA/Internal/Export Unitypackage")]
    public static void Export()
    {
        var root = "Plugins/KimerA";
        var version = GetVersion(root);

        var fileName = version.IsNullOrWhitespace() ? "KimerA.unitypackage" : $"KimerA.{version!.Trim()}.unitypackage";
        var exportPath = "./" + fileName;

        var path = Path.Combine(Application.dataPath, root);
        var assets = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
            .Where(x => Path.GetExtension(x) is ".cs" or ".asmdef" or ".json" or ".meta" or ".dll" or ".props")
            .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
            .ToArray();

        Debug.Log($"Export below files" + Environment.NewLine + string.Join(Environment.NewLine, assets));

        if (EditorUtility.DisplayDialog("Export Unitypackage", $"Exporting {assets.Length} files to {Path.GetFullPath(exportPath)}", "OK", "Cancel"))
        {
            AssetDatabase.ExportPackage(assets, exportPath, ExportPackageOptions.Default);
            Debug.Log($"Export complete: {Path.GetFullPath(exportPath)}");
        }
    }

    private static string? GetVersion(string root)
    {
        var version = Environment.GetEnvironmentVariable("UNITY_PACKAGE_VERSION");
        var versionJson = Path.Combine(Application.dataPath, root, "package.json");

        if (File.Exists(versionJson))
        {
            var ver = JsonConvert.DeserializeObject<Version>(File.ReadAllText(versionJson));

            if (version.IsNullOrWhitespace() is false && ver?.version != version)
            {
                var msg = $"""
                package.json and env version are **MISMATCHED**.
                UNITY_PACKAGE_VERSION: {version}, package.json: {ver?.version}
                """;

                if (Application.isBatchMode)
                {
                    Console.WriteLine(msg);
                    Application.Quit(1);
                }

                throw new Exception(msg);
            }

            version = ver?.version;
        }

        return version;
    }

    public class Version
    {
        public required string version;
    }
}

#endif