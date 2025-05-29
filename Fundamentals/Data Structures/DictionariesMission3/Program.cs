using System;
using System.Collections.Generic;
using System.Linq;

class ScoreKeeper
{
    static void Main()
    {
        var scores = new Dictionary<string, int>();

        while (true)
        {
            Console.Write("Who won this round? ");
            string winner = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(winner))
                break;

            if (scores.ContainsKey(winner))
                scores[winner]++;
            else
                scores[winner] = 1;

            Console.WriteLine("\nRANKINGS");
            var sortedPlayers = scores.Keys.OrderByDescending(p => scores[p]);

            foreach (var player in sortedPlayers)
                Console.WriteLine($"{player} {scores[player]}");

            Console.WriteLine();
        }
    }
}
