using System;

namespace KimerA.Utils
{
#region TypeConstraintAttribute
    // TODO: 添加对 TypeConstraintAttribute 参数的合法性检查
    // 比如：
    // 函数参数类型本身与 Constraint 中的类型存在冲突
    // void TestMethod([TypeConstraint(typeof(string))] int a) {}
    // 预期应该报错，因为 string 无法限定 int， 也就是说必须保证 Constraint 中的类型范围都是函数参数类型范围的子集

    // TODO: 允许 `AllowMultiple = true`，以便在单个参数上允许多个约束
    // 示例：
    // void TestMethod([TypeConstraint(typeof(IA))][TypeConstraint(typeof(IB))] object obj) {}
    // 该方法应接受实现了 IA 和 IB 接口的对象
    // 换言之，单个 TypeConstraintAttribute 内的约束是 'or' 关系，多个 TypeConstraintAttribute 之间的约束是 'and' 关系

    /// <summary>
    /// TypeConstraintAttribute 可以用于约束函数参数的类型，使得函数的实参类型必须包含或派生自指定的类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class TypeConstraintAttribute : Attribute
    {
        /// <summary>
        /// 约束类型
        /// </summary>
        public Type[] Constraints { get; } = Array.Empty<Type>();

        public TypeConstraintAttribute(params Type[] constraints)
        {
            Constraints = constraints;
        }
    }
#endregion

#region Archive
    /// <summary>
    /// 标记一个类为 Archive Sender
    /// </summary>
    /// <typeparam name="TArchive"></typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ArchiveToAttribute<TArchive> : Attribute
    {

    }

    /// <summary>
    /// 标记一个类为 Archive Receiver, 用于管理存档的读写
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ArchiveReceiverAttribute : Attribute
    {

    }

    /// <summary>
    /// 标记一个字段或属性为可存档
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ArchivableAttribute : Attribute
    {

    }
#endregion


}