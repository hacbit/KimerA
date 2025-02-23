using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace KimerA.Db
{
    [Serializable]
    public sealed class SqlResult
    {
        [ShowInInspector]
        public bool Success { get; set; }
        [ShowInInspector]
        public string Message { get; set; }
        [ShowInInspector]
        public List<Dictionary<string, object>> Data { get; set; }

        public SqlResult()
        {
            Success = false;
            Message = string.Empty;
            Data = new List<Dictionary<string, object>>();
        }
    }
}