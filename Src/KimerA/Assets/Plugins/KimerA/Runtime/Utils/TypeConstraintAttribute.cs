//! DO NOT MODIFY THIS FILE !
//! If you need to change the TypeConstraintAttribute,
//! you should change the KimerA.Analysis project and rebuild analyzer

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

using System;

namespace KimerA.Utils
{
    /// <summary>
    /// TypeConstraintAttribute is like the 'where' keyword in C#.
    /// It is used to specify constraints on the parameter of a method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class TypeConstraintAttribute : Attribute
    {
        /// <summary>
        /// Constraints is a list of types that the type of parameter must contain or be derived from.
        /// </summary>
        public Type[] Constraints { get; } = Array.Empty<Type>();

        public TypeConstraintAttribute(params Type[] constraints)
        {
            Constraints = constraints;
        }
    }
}