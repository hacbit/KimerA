using KimerA.Utils;
using UnityEngine;

namespace KimerA.Test
{
public class TypeConstraintTest : MonoBehaviour
{
    public interface ITestInterface {}

    public class TestClass : ITestInterface {}

    public class TestClass2 : TestClass {}

    public void TestMethod(
        [TypeConstraint(typeof(ITestInterface), typeof(int))] object? obj,
        [TypeConstraint(typeof(string))] object? obj2,
        object? obj3
    )
    {
        Debug.Log("TestMethod called");
    }

    private void Start()
    {
        TestMethod(new TestClass(), "Hello", null);
        // error
        // TestMethod(new TestClass2(), 1, null);
        // error
        // TestMethod(new object(), "Hello", null);
        TestMethod(114514, "Hello", null);
        TestMethod(null, null, null);
    }
}
}