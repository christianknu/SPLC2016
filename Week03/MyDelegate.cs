using System;
using System.Collections.Generic;

namespace Week03
{
    delegate bool GetNumbers(int n);
    public class MyDelegate
    {
        public static void RunDelegates() {
            int[] numbers = { 2, 7, 56, 19, 2, 7, 4, 11};
            IEnumerable<int> result = SelectNumbers(numbers, n => n < 20);
            foreach (var num in result)
                Console.Write("{0} ", num);
        }

        static IEnumerable<int> SelectNumbers(IEnumerable<int> numbers, GetNumbers selector)
        {
            foreach (var number in numbers)
                if(selector(number))
                    yield return number;
        }
    }
}