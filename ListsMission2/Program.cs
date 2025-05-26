using System;
using System.Collections.Generic;

class BasiliskBattleFull
{
    static void Main()
    {
        var random = new Random();
        var party = new List<string> { "Jazlyn", "Theron", "Dayana", "Rolando" };

        Console.WriteLine($"Fighters {string.Join(", ", party)} descend into the dungeon.");

        int basiliskHP = RollDice(random, 8, 8) + 16;
        Console.WriteLine($"\nA basilisk with {basiliskHP} HP appears!\n");

        while (basiliskHP > 0 && party.Count > 0)
        {
            // Attack round — each surviving party member attacks with a dagger
            for (int i = 0; i < party.Count; i++)
            {
                if (basiliskHP <= 0) break;

                string fighter = party[i];
                int damage = random.Next(1, 5); // dagger (1d4)
                basiliskHP -= damage;
                if (basiliskHP < 0) basiliskHP = 0;

                Console.WriteLine($"{fighter} hits the basilisk for {damage} damage. Basilisk has {basiliskHP} HP left.");
            }

            if (basiliskHP <= 0 || party.Count == 0) break;

            // Basilisk gaze on random target
            string target = party[random.Next(party.Count)];
            Console.WriteLine($"\nThe basilisk uses petrifying gaze on {target}!");

            int roll = random.Next(1, 21); // d20
            int total = roll + 3;          // constitution modifier +3

            if (total >= 12)
            {
                Console.WriteLine($"{target} rolls a {roll} and is saved from the attack.\n");
            }
            else
            {
                Console.WriteLine($"{target} rolls a {roll} and fails to be saved. {target} is turned into stone.\n");
                party.Remove(target);
            }
        }

        // Ending
        if (party.Count == 0)
        {
            Console.WriteLine("The party has failed and the basilisk continues to turn unsuspecting adventurers to stone.");
        }
        else
        {
            Console.WriteLine("The basilisk collapses and the heroes celebrate their victory!");
        }
    }

    // Helper method for rolling dice
    static int RollDice(Random rand, int count, int sides)
    {
        int total = 0;
        for (int i = 0; i < count; i++)
            total += rand.Next(1, sides + 1);
        return total;
    }
}
