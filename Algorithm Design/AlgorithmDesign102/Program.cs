using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        List<string> heroes = new List<string> { "Sollux", "Garrosh", "Thrall", "Talanji" };
        List<string> currentParty = new List<string>();

        foreach (var hero in heroes)
        {
            currentParty.Add(hero);
            Console.WriteLine("The heroes in the party are: " + JoinWithAnd(currentParty, true)); // With serial comma
            Console.WriteLine("The heroes in the party are: " + JoinWithAnd(currentParty, false)); // Without serial comma
        }
    }

    static string JoinWithAnd(List<string> items, bool useSerialComma)
    {
        int count = items.Count;
        if (count == 0) return "";
        if (count == 1) return items[0];
        if (count == 2) return items[0] + " and " + items[1];

        string separator = useSerialComma ? ", and " : " and ";
        return string.Join(", ", items.GetRange(0, count - 1)) + separator + items[count - 1];
    }
}
