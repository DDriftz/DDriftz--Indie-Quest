using System;

class Program
{
    static int RollDice(string diceNotation)
    {
        int numRolls = 0;
        int dieType = 0;
        int modifier = 0;

        // Check if there's a modifier (e.g., +40)
        if (diceNotation.Contains('+'))
        {
            var parts = diceNotation.Split('+');
            diceNotation = parts[0];
            modifier = int.Parse(parts[1]);
        }

        // Split the dice part (e.g., "3d6" -> numRolls = 3, dieType = 6)
        var diceParts = diceNotation.Split('d');
        numRolls = int.Parse(diceParts[0]);
        dieType = int.Parse(diceParts[1]);

        // Roll the dice and sum up the results
        int total = 0;
        Random rand = new Random();
        for (int i = 0; i < numRolls; i++)
        {
            total += rand.Next(1, dieType + 1);
        }

        return total + modifier;
    }

    static void Main(string[] args)
    {
        // Task 2: Generate character's strength by rolling 3d6
        int characterStrength = RollDice("3d6");
        Console.WriteLine($"A character with strength {characterStrength} was created.");

        // Task 3: Create a gelatinous cube with 8d10+40 HP
        int gelatinousCubeHP = RollDice("8d10+40");
        Console.WriteLine($"A gelatinous cube with {gelatinousCubeHP} HP appears!");

        // Task 4: Create an army of 100 gelatinous cubes and display their combined HP
        int totalArmyHP = 0;
        for (int i = 0; i < 100; i++)
        {
            totalArmyHP += RollDice("8d10+40");
        }
        Console.WriteLine($"Dear gods, an army of 100 cubes descends upon us with a total of {totalArmyHP} HP. We are doomed!");
    }
}
