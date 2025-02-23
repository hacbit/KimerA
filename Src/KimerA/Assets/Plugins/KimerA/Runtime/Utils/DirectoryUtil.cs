namespace KimerA.Utils
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class DirectoryUtil
    {
        public static void ApplyDirectory(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }

        public static IEnumerable<string> GetFilePaths()
        {
            return Directory
                .GetDirectories("Assets", "*", SearchOption.AllDirectories)
                .Where(file =>
                    file.StartsWith(".") == false
                    && file.Contains("Plugins") == false
                    && file.Contains("Editor") == false
                ).Select(f => f.Replace("\\", "/")).Distinct();
        }

        public static string ExtractName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string ExtractFolder(string path)
        {
            return Path.GetDirectoryName(path);
        }
    }
}