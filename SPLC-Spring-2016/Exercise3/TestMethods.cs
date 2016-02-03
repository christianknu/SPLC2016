using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            B.SM();
            C.SM();
            B b = new C();
            C c = new C();
            b.VIM();
            b.NIM();
            c.VIM();
            c.NIM();
        }
    }
}
