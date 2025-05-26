using System;
using System.Threading;

class Program
{
    static void Main()
    {
        // Game introduction
        Console.Write("Welcome to Tanker World! What is your name? ");
        string? input = Console.ReadLine();
        string playerName = input ?? "Player";
        Console.WriteLine($"\nWelcome, {playerName}! We call upon you to defend your base from enemy tanks in battle that will decide your fate !! Good Luck Major !!");

        // Choose difficulty level
        int maxAttempts = SelectDifficulty();

        // Proceed to the tank battle
        Console.WriteLine("\nPress any key to proceed to the battlefield...");
        Console.ReadKey();
        Console.Clear();

        StartTankBattle(maxAttempts);
    }

    static int SelectDifficulty()
    {
        Console.WriteLine("\nChoose your difficulty level:");
        Console.WriteLine("1. Easy (7 attempts)");
        Console.WriteLine("2. Medium (5 attempts)");
        Console.WriteLine("3. Hard (3 attempts)");

        int choice;
        while (true)
        {
            Console.Write("\nEnter your choice (1-3): ");
            if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= 3)
                break;
            Console.WriteLine("Invalid input! Please enter 1, 2, or 3.");
        }

        return choice switch
        {
            1 => 7, // Easy
            2 => 5, // Medium
            3 => 3, // Hard
            _ => 5 // Default (Medium)
        };
    }

    static void StartTankBattle(int maxAttempts)
    {
        Random rand = new Random();
        int tankDistance = rand.Next(40, 71); // Random distance between 40 and 70
        bool tankDestroyed = false;

        Console.WriteLine("\nDANGER! An enemy tank is approaching our position.");
        Console.WriteLine("You control our artillery unit and are our only hope!");
        Console.WriteLine("\nHere is the battlefield map:\n");

        // Create battlefield
        string battlefield = "/" + new string('_', tankDistance - 2) + "T" + new string('_', 80 - tankDistance - 1);
        Console.WriteLine(battlefield);

        Console.WriteLine($"\nYou have {maxAttempts} shells to destroy the tank. Aim carefully!");

        // Player shooting loop
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            Console.Write($"\nAttempt {attempt}/{maxAttempts} - Enter your firing distance: ");
            int shotDistance;
            
            // Validate user input
            while (!int.TryParse(Console.ReadLine(), out shotDistance))
            {
                Console.Write("Invalid input! Enter a number: ");
            }

            // Shooting sound effect (Windows only)
            #if WINDOWS
            Console.Beep(500, 200);
            #endif

            // Check the result
            if (shotDistance == tankDistance)
            {
                Console.WriteLine("\n🎯 DIRECT HIT! The tank has been destroyed! You win! 🎉");
                tankDestroyed = true;
                ShowExplosionAnimation();
                break;
            }
            else if (shotDistance < tankDistance)
            {
                Console.WriteLine("Your shot fell SHORT. Increase your range!");
                #if WINDOWS
                Console.Beep(300, 200);
                #endif
            }
            else
            {
                Console.WriteLine("Your shot went TOO FAR. Reduce your range!");
                #if WINDOWS
                Console.Beep(200, 200);
                #endif
            }
        }

        // If the tank wasn't destroyed after max attempts
        if (!tankDestroyed)
        {
            Console.WriteLine("\n💥 Out of shells! The tank has reached your position. GAME OVER.");
            ShowLossScreen();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static void ShowExplosionAnimation()
    {
        string[] explosionFrames = {
            "      _.-^^---....,,--       \n" +
            "  _--                  --_  \n" +
            " <                        >)\n" +
            " |                         | \n" +
            "  \\._                   _./  \n" +
            "     ```--. . , ; .--'''       \n",

            "      _.-^^---....,,--       \n" +
            "  _--                  --_  \n" +
            " <                        >)\n" +
            " |                         | \n" +
            "  \\._  💥🔥🔥💥  _./  \n" +
            "     ```--. . , ; .--'''       \n",

            "      _.-^^---....,,--       \n" +
            "  _--                  --_  \n" +
            " <                        >)\n" +
            " |    💥💥💥💥💥    | \n" +
            "  \\._  🔥🔥🔥🔥🔥  _./  \n" +
            "     ```--. . , ; .--'''       \n"
        };

        foreach (var frame in explosionFrames)
        {
            Console.Clear();
            Console.WriteLine(frame);
            Thread.Sleep(500);
        }
    }

    static void ShowLossScreen()
    {
           string lossScreen = @"
     ____                         ___                 
  / ___| __ _ _ __ ___   ___   / _ \__   _____ _ __ 
 | |  _ / _` | '_ ` _ \ / _ \ | | | \ \ / / _ \ '__|
 | |_| | (_| | | | | | |  __/ | |_| |\ V /  __/ |   
  \____|\__,_|_| |_| |_|\___|  \___/  \_/ \___|_|   


     The tank has overrun your position.
             GAME OVER.
        ";
        Console.Clear();
        Console.WriteLine("GAME OVER. The tank has overrun your position.");
        Thread.Sleep(2000);
    }
}
