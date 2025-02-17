namespace KimerA.ECS
{
    using System;

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class GenericStructGenerateAttribute : Attribute
    {
        public uint MinGenericCount { get; }

        public uint MaxGenericCount { get; }

        public GenericStructGenerateAttribute(uint minGenericCount, uint maxGenericCount)
        {
            MinGenericCount = minGenericCount;
            MaxGenericCount = maxGenericCount;
        }
    }
}