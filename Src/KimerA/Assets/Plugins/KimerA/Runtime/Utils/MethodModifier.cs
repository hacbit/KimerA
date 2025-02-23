using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KimerA.Utils
{
    public static class MethodModifier
    {
        public static MethodOperation RedirectTo<T>(this MethodInfo origin, Func<T> target)
            => RedirectTo(origin, target.Method);

        public static MethodOperation RedirectTo<T, R>(this MethodInfo origin, Func<T, R> target)
            => RedirectTo(origin, target.Method);

        public static MethodOperation RedirectTo(this MethodInfo origin, MethodInfo target)
        {
            var originPtr = GetMethodAddress(origin);
            var targetPtr = GetMethodAddress(target);

            // The function pointer gives the value located at the method address...
            // This can be used for validation that the method address matches
            Debug.Assert(Marshal.ReadIntPtr(originPtr) == origin.MethodHandle.GetFunctionPointer());
            Debug.Assert(Marshal.ReadIntPtr(targetPtr) == target.MethodHandle.GetFunctionPointer());

            return Redirect(originPtr, targetPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MethodRedirector Redirect(IntPtr origin, IntPtr target)
        {
            // Must create the token before address is assigned
            var token = new MethodRedirector(origin);
            // Redirect origin method to target method
            Marshal.Copy(new[] { Marshal.ReadIntPtr(target) }, 0, origin, 1);
            return token;
        }

        /// <summary>
        /// Obtain the unconditional jump address to the JIT-compiled method
        /// </summary>
        /// <param name="method"></param>
        /// <remarks>
        /// Before JIT compilation:
        ///   - call to PreJITStub to initiate compilation.            
        ///   - the CodeOrIL field contains the Relative Virtual Address (IL RVA) of the method implementation in IL.
        ///
        /// After on-demand JIT compilation:
        ///   - CRL changes the call to the PreJITStub for an unconditional jump to the JITed method.
        ///   - the CodeOrIL field contains the Virtual Address (VA) of the JIT-compiled method.
        /// </remarks>
        /// <returns>The JITed method address</returns>
        private static IntPtr GetMethodAddress(MethodInfo method)
        {
            const ushort SLOT_NUMBER_MASK = 0xffff; // 2 bytes mask
            const int MT_OFFSET_32BIT = 0x28;       // 40-bytes offset
            const int MT_OFFSET_64BIT = 0x40;       // 64-bytes offset

            IntPtr address;

            RuntimeHelpers.PrepareMethod(method.MethodHandle);

            var methodDescriptorAddress = method.MethodHandle.Value;
            var methodTableAddress = method.DeclaringType.TypeHandle.Value;

            if (method.IsVirtual)
            {
                // The fixed-size portion of the MethodTable structure depends on the process type:
                // For 32-bit process (IntPtr.Size == 4), the fixed-size portion is 40 (0x28) bytes
                // For 64-bit process (IntPtr.Size == 8), the fixed-size portion is 64 (0x40) bytes
                var offset = IntPtr.Size == 4 ? MT_OFFSET_32BIT : MT_OFFSET_64BIT;

                // First method slot = MethodTable address + fixed-size offset
                // This is the address of the first method of any type (i.e. ToString)
                var ms = Marshal.ReadIntPtr(methodTableAddress + offset);

                // Get the slot number of the virtual method entry from the MethodDesc data structure
                var shift = Marshal.ReadInt64(methodDescriptorAddress) >> 32;
                var slot = (int)(shift & SLOT_NUMBER_MASK);
                
                // Get the virtual method address relative to the first method slot
                address = ms + (slot * IntPtr.Size);                                
            }
            else
            {
                // Bypass default MethodDescriptor padding (8 bytes) 
                // Reach the CodeOrIL field which contains the address of the JIT-compiled code
                address = methodDescriptorAddress + 8;
            }

            return address;
        }
    }

    public abstract class MethodOperation : IDisposable
    {
        public abstract void Restore();

        public void Dispose()
        {
            Restore();
        }
    }

    public class MethodRedirector : MethodOperation
    {
        public MethodToken Token { get; private set; }

        public MethodRedirector(IntPtr address)
        {
            Token = new MethodToken(address);
        }

        public override void Restore()
        {
            Token.Restore();
        }

        public override string ToString()
        {
            return Token.ToString();
        }
    }

    public struct MethodToken : IDisposable
    {
        public IntPtr Address { get; private set; }
        public IntPtr Value { get; private set; }

        public MethodToken(IntPtr address)
        {
            Address = address;
            Value = Marshal.ReadIntPtr(address);
        }

        public void Restore()
        {
            Marshal.Copy(new[] { Value }, 0, Address, 1);
        }

        public override string ToString()
        {
            var method = Address;
            var target = Marshal.ReadIntPtr(Address);
            var origin = Value;

            return $@"Method address = {method.ToString("x").PadLeft(8, '0')}
Target address = {target.ToString("x").PadLeft(8, '0')}
Origin address = {origin.ToString("x").PadLeft(8, '0')}";
        }

        public void Dispose()
        {
            Restore();
        }
    }
}