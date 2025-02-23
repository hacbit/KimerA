namespace KimerA.Db
{
    public enum SqlDataType
    {
        Interger,
        Real,
        Text,
        Blob,
        Null,
    }

    public static class SqlDataTypeExtension
    {
        public static string ToPureSqlString(this SqlDataType type)
        {
            return type switch
            {
                SqlDataType.Interger => "INTEGER",
                SqlDataType.Real => "REAL",
                SqlDataType.Text => "TEXT",
                SqlDataType.Blob => "BLOB",
                SqlDataType.Null => "NULL",
                _ => throw new System.NotImplementedException(),
            };
        }

        public static SqlDataType ToSqlDataType(this string type)
        {
            return type switch
            {
                "INTEGER" => SqlDataType.Interger,
                "REAL" => SqlDataType.Real,
                "TEXT" => SqlDataType.Text,
                "BLOB" => SqlDataType.Blob,
                "NULL" => SqlDataType.Null,
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}