using System;
using System.Collections.Generic;

namespace Week4
{
    public class Generics
    {
        public static void Main()
        {
            List<Double> temperatures = new List<double>();
            Random myRandom = new Random();

            for (int i = 0; i < 20; i++)
            {
                temperatures.Add(GetRandomNumber(0.0, 40.0));
            }

            String tempStr = "";
            foreach (var el in temperatures)
            {
                tempStr = tempStr + el + " ";
            }
            Console.WriteLine(tempStr);
            Console.WriteLine("The number of elements of 'list' that are greater than or equal to '25': " + GreaterCount(temperatures, 25));
        }

        public Generics() { }
        
        static int GreaterCount(List<double> list, double min)
        {
            int highTemps = 0;
            foreach (var element in list)
            {
                if(element >= min)
                {
                    highTemps++;
                }
            }
            return highTemps;
        }

        static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}

