namespace KimerA.Utils
{
    using System.IO;

    public static class FileUtil
    {
        public static void CreateFile(string path)
        {
            if (File.Exists(path) == false)
            {
                DirectoryUtil.ApplyDirectory(Path.GetDirectoryName(path));
                File.Create(path).Close();
            }
        }
    }
}