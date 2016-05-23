using System;

namespace Week03
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Yield demo:");
            GalaxyClass.ShowGalaxies();
            Console.WriteLine("\n");
            
            // Exercise 1
            Console.WriteLine("Exercise 1:");
            IntAction a = x => TestDelegate.PrintInt(x);
            a(42);
            int[] arr = {3, 4, 14, 53, 54, 89, 11};
            TestDelegate.Perform(a, arr);
            TestDelegate.Perform(Console.WriteLine, arr);
            TestDelegate.Perform(Console.WriteLine, 1, 4, 8, 16, 32);
            Console.WriteLine("\n");
            
            // Exercise 2
            Console.WriteLine("Exercise 2:");
            Book[] myBooks = {
                new Book("Peter Sestoft", "Programming Language Concepts", 2012),
                new Book("Walt Disney", "Anders And", 1983),
                new Book("Peter Sestoft & Henrik I. Hansen", "C# Precisely (Second Edition)", 2012),
                new Book("Raghu Ramakrishnan & Johannes Gehrke", "Database Management Systems", 2003),
                new Book("David Basin, Patrick Schaller & Michael Schläpfer", "Applied Information Security", 2011),
                new Book("Y. Daniel Liang", "Introduction to Java Programming (Brief Version)", 2015),
                new Book("Lars Mathiassen, Andreas Munk-Madsen, Peter Axel Nielsen & Jan Stage", "Object Oriented Analysis & Design", 2000),
                new Book("Ian Sommerville", "Software Engineering", 2016),
                new Book("Susanna S. Epp", "Discrete Mathematics with Applications", 2011)
                };
            
            //myBooks = GenericMethods.Filter(myBooks, x => (x.year < 2010));
            //GenericMethods.Quicksort(myBooks, (x, y) => MyComparer.BookCompare(x, y));
            
            String[] bookTitles = GenericMethods.Map<Book,String>(myBooks, x => x.title);
            
            foreach (var book in myBooks)
            {
                Console.WriteLine(book);
            }
            
            foreach(var bookTitle in bookTitles){
                Console.WriteLine(bookTitle);
            }
            Console.WriteLine("\n");
        }
    }
}
