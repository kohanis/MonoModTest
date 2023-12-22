using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using MonoMod.Core.Platforms;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace MonoModTest
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Test();
            MonoTest();
        }

        private static void Test()
        {
            
            unsafe {
                var clib = PlatformDetection.OS switch {
                    OSKind.Windows => "msvcrt",
                    _ => "c"
                };
                
                var msvcrt = DynDll.OpenLibrary(clib);
                var msvcrand = (delegate* unmanaged[Cdecl]<int>) DynDll.GetExport(msvcrt, "rand");
        
                using (new NativeHook((IntPtr) msvcrand, FakeRandom)) {
                    Helpers.Assert(msvcrand() == 1);
                }
                
                using (new Hook(() => EvenMoreFakeRandom(), FakeRandom)) {
                    Helpers.Assert(EvenMoreFakeRandom() == 1);
                }
            }
        }

        private static int FakeRandom(msvcrandDelegate orig)
        {
            return 1;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int EvenMoreFakeRandom()
        {
            return 0;
        }

        private delegate int msvcrandDelegate();

        private static void MonoTest()
        {
            if(Type.GetType ("Mono.Runtime") is null)
                return;
            
            var original = Assembly.GetExecutingAssembly();

            var getAssemblyMethod = typeof(Assembly).GetMethod(nameof(Assembly.GetExecutingAssembly), new Type[] { });
            using var getAssemblyHookNative = new NativeHook(PlatformTriple.Current.GetNativeMethodBody(getAssemblyMethod), GetAssemblyFix);

            var patched = Assembly.GetExecutingAssembly();
            
            Helpers.Assert(Equals(original, patched));
        }
        
        delegate Assembly GetAssemblyDelegate();
        
        private static Assembly GetAssemblyFix(GetAssemblyDelegate orig)
        {
            // return orig();
            return Assembly.GetAssembly(typeof(Program));
        }
    }
    

}