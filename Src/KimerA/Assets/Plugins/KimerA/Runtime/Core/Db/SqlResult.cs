using System;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace KimerA.Db
{
    [Serializable]
    public sealed class SqlResult
    {
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public bool Success { get; set; }
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public string Message { get; set; }
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public List<Dictionary<string, object>> Data { get; set; }

        public SqlResult()
        {
            Success = false;
            Message = string.Empty;
            Data = new List<Dictionary<string, object>>();
        }
    }
}