using System;

namespace KimerA
{
    /// <summary>
    /// DO NOT USE THIS INTERFACE DIRECTLY
    /// <para>This interface is used in Auto-Generated code</para>
    /// </summary>
    public interface IArchiveSender
    {
        object Save();

        void Load(Func<Type, object?> converter);
    }
}