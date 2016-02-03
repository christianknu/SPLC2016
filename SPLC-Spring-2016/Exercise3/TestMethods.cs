using System;

namespace Exercise3
{
    class B
    {
        public static void SM() { Console.WriteLine("Hello from B.SM()"); }
        public virtual void VIM() { Console.WriteLine("Hello from B.VIM()"); }
        public void NIM() { Console.WriteLine("Hello from B.NIM()"); }
    }

    class C : B
    {
        public static new void SM() { Console.WriteLine("Hello from C.SM()"); }

        public override void VIM() { Console.WriteLine("Hello from C.VIM()"); }

        public new void NIM() { Console.WriteLine("Hello from C.NIM()"); }
    }

    class LetsRunIt
    {
        public static void Main()
        {
            Console.Write("\n_____________________________________\n\n");

            Console.WriteLine("B.SM() - Calls the static SM() method in the B class:");
            B.SM();
            Console.Write("_____________________________________\n");

            Console.WriteLine("\nC.SM() - Calls the static SM() method in the C class:");
            C.SM();
            Console.Write("_____________________________________\n");

            B b = new C();
            var c = new C();

            Console.WriteLine("\nb.VIM() - Calls the VIM() method in the C class, because the run-time type of the instance (which is C), not the compile-time type of the instance (which is B), determines the actual method implementation to invoke:");
            b.VIM();
            Console.Write("_____________________________________\n");

            Console.WriteLine("\nb.NIM() - Calls the NIM()-method in the B class, as b is of compile-time type B, and the compile-time type is used when the method is non-virtual:");
            b.NIM();
            Console.Write("_____________________________________\n");

            Console.WriteLine("\nc.VIM() - Calls the VIM()-method in the C class, as c is of compile-time type C, and the compile-time type is used when the method is virtual:");
            c.VIM();
            Console.Write("_____________________________________\n");

            Console.WriteLine("\nc.NIM() - Calls the NIM()-method in the C class, as c is of run-time type C, and the run-time type is used when the method is non-virtual:");
            c.NIM();
            Console.Write("_____________________________________\n");
        }
    }
}
