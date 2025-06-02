﻿using System;
using System.IO;

class Program
{
    static int width;
    static int height;
    static char[,]? map;
    static int playerX;
    static int playerY;
    static string? levelName;

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        LoadLevel("level1.txt");
        DrawMap();
    }

    static void LoadLevel(string path)
    {
        var lines = File.ReadAllLines(path);
        levelName = lines[0];
        string[] dims = lines[1].Split(' ');
        width = int.Parse(dims[0]);
        height = int.Parse(dims[1]);

        map = new char[width, height];

        for (int y = 0; y < height; y++)
        {
            string line = lines[y + 2];
            for (int x = 0; x < width; x++)
            {
                char c = line[x];
                map[x, y] = c;

                if (c == 'S')
                {
                    playerX = x;
                    playerY = y;
                    map[x, y] = ' '; // clear out the start tile
                }
            }
        }

        // Optional: Randomly add ♠ trees in first three rows
        var random = new Random();
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x, y] == ' ' && random.NextDouble() < 0.05)
                {
                    map[x, y] = '♠';
                }
            }
        }
    }

    static void DrawMap()
    {
        Console.Clear();
        Console.WriteLine(levelName + "\n");

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == playerX && y == playerY)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write('☺');
                }
                else if (map != null)
                {
                    switch (map[x, y])
                    {
                        case '#':
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write('#');
                            break;
                        case 'M':
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write('M');
                            break;
                        case '♠':
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write('♠');
                            break;
                        default:
                            Console.ResetColor();
                            Console.Write(map[x, y]);
                            break;
                    }
                }
            }
            Console.WriteLine();
        }

        Console.ResetColor();
    }
}
