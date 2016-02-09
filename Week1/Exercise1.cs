using System;

namespace SPLC_Spring_2016
{
    class Exercise1
    {
        static void Main()
        {
            Console.WriteLine("Hello World!\nTo exit this application just press Enter");
            Console.WriteLine("Please enter your name:");
            var userInput = Console.ReadLine();

            if (string.IsNullOrEmpty(userInput)) return;

            while (true)
            {
                Console.WriteLine("\nHello " + userInput);
                Console.WriteLine("Try to enter another name:");
                userInput = Console.ReadLine();

                if (!string.IsNullOrEmpty(userInput)){ continue; }

                break;
            }
        }
    }
}
