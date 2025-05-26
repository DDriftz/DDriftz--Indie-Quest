using System;
using System.Collections.Generic;
using System.Threading;

namespace Algorithm_Design_Unit_7_Mission_3
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
            var data = new List<int>();
            var random = new Random();

            for (int i = 0; i < 70; i++)
            {
                data.Add(random.Next(20));
                DisplayData(data);
            }

          
            for (int sortingRange = data.Count; sortingRange > 0; sortingRange--)
            {
                
                for (int i = 0; i < sortingRange; i++)
                {
                    if (i < sortingRange - 1)
                    {
                       
                        if (data[i + 1] < data[i])
                        {
                            
                            int temp = data[i + 1];
                            data[i + 1] = data[i];
                            data[i] = temp;
                        }

                       
                        DisplayData(data);
                    }
                }
            }
        }

    }
}