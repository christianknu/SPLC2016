using System;

namespace Week4 {
	public delegate bool Func<Integer,Boolean>(int x);

	class Program
	{
		public static void Main()
		{
			// Exercise 1.1
            Console.WriteLine("Exercise 1.1:");
            int[] myArray = {3, 5, 2, 5, 5, 3, 90, 345, 23, 17};
            int[] myArray2 = {2, 4, 6, 8, 10, 12, 14, 16, 18};
			Func<int,bool> myFunc = (int x) => (x % 2 == 0);
            
            Console.WriteLine("Is all numbers even?: " + Forall(myArray, myFunc));         // Returns false
            Console.WriteLine("Is all numbers even?: " + Forall(myArray2, myFunc));        // Returns true
            
            Console.WriteLine("Is at least one of the numbers even?: " + Exists(myArray, myFunc));
            Console.WriteLine("\n");
		}

		static bool Forall(int[] xs, Func<int,bool> p)
		{
			bool doesItHold = false;
            foreach (int instance in xs) 
			{
				if(!p(instance)) {
					doesItHold = false;
					break;
				} else {
                    doesItHold = true;
                }
			}
            return doesItHold;
		}
        
        static bool Exists(int[] xs, Func<int,bool> p)
        {
            bool doesItHold = false;
            foreach (int item in xs)
            {
                if(p(item)){ 
                    doesItHold = true;
                    break;
                }
                else
                {
                    doesItHold = false;
                }
            }
            return doesItHold;
        }
	}
}
