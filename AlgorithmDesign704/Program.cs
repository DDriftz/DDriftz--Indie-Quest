using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        var data = new List<int>() { 9, 5, 3, 1, 4, 2 };
        var random = new Random();

        // Bubble sort
        for (int sortingRange = data.Count - 1; sortingRange > 0; sortingRange--)
        {
            for (int i = 0; i < sortingRange; i++)
            {
                if (data[i + 1] < data[i])
                {
                    int temp = data[i];
                    data[i] = data[i + 1];
                    data[i + 1] = temp;
                }

                DisplayData(data);
            }
        }

        Console.WriteLine("Sorted: " + string.Join(", ", data));
    }

    static void DisplayData(List<int> data)
    {
        Console.WriteLine(string.Join(" ", data));
        Thread.Sleep(100);
    }
}
