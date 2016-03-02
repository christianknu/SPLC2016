using System;
using System.Collections.Generic;

namespace Week4
{
    public delegate bool Func<T, Boolean>(T x);

    class Program
    {
        public static void Main()
        {
            // Exercise 1.1
            Console.WriteLine("Exercise 1.1:");
            int[] myArray = { 3, 5, 2, 5, 5, 3, 90, 345, 23, 17 };
            int[] myArray2 = { 2, 4, 6, 8, 10, 12, 14, 16, 18 };
            Func<int, bool> myFunc = x => (x % 2 == 0);

            Console.WriteLine("Is all numbers even?: " + Forall(myArray, myFunc));         // Returns false
            Console.WriteLine("Is all numbers even?: " + Forall(myArray2, myFunc));        // Returns true

            Console.WriteLine("Is at least one of the numbers even?: " + Exists(myArray, myFunc));
            Console.WriteLine("\n");

            // Exercise 1.2
            Console.WriteLine("Exercise 1.2:");
            int[][] myTwoDimArray = { new int[] { 4, 2, 5, 3 }, new int[] { 19, 22, 16, 80 }, new int[] { 47, 19, 60, 40 }, new int[] { 2, 6 } };
            Func<int[], bool> myFuncArray = x => x.Length > 2;

            Console.WriteLine("Does every array has has more than two elements?: " + Forall<int[]>(myTwoDimArray, myFuncArray));

            // Exercise 1.3
            List<int> firstList = new List<int>();
            firstList.Add(3); firstList.Add(6); firstList.Add(8); firstList.Add(10); firstList.Add(13);

            List<int> secondList = new List<int>();
            secondList.Add(1); secondList.Add(2); secondList.Add(7); secondList.Add(10);
            List<int> mergedLists = Merge(firstList, secondList);


            String output = "";
            foreach (var item in mergedLists)
            {
                output = output.Insert(output.Length, item.ToString() + " ");
            }
            Console.WriteLine(output);
        }

        static bool Forall<T>(T[] xs, Func<T, bool> p)
        {
            bool doesItHold = false;
            foreach (T instance in xs)
            {
                if (!p(instance))
                {
                    doesItHold = false;
                    break;
                }
                else
                {
                    doesItHold = true;
                }
            }
            return doesItHold;
        }

        static bool Exists<T>(T[] xs, Func<T, bool> p)
        {
            bool doesItHold = false;
            foreach (T item in xs)
            {
                if (p(item))
                {
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

        static List<T> Merge<T>(List<T> listA, List<T> listB) where T : IComparable<T>
        {
            List<T> tempList = new List<T>();
            int i = 0;
            int j = 0;
            int listACount = listA.Count;
            int listBCount = listB.Count;
            
            while (i < listACount && j < listBCount)
            {
                if (listA[i].CompareTo(listB[j]) == 0)
                {
                    tempList.Add(listA[i++]);
                    tempList.Add(listB[j++]);
                }
                else if (listA[i].CompareTo(listB[j]) < 0)
                {
                    tempList.Add(listA[i++]);
                }
                else
                {
                    tempList.Add(listB[j++]);
                }
            }
            if (i < listACount)
            {
                for (int p = i; p < listACount; p++)
                {
                    tempList.Add(listA[p]);
                }
            }
            else
            {
                for (int p = j; p < listBCount; p++)
                {
                    tempList.Add(listB[p]);
                }
            }

            return tempList;
        }
    }
}
