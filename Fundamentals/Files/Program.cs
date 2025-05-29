using System;
using System.IO;
using System.Linq;

class AdventureGame
{
    static void Main()
    {
        string playerNamePath = "player-name.txt";
        string playerName;

        // Mission 1: Remember the player's name
        if (!File.Exists(playerNamePath))
        {
            Console.WriteLine("Welcome to your biggest adventure yet!\n");
            Console.WriteLine("What is your name, traveler?");
            Console.Write("> ");
            playerName = Console.ReadLine() ?? string.Empty;
            File.WriteAllText(playerNamePath, playerName);
            Console.WriteLine($"\nNice to meet you, {playerName}!");
        }
        else
        {
            playerName = File.ReadAllText(playerNamePath).Trim();
            Console.WriteLine($"\nWelcome back, {playerName}, let's continue!");
        }

        // Mission 2: Detect Kickstarter backers
        string backersPath = "backers.txt";

        if (File.Exists(backersPath))
        {
            string[] backers = File.ReadAllLines(backersPath);
            bool isBacker = backers.Any(b => b.Trim().Equals(playerName, StringComparison.OrdinalIgnoreCase));

            if (isBacker)
            {
                Console.WriteLine("\nYou successfully enter Dr. Fred's secret laboratory and are greeted with a warm welcome for backing the game's Kickstarter!");
            }
            else
            {
                Console.WriteLine("\nUnfortunately I cannot let you into Dr. Fred's secret laboratory.");
            }
        }
        else
        {
            Console.WriteLine("\nBackers file not found. No Kickstarter access check performed.");
        }
    }
}
