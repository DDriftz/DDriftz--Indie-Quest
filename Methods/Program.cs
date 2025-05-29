using System;
using System.Collections.Generic;

class RPGCombatSimulator
{
    static Random random = new Random();

    static void Main()
    {
        var heroes = new List<string> { "Jazlyn", "Theron", "Dayana", "Rolando" };
        Console.WriteLine($"Fighters {string.Join(", ", heroes)} descend into the dungeon.\n");

        // Orc: 2d8 + 6 HP, DC 10
        SimulateCombat(heroes, "orc", DiceRoll(2, 8, 6), 10);
        if (heroes.Count == 0) return;

        // Azer: 6d8 + 12 HP, DC 18
        SimulateCombat(heroes, "azer", DiceRoll(6, 8, 12), 18);
        if (heroes.Count == 0) return;

        // Troll: 8d10 + 40 HP, DC 16
        SimulateCombat(heroes, "troll", DiceRoll(8, 10, 40), 16);
        if (heroes.Count == 0) return;

        Console.WriteLine($"After three grueling battles, the heroes {string.Join(", ", heroes)} return from the dungeons to live another day.");
    }

    static int DiceRoll(int numberOfRolls, int diceSides, int fixedBonus = 0)
    {
        int total = fixedBonus;
        for (int i = 0; i < numberOfRolls; i++)
        {
            total += random.Next(1, diceSides + 1); // +1 because Next is exclusive
        }
        return total;
    }

    static void SimulateCombat(List<string> characterNames, string monsterName, int monsterHP, int savingThrowDC)
    {
        Console.WriteLine($"Watch out, {monsterName} with {monsterHP} HP appears!");

        while (monsterHP > 0 && characterNames.Count > 0)
        {
            // Attack phase
            for (int i = 0; i < characterNames.Count; i++)
            {
                if (monsterHP <= 0) break;

                string fighter = characterNames[i];
                int damage = DiceRoll(2, 6); // greatsword: 2d6
                monsterHP -= damage;
                if (monsterHP < 0) monsterHP = 0;

                Console.WriteLine($"{fighter} hits the {monsterName} for {damage} damage. The {monsterName} has {monsterHP} HP left.");
            }

            if (monsterHP <= 0) break;

            // Monster attacks a random hero
            string target = characterNames[random.Next(characterNames.Count)];
            Console.WriteLine($"\nThe {monsterName} attacks {target}!");

            int roll = DiceRoll(1, 20);
            int total = roll + 3; // Constitution modifier +3

            if (total >= savingThrowDC)
            {
                Console.WriteLine($"{target} rolls a {roll} and is saved from the attack.\n");
            }
            else
            {
                Console.WriteLine($"{target} rolls a {roll} and fails to be saved. {target} is killed.\n");
                characterNames.Remove(target);
            }
        }

        if (characterNames.Count == 0)
        {
            Console.WriteLine($"The party has failed and the {monsterName} continues to attack unsuspecting adventurers.\n");
        }
        else
        {
            Console.WriteLine($"The {monsterName} collapses and the heroes celebrate their victory!\n");
        }
    }
}
