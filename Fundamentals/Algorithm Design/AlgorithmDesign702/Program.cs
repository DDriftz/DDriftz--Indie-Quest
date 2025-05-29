using System;
using System.Collections.Generic;
using System.Threading;

namespace Algorithm_Design_Unit_7_Mission_2
{
    internal class Program
    {

        static void DisplayData(List<int> data)
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            for (int y = 20; y >= 0; y--)
            {
                if (y % 5 == 0)
                {
                    Console.Write($"{y,3} |");
                }
                else
                {
                    Console.Write("    |");
                }

                for (int x = 0; x < data.Count; x++)
                {
                    if (y == 0)
                    {
                        Console.Write("-");
                        continue;
                    }

                    Console.Write(y <= data[x] ? "\u2592" : " ");
                }

                Console.WriteLine();
            }

            Thread.Sleep(10);
        }

        static void Main(string[] args)
        {
            var data = new List<int>() { 1, 14, 7, 3, 12, 10, 15, 4, 2, 9, 11, 5, 8, 6, 13 };

          
            int sortedCount = 1;

            do
            {
               
                int indexOfFirstUnsortedNumber = sortedCount;
                int firstUnsortedNumber = data[indexOfFirstUnsortedNumber];

                
                int testIndex = indexOfFirstUnsortedNumber - 1;

                while (data[testIndex] > firstUnsortedNumber)
                {
                  
                    data[testIndex + 1] = data[testIndex];

                    testIndex--;

                    DisplayData(data);
                }

                int insertionIndex = testIndex + 1;
                data[insertionIndex] = firstUnsortedNumber;

                sortedCount++;

                DisplayData(data);

            } while (sortedCount < data.Count);

            Console.WriteLine($"The sorted numbers are: {string.Join(", ", data)}");
        }
    }
}