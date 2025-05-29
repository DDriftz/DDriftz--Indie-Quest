using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string allParticipants = "Allie, Ben, Claire, Joey"; 
        List<string> participants = new List<string>(allParticipants.Split(new[] { ", " }, StringSplitOptions.None));

        Console.WriteLine("Signed-up participants: " + string.Join(", ", participants));
        Console.WriteLine("\nStarting orders:");

        WriteAllPermutations(participants, new List<string>(), new HashSet<int>());
    }

    static void WriteAllPermutations(List<string> items, List<string> current, HashSet<int> used)
    {
        if (current.Count == items.Count)
        {
            Console.WriteLine(string.Join(", ", current));
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (used.Contains(i)) continue;

            used.Add(i);
            current.Add(items[i]);
            WriteAllPermutations(items, current, used);
            current.RemoveAt(current.Count - 1);
            used.Remove(i);
        }
    }
}