namespace KimerA.ECS
{
    using System;

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class SystemFunctionGenerateAttribute : Attribute
    {
        public uint MaxGenericCount { get; }

        public SystemFunctionGenerateAttribute(uint maxGenericCount)
        {
            MaxGenericCount = maxGenericCount;
        }
    }
}