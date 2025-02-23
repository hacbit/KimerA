namespace KimerA.Data.Excel
{
    using System.Collections.Generic;
    using System.IO;

    public static class CsvDataReader
    {
        public static List<T> ReadCsv<T>(string path) where T : IExcelStruct, new()
        {
            var result = new List<T>();
            
            using (var reader = new StreamReader(path))
            {
                var line = reader.ReadLine();
                var keys = line.Split(',');
                while (reader.EndOfStream == false)
                {
                    line = reader.ReadLine();
                    var values = line.Split(',');
                    var item = new T();
                    for (var i = 0; i < keys.Length; i++)
                    {
                        
                    }
                    result.Add(item);
                }
            }

            return result;
        }
    }
}
