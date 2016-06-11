using System;
using System.Collections.Generic;

namespace Week04
{
    public class Generics
    {
        public static void GenericsMain()
        {
            // Exercise 2.1
            List<Double> temperatures = new List<double>();
            temperatures.Add(25.0); temperatures.Add(30.0); temperatures.Add(12.3); temperatures.Add(17.9); temperatures.Add(27.4);
            //Console.WriteLine("The number of elements of 'list' that are greater than or equal to '25': " + GreaterCount(temperatures, 25));

            // Exercise 2.2 & exercise 2.3
            Double[] temperatures2 = { 25, 30, 12.3, 17.9, 27.4 };
            int[] intTemperatures = { 25, 30, 12, 17, 27 };
            Console.WriteLine("The number of elements of 'temperatures2' that are greater than or equal to '25': " + GreaterCount(temperatures2, 25));
            Console.WriteLine("The number of elements of 'temperatures' that are greater than or equal to '25': " + GreaterCount(temperatures, 25));
            Console.WriteLine("The number of elements of 'temperatures' that are greater than or equal to '25': " + GreaterCount((IEnumerable<Double>)temperatures, 25));

            String[] myStrArray = { "Hej Jens", "Davs", "Hvordan går det?", "Helt fint, tak", "Godt at høre!" };
            Console.WriteLine("The number of elements of 'myStrArray' that are greater than or equal to 'Fedt, Fedt, Fedt!': " + GreaterCount(myStrArray, "Fedt, Fedt, Fedt!"));

            // Exercise 2.4
            Student s1 = new Student("215874-6589");
            Student s2 = new Student("254327-6571");
            Student s3 = new Student("964132-5731");
            IEnumerable<Student> xs = new Student[] { s1, s2, s3 };
            IEnumerable<Person> ys = xs;

            Student x = new Student("512486-4577");
            int result = GreaterCount(xs, x);
            Console.WriteLine(result);
        }

        static int GreaterCount(List<double> list, double min)
        {
            Console.WriteLine("\n----- GreaterCount(List<double> list, double min) -----");
            int highTemps = 0;
            foreach (var element in list)
            {
                if (element >= min)
                {
                    highTemps++;
                }
            }
            return highTemps;
        }

        static int GreaterCount(IEnumerable<double> eble, double min)
        {
            Console.WriteLine("\n----- GreaterCount(IEnumerable<double> eble, double min) -----");
            int highTemps = 0;
            foreach (var item in eble)
            {
                if (item >= min)
                {
                    highTemps++;
                }
            }
            return highTemps;
        }

        static int GreaterCount(IEnumerable<String> eble, String min)
        {
            Console.WriteLine("\n----- GreaterCount(IEnumerable<String> eble, String min) -----");
            int highTemps = 0;
            foreach (var item in eble)
            {
                if (item.CompareTo(min) >= 0)
                {
                    highTemps++;
                }
            }
            return highTemps;
        }

        static int GreaterCount<T>(IEnumerable<T> eble, T x) where T : IComparable<T>
        {
            Console.WriteLine("\n----- GreaterCount(IEnumerable<String> eble, String min) -----");
            int highTemps = 0;
            foreach (var item in eble)
            {
                if (item.CompareTo(x) >= 0)
                {
                    highTemps++;
                }
            }
            return highTemps;
        }
    }

    public class Person : IComparable<Person>
    {
        public String CPRNumber;
        public Person(String CPRNumber)
        {
            this.CPRNumber = CPRNumber;
        }
        public int CompareTo(Person other)
        {
            return string.Compare(this.CPRNumber, other.CPRNumber);
        }
    }

    public class Student : Person
    {
        public Student(String CPRNumber) : base(CPRNumber)
        {
            this.CPRNumber = CPRNumber;
        }
    }
}

