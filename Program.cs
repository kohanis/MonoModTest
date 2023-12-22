using System;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace MonoModTest
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Test();
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

        private static int EvenMoreFakeRandom()
        {
            return 0;
        }

        private delegate int msvcrandDelegate();
    }
    

}