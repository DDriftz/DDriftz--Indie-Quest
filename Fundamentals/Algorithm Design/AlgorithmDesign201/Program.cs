using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        List<string> participants = new List<string> { "Allie", "Ben", "Claire", "Dan", "Eleanor, Ross, Joey" };
        
        Console.WriteLine("Signed-up participants: " + string.Join(", ", participants));
        
        ShuffleList(participants);
        
        Console.WriteLine("\nGenerating starting order ...\n");
        Console.WriteLine("Starting order: " + string.Join(", ", participants));
    }

    static void ShuffleList(List<string> items)
    {
        Random rand = new Random();
        
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]); // Swap
        }
    }
}
