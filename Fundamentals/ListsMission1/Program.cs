using System;
using System.Collections.Generic;
using System.Linq;

class CharacterGenerator
{
    static void Main()
    {
        var random = new Random();
        var abilityScores = new List<int>();

        for (int i = 0; i < 6; i++)
        {
            var rolls = new List<int>();
            for (int j = 0; j < 4; j++)
                rolls.Add(random.Next(1, 7));

            rolls.Sort();
            rolls.RemoveAt(0); // remove the lowest
            int score = rolls.Sum();

            Console.WriteLine($"You roll {string.Join(", ", rolls)}. The ability score is {score}.");
            abilityScores.Add(score);
        }

        abilityScores.Sort();
        Console.WriteLine($"\nYour available ability scores are: {string.Join(", ", abilityScores)}");
    }
}
