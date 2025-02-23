using System;
using System.Reflection;
using KimerA.Utils;
using UnityEngine;

namespace KimerA.Test
{
    public class ReplaceTest : MonoBehaviour
    {
        private void Start()
        {
            var helper = new ReplaceTestHelper();
            helper.Start();
        }
    }

    public class ReplaceTestHelper
    {
        public void Start()
        {
            Debug.Log("Redirect : MethodRedirect.Scenario1.InternalInstanceMethod()");
            Debug.Log("To       : MethodRedirect.Scenario1.PrivateInstanceMethod()");

            Type Scenario_Type = typeof(ReplaceTestHelper);

            MethodInfo Scenario_InternalInstanceMethod = Scenario_Type.GetMethod("InternalInstanceMethod", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo Scenario_PrivateInstanceMethod = Scenario_Type.GetMethod("PrivateInstanceMethod", BindingFlags.Instance | BindingFlags.NonPublic);

            var token = Scenario_InternalInstanceMethod.RedirectTo(Scenario_PrivateInstanceMethod);

            // Using "dynamic" type to resolve the following issue in x64 and Release (with code optimizations) builds.
            //
            // Symptom: the second call to method InternalInstanceMethod() does not return the expected string value
            //
            // Cause: the result from the first call to method InternalInstanceMethod() is cached and so the second
            // call gets the cached value instead of making the call again.
            //
            // Remark: for some reason, without "dynamic" type, only the "Debug x86" build configuration would reevaluate 
            // the second call to InternalInstanceMethod() without using the cached string value.
            //
            // Also, using the [MethodImpl(MethodImplOptions.NoOptimization)] attribute on the method did not work.
            dynamic scenario = (ReplaceTestHelper)Activator.CreateInstance(Scenario_Type);

            string methodName = scenario.InternalInstanceMethod();

            Debug.LogFormat("Call MethodRedirect.Scenario1.InternalInstanceMethod => {0}", methodName);

            Debug.Assert(methodName == "MethodRedirect.Scenario1.PrivateInstanceMethod");

            if (methodName == "MethodRedirect.Scenario1.PrivateInstanceMethod")
            {
                Debug.Log("\nRestore...");
                
                token.Restore();

                Debug.Log(token);

                methodName = scenario.InternalInstanceMethod();

                Debug.LogFormat("Call MethodRedirect.Scenario1.InternalInstanceMethod => {0}", methodName);

                Debug.Assert(methodName == "MethodRedirect.Scenario1.InternalInstanceMethod");

                if (methodName == "MethodRedirect.Scenario1.InternalInstanceMethod")
                {
                    Debug.Log("\nSUCCESS!");
                }
                else
                {
                    Debug.Log("\nRestore FAILED");                    
                }
            }
            else
            {
                Debug.Log("\nRedirection FAILED");
            }

            Console.ReadKey();
        }
        
        internal string InternalInstanceMethod()
        {
            return "MethodRedirect.Scenario1.InternalInstanceMethod";
        }
        
        private string PrivateInstanceMethod()
        {
            return "MethodRedirect.Scenario1.PrivateInstanceMethod";
        }

        public string PublicInstanceMethod()
        {
            return "MethodRedirect.Scenario1.PublicInstanceMethod";
        }

        public static string PublicStaticMethod()
        {
            return "MethodRedirect.Scenario1.PublicStaticMethod";
        }
    }
}