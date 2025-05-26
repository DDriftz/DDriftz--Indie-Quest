using System;
using System.Threading;

namespace TankBattleGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // Game introduction
            Console.WriteLine("Welcome to the Tank Battle Game!");
            Console.Write("Enter your name: ");
            string playerName = Console.ReadLine()!;

            Console.WriteLine($"\n{playerName}, you are in control of an artillery unit. Destroy the advancing tank!");

            // Choose difficulty level
            int maxAttempts = SelectDifficulty();

            // Proceed to the tank battle
            Console.WriteLine("\nPress any key to proceed to the battlefield...");
            Console.ReadKey();
            Console.Clear();

            StartTankBattle(playerName, maxAttempts);
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

        static void StartTankBattle(string playerName, int maxAttempts)
        {
            Random random = new Random();
            int tankDistance = random.Next(40, 71); // Random distance between 40 and 70
            int remainingShells = maxAttempts;
            bool tankDestroyed = false; // Track whether the tank has been destroyed

            Console.WriteLine($"You have {maxAttempts} shells to hit the tank. Good luck!\n");

            // Game loop
            while (remainingShells > 0 && tankDistance > 0)
            {
                // Draw the battlefield
                DrawBattlefield(tankDistance);

                // Ask for firing distance
                Console.Write($"{playerName}, enter the distance to fire at (0-80): ");
                int fireDistance;
                bool isValidInput = int.TryParse(Console.ReadLine(), out fireDistance);

                if (!isValidInput || fireDistance < 0 || fireDistance > 80)
                {
                    Console.WriteLine("Invalid input. Please enter a number between 0 and 80.");
                    continue;
                }

                // Draw explosion and check hit
                Console.Clear();
                DrawBattlefield(tankDistance);
                DrawExplosion(fireDistance);

                if (fireDistance == tankDistance)
                {
                    Console.WriteLine("🎯 DIRECT HIT! The tank has been destroyed! You win! 🎉");
                    tankDestroyed = true; // Set tankDestroyed to true
                    ShowExplosionAnimation();
                    break;
                }
                else if (fireDistance < tankDistance)
                {
                    Console.WriteLine("Your shot was too short!");
                    #if WINDOWS
                    Console.Beep(300, 200);
                    #endif
                }
                else
                {
                    Console.WriteLine("Your shot was too long!");
                    #if WINDOWS
                    Console.Beep(200, 200);
                    #endif
                }

                // Reduce remaining shells
                remainingShells--;
                Console.WriteLine($"You have {remainingShells} shells remaining.\n");

                // Move the tank closer
                if (remainingShells > 0)
                {
                    int tankMovement = random.Next(1, 16); // Move tank by 1-15 units
                    tankDistance -= tankMovement;
                    if (tankDistance < 0) tankDistance = 0;
                    Console.WriteLine($"The tank moved closer by {tankMovement} units.\n");
                }

                // Wait for player to press Enter
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                Console.Clear();
            }

            // Game over conditions
            if (!tankDestroyed) // Check if the tank was not destroyed
            {
                if (tankDistance <= 0)
                {
                    Console.WriteLine("💥 The tank has reached your position! GAME OVER.");
                }
                else if (remainingShells == 0)
                {
                    Console.WriteLine("💥 Out of shells! The tank has reached your position. GAME OVER.");
                }
                ShowLossScreen();
            }

            Console.WriteLine("Thanks for playing!");
        }

        static void DrawBattlefield(int tankDistance)
        {
            Console.WriteLine(new string('_', 80)); // Draw ground
            Console.Write("/"); // Draw artillery
            Console.Write(new string(' ', tankDistance)); // Space between artillery and tank
            Console.Write("T"); // Draw tank
            Console.WriteLine(new string('_', 80 - tankDistance - 2)); // Draw remaining ground
        }

        static void DrawExplosion(int fireDistance)
        {
            Console.Write(new string(' ', fireDistance)); // Space before explosion
            Console.WriteLine("*"); // Draw explosion
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
            Console.WriteLine(lossScreen);
            Thread.Sleep(2000);
        }
    }
}