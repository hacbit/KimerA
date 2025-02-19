using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KimerA
{
    /// <summary>
    /// An utility class for file system operations.
    /// </summary>
    public static class FileSystem
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

        /// <summary>
        /// Get the full path by the specified path type and path.
        /// <para>if the path type is invalid, return null.</para>
        /// </summary>
        /// <param name="pathType"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? GetPath(PathType pathType, string? path)
        {
            var basePath = pathType switch
            {
                PathType.Assets => Application.dataPath,
                PathType.Runtime => Application.streamingAssetsPath,
                PathType.AppData => Application.persistentDataPath,
                PathType.Absolute => string.Empty,
                _ => null,
            };
            if (basePath is null) return null;
            path ??= string.Empty;
            return pathType == PathType.Absolute ? path : Path.Combine(basePath, path);
        }

        /// <summary>
        /// Create a directory by the specified path type and path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathType"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateDirectory(string? path, PathType pathType = PathType.AppData)
        {
            var fullPath = GetPath(pathType, path);
            if (fullPath is null || Directory.Exists(fullPath))
            {
                return;
            }
            Directory.CreateDirectory(fullPath);
        }

        /// <summary>
        /// Clear the directory by the specified path type and path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathType"></param>
        public static void ClearDirectory(string? path, PathType pathType = PathType.AppData)
        {
            var fullPath = GetPath(pathType, path);
            if (fullPath is null || Directory.Exists(fullPath) is false)
            {
                return;
            }
            Directory.Delete(fullPath, true);
            Directory.CreateDirectory(fullPath);
        }

        /// <summary>
        /// Write text to the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public static void WriteText(string? path, string text, PathType pathType = PathType.AppData)
        {
            var fullPath = GetPath(pathType, path);
            if (fullPath is null) return;
            CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, text);
        }

        /// <summary>
        /// Read text from the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public static string? ReadText(string? path, PathType pathType = PathType.AppData)
        {
            var fullPath = GetPath(pathType, path);
            if (fullPath is null || File.Exists(fullPath) == false)
            {
                return null;
            }
            return File.ReadAllText(fullPath);
        }

        /// <summary>
        /// Write bytes to the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        /// <param name="pathType"></param>
        public static void WriteBytes(string? path, byte[] bytes, PathType pathType = PathType.AppData)
        {
            var fullPath = GetPath(pathType, path);
            if (fullPath is null) return;
            CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, bytes);
        }

        /// <summary>
        /// Read bytes from the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(string? path, PathType pathType = PathType.AppData)
        {
            var fullPath = GetPath(pathType, path);
            if (fullPath is null || File.Exists(fullPath) == false)
            {
                return Array.Empty<byte>();
            }
            return File.ReadAllBytes(fullPath);
        }
    }
}