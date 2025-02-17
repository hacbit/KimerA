using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KimerA
{
    internal static class FileSystem
    {
        /// <summary>
        /// The type of path to read/write.
        /// </summary>
        public enum PathType
        {
            /// <summary>
            /// The project's folder path.
            /// </summary>
            Assets,

            /// <summary>
            /// The streaming assets path.
            /// </summary>
            Runtime,

            /// <summary>
            /// The persistent data path.
            /// </summary>
            AppData,

            /// <summary>
            /// The absolute path.
            /// </summary>
            Absolute,
        }

        public static string GetPath(PathType pathType, string path)
        {
            var basePath = pathType switch
            {
                PathType.Assets => Application.dataPath,
                PathType.Runtime => Application.streamingAssetsPath,
                PathType.AppData => Application.persistentDataPath,
                PathType.Absolute => string.Empty,
                _ => throw new ArgumentException("Invalid path type.")
            };

            return pathType == PathType.Absolute ? path : Path.Combine(basePath, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateDirectory(string path, PathType pathType = PathType.Runtime)
        {
            path = GetPath(pathType, path);
            if (Directory.Exists(path))
            {
                return;
            }
            Directory.CreateDirectory(path);
        }

        public static void ClearDirectory(string path, PathType pathType = PathType.Runtime)
        {
            path = GetPath(pathType, path);
            if (Directory.Exists(path) == false)
            {
                return;
            }
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }

        public static void WriteText(string path, string text, PathType pathType = PathType.Runtime)
        {
            var fullPath = GetPath(pathType, path);
            CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, text);
        }

        public static string ReadText(string path, PathType pathType = PathType.Runtime)
        {
            var fullPath = GetPath(pathType, path);
            return File.ReadAllText(fullPath);
        }

        public static void WriteBytes(string path, byte[] bytes, PathType pathType = PathType.Runtime)
        {
            var fullPath = GetPath(pathType, path);
            CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, bytes);
        }

        public static byte[] ReadBytes(string path, PathType pathType = PathType.Runtime)
        {
            var fullPath = GetPath(pathType, path);
            return File.ReadAllBytes(fullPath);
        }
    }
}