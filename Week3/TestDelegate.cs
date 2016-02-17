using System;

namespace Week3
{
    public delegate void IntAction(int myInt); 
    public class TestDelegate
    {
        public static void PrintInt(int myInteger){
            Console.WriteLine(myInteger);
        }
        
        public static void Perform(IntAction act, params int[] arr) {
            foreach(int i in arr) {
                act(i);
            }
        }
    }
}
