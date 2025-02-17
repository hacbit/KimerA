#if UNITY_EDITOR

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public sealed class BuildAndImportAnalyzer : OdinEditorWindow
{
    [MenuItem("KimerA/Internal/Build and Import Analyzer")]
    private static void ShowWindow()
    {
        var window = GetWindow<BuildAndImportAnalyzer>("Build and Import Analyzer");
        window.Show();
    }

    [ShowInInspector, ReadOnly]
    private static readonly string projectBasePath = Path.GetFullPath(Application.dataPath + "/../..");

    [ShowInInspector]
    [ListDrawerSettings(ShowFoldout = true)]
    private static readonly Dictionary<string, bool> analyzerProjects = new();

    public struct BuildConfig
    {
        public List<string> analyzers;
    }

    [Button(ButtonSizes.Large)]
    private static void FindAnalyzers()
    {
        var buildConfigJson = Path.Combine(projectBasePath, "build.json");
        if (File.Exists(buildConfigJson) is false)
        {
            UnityEngine.Debug.LogError($"No build.json found in {projectBasePath}");
            return;
        }

        var config = JsonConvert.DeserializeObject<BuildConfig>(File.ReadAllText(buildConfigJson));
        
        analyzerProjects.Clear();
        foreach (var analyzer in Directory.GetDirectories(projectBasePath, "KimerA.Analysis.*", SearchOption.TopDirectoryOnly))
        {
            var analyzerName = Path.GetFileName(analyzer);
            analyzerProjects.Add(analyzerName, config.analyzers.Contains(analyzerName));
        }
    }

    [Button(ButtonSizes.Large)]
    private static async UniTask BuildAndImport()
    {
        var tasks = new List<UniTask<string?>>();
        foreach (var (analyzer, shouldBuild) in analyzerProjects)
        {
            if (shouldBuild is false)
            {
                UnityEngine.Debug.Log($"Skipping {analyzer}");
                continue;
            }

            var projectPath = Path.Combine(projectBasePath, analyzer, $"{analyzer}.csproj");
            if (File.Exists(projectPath) is false)
            {
                UnityEngine.Debug.LogWarning($"No project found at {projectPath}");
                continue;
            }

            var task = BuildAndImportAsync(projectPath);
            tasks.Add(task);
        }

        var token = new CancellationTokenSource();
        await UniTask.SwitchToMainThread(cancellationToken: token.Token);
        var targets = await UniTask.WhenAll(tasks);

        AssetDatabase.Refresh();
        await UniTask.Yield(PlayerLoopTiming.Update, token.Token);


        foreach (var target in targets)
        {
            if (target is null) continue;

            var relativePath = Path.GetRelativePath(Application.dataPath + "/..", target);
            var imported = AssetDatabase.LoadAssetAtPath<DefaultAsset>(relativePath);
            if (imported == null)
            {
                UnityEngine.Debug.LogError($"Failed to import {relativePath}");
                continue;
            }

            UnityEngine.Debug.Log($"Imported {imported.name} at {relativePath}");

            AssetDatabase.SetLabels(imported, new[] { "KimerA", "RoslynAnalyzer" });

            if (AssetImporter.GetAtPath(relativePath) is PluginImporter plugin)
            {
                UnityEngine.Debug.Log($"Setting plugin settings for {imported.name}");
                plugin.SetCompatibleWithAnyPlatform(false);
                plugin.SetCompatibleWithEditor(false);
                plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                plugin.SaveAndReimport();
            }
        }

        token.Cancel();
    }

    private static async UniTask<string?> BuildAndImportAsync(string projectPath)
    {
        // Build
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build {projectPath} -c Release",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        buildProcess.Start();

        var buildOutput = await buildProcess.StandardOutput.ReadToEndAsync();
        var buildError = await buildProcess.StandardError.ReadToEndAsync();
        buildProcess.WaitForExit();

        if (buildProcess.ExitCode != 0)
        {
            UnityEngine.Debug.LogError($"Build failed for {projectPath}\n{buildError}");
            return null;
        }

        UnityEngine.Debug.Log($"Build succeeded for {projectPath}\n{buildOutput}");

        // Import roslyn analyzer
        var dllPath = Path.Combine(Path.GetDirectoryName(projectPath), "bin", "Release", "netstandard2.0", $"{Path.GetFileNameWithoutExtension(projectPath)}.dll");

        var targetPath = Path.Combine(Application.dataPath, "Plugins", "KimerA", "Runtime", "Analyzers", $"{Path.GetFileNameWithoutExtension(projectPath)}.dll");
        File.Copy(dllPath, targetPath, true);

        return targetPath;
    }
}

#endif