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
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            // Read level data from a file (replace "level.txt" with your actual file path)
            string[] levelData = File.ReadAllLines("MazeLevel.txt");
            // Extract the level name from the first row
            string levelName = levelData[0];
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
            DrawMap();
        }
        static void DrawMap()
        {
            Console.Clear();
            // Optional: Randomly generate trees (♠) in the first three rows
            Random random = new Random();
            if (map != null)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (random.Next(5) == 0)
                        {
                            map[x, y] = '♠';
                        }
                    }
                }
            }
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
    }
}