namespace KimerA.ECS
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GenericGenerateAttribute : Attribute
    {
        public uint StartGenericCount;

        public uint MaxGenericCount;

        public string NamespaceName;

        public string MethodTemplate;

        public string TypeName;

        public string TypePrefix;

        public string Constraint;

        public GenericGenerateAttribute(
            uint startGenericCount,
            uint maxGenericCount,
            string namespaceName,
            string methodTemplate,
            string typeName,
            string typePrefix = "T",
            string constraint = ""
        )
        {
            StartGenericCount = startGenericCount;
            MaxGenericCount = maxGenericCount;
            NamespaceName = namespaceName;
            MethodTemplate = methodTemplate;
            TypeName = typeName;
            TypePrefix = typePrefix;
            Constraint = constraint;
        }
    }
}