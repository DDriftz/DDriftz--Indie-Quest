﻿using System;
using System.IO;

namespace MinotaursLair
{
    public class Program
    {
        static int width;
        static int height;
        static char[,]? map;
        static int playerX;
        static int playerY;
        static bool treesGenerated = false; // Add a flag to track tree generation

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Read level data from a file (replace "level.txt" with your actual file path)
            string[] levelData = File.ReadAllLines("MazeLevel.txt");

            // Extract the level name from the first row
            string levelName = levelData[0];

            // Output the title screen
            Console.WriteLine($"Mission: {levelName}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true); // Wait for user input

            // Extract the level dimensions from the second row
            string[] dimensions = levelData[1].Split('x'); // Split by 'x'
            width = int.Parse(dimensions[0]);
            height = int.Parse(dimensions[1]);

            // Initialize the map and player coordinates
            map = new char[width, height];

            // Find the player's starting position (S) in the map
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y] = levelData[y + 2][x];
                    if (map[x, y] == 'S')
                    {
                        playerX = x;
                        playerY = y;
                        // Replace the player's starting position with a space
                        map[x, y] = ' ';
                    }
                }
            }

            // Generate trees only once at the beginning
            GenerateTrees();

            // Create an infinite game loop
            while (true)
            {
                DrawMap();

                // Wait for a key press
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // Handle player movement based on arrow keys
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        MovePlayer(0, -1);
                        break;
                    case ConsoleKey.DownArrow:
                        MovePlayer(0, 1);
                        break;
                    case ConsoleKey.LeftArrow:
                        MovePlayer(-1, 0);
                        break;
                    case ConsoleKey.RightArrow:
                        MovePlayer(1, 0);
                        break;
                    case ConsoleKey.Escape:
                        return; // Exit the game if the user presses the Escape key
                }
                // Check if the player has reached the Minotaur
                if (map[playerX, playerY] == 'M')
                {
                    Console.Clear();
                    Console.WriteLine("Congratulations! You've reached the Minotaur. You win!");
                    return; // Exit the game
                }
            }
        }
        static void DrawMap()
        {
            Console.Clear();

            // Set the console cursor to position (0, 0)
            Console.SetCursorPosition(0, 0);

            // Draw the map and player
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == playerX && y == playerY)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow; // Player color
                        Console.Write("☺");
                    }
                    else
                    {
                        char mapChar = map != null ? map[x, y] : ' ';
                        Console.ForegroundColor = GetColorForMapChar(mapChar);
                        Console.Write(mapChar);
                    }
                }
                Console.WriteLine();
            }
        }
        static void GenerateTrees()
        {
            if (!treesGenerated)
            {
                // Optional: Randomly generate trees (♠) in the first three rows
                Random random = new Random();
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (random.Next(5) == 0)
                        {
                            if (map != null)
                            {
                                map[x, y] = '♠';
                            }
                        }
                    }
                }
                treesGenerated = true;
            }
        }
        static ConsoleColor GetColorForMapChar(char mapChar)
        {
            // Optional: Define colors based on map characters (customize as needed)
            switch (mapChar)
            {
                case '█':
                    return ConsoleColor.Gray; // Wall color
                case 'M':
                    return ConsoleColor.Red; // Minotaur color
                case '♠':
                    return ConsoleColor.Green; // Tree color
                default:
                    return ConsoleColor.White; // Default color (empty space)
            }
        }
        static void MovePlayer(int deltaX, int deltaY)
        {
            int newPlayerX = playerX + deltaX;
            int newPlayerY = playerY + deltaY;

            // Check if the new position is within bounds and is an empty space or a Minotaur
            if (map != null &&
                newPlayerX >= 0 && newPlayerX < width && newPlayerY >= 0 && newPlayerY < height &&
                (map[newPlayerX, newPlayerY] == ' ' || map[newPlayerX, newPlayerY] == 'M'))
            {
                // Update the player's position
                playerX = newPlayerX;
                playerY = newPlayerY;
            }
        }
    }
}