﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DiceSimulator
{
    class Program
    {
        static Random random = new Random();
        private static readonly char[] DiceSplitChars = { '+', '-' };
        
        // Dictionary mapping dice types to their ASCII art files
        // Placeholder 'X' is expected in the art files.
        private static readonly Dictionary<int, (string path, ConsoleColor color)> DiceArtFiles = new()
        {
            {4, ("D4.txt", ConsoleColor.Red)},
            {6, ("D6.txt", ConsoleColor.Green)},
            {8, ("D8.txt", ConsoleColor.Blue)},
            {10, ("D10.txt", ConsoleColor.Yellow)},
            {12, ("D12.txt", ConsoleColor.Cyan)},
            {20, ("D20.txt", ConsoleColor.Magenta)}
        };

        static void Main()
        {
            Console.Clear();
            // Note: DiceLogo.txt was not provided, so this might show a "file missing" message.
            DrawArt("DiceLogo.txt", ConsoleColor.DarkCyan); 
            Simulate();
        }

        static void Simulate()
        {
            string? currentNotationInput = null; // Store the last valid notation

            while (true)
            {
                if (currentNotationInput == null) // Only ask for new input if we don't have one
                {
                    currentNotationInput = GetValidDiceNotation();
                }
                
                int total = DiceRoll(currentNotationInput);
                
                Console.WriteLine($"\nYou rolled {total} (including bonuses).\n");
                
                ConsoleKey action = PromptForNextAction();

                if (action == ConsoleKey.Q) break; // Quit
                if (action == ConsoleKey.N) // New roll
                {
                    currentNotationInput = null; // Clear current notation to ask for new
                    Console.Clear();
                    DrawArt("DiceLogo.txt", ConsoleColor.DarkCyan); // Redraw logo if needed
                }
                // If action is R (Repeat), loop continues with currentNotationInput
            }
            Console.WriteLine("Exiting Dice Simulator. Goodbye!");
        }

        static string GetValidDiceNotation()
        {
            while (true)
            {
                Console.WriteLine("Enter desired roll in standard dice notation (e.g. 2d6+1):");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) // Check for null or whitespace
                {
                    Console.WriteLine("\nInput cannot be empty. Please try again.\n");
                    continue;
                }
                try
                {
                    IsStandardDiceNotation(input); // This method throws if invalid
                    return input;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"\n{ex.Message} Please try again.\n");
                }
            }
        }

        // Updated PromptForNextAction to align with Word doc (Part 2)
        static ConsoleKey PromptForNextAction()
        {
            Console.WriteLine("Do you want to (r)epeat, enter a (n)ew roll, or (q)uit?");
            while (true)
            {
                var keyInfo = Console.ReadKey(true); // Read key without displaying it
                
                switch (keyInfo.Key)
                {
                    case ConsoleKey.R:
                        Console.WriteLine("Repeating roll...");
                        return ConsoleKey.R;
                    case ConsoleKey.N:
                        Console.WriteLine("Entering new roll...");
                        return ConsoleKey.N;
                    case ConsoleKey.Q:
                        return ConsoleKey.Q;
                    default:
                        Console.WriteLine("Invalid option. Please press R, N, or Q.");
                        break;
                }
            }
        }
        
        // Modified DiceRoll to align with Part 2 (repeat uses same notation)
        static int DiceRoll(string diceNotation)
        {
            Console.WriteLine("\nSimulating...\n");
            Thread.Sleep(100); // Reduced sleep time slightly

            ParseDiceNotation(diceNotation, out int rolls, out int sides, out int bonus);
            int totalOfDice = 0; // Sum of dice rolls only

            for (int i = 0; i < rolls; i++)
            {
                int result = random.Next(1, sides + 1);
                totalOfDice += result;
                DisplayRollResult(i + 1, sides, result);
                Thread.Sleep(100); // Reduced sleep time slightly
            }
            
            // Output individual rolls sum before bonus, as per Word doc Part 1 example
            if (rolls > 0 && bonus != 0)
            {
                 Console.WriteLine($"\nYou rolled {totalOfDice} from the dice.");
            } else if (rolls > 0 && bonus == 0) {
                 // If no bonus, the totalOfDice is the final "You rolled X" message
                 // This is handled by the total in Simulate()
            }


            return totalOfDice + bonus; // Final score including bonus
        }


        static void ParseDiceNotation(string notation, out int rolls, out int sides, out int bonus)
        {
            notation = notation.ToLower(); // Standardize to lowercase for parsing 'd'
            int dIndex = notation.IndexOf('d');
            
            // Rolls: if 'd' is at the start (e.g., "d6"), rolls = 1. Otherwise, parse number before 'd'.
            rolls = dIndex > 0 ? int.Parse(notation[..dIndex]) : 1;

            string afterD = notation[(dIndex + 1)..];
            int opIndex = afterD.IndexOfAny(DiceSplitChars);

            if (opIndex != -1)
            {
                sides = int.Parse(afterD[..opIndex]);
                bonus = int.Parse(afterD[opIndex..]); // e.g., "+5" or "-2"
            }
            else
            {
                sides = int.Parse(afterD);
                bonus = 0;
            }
        }

        static void DisplayRollResult(int rollNumber, int sides, int result)
        {
            if (DiceArtFiles.TryGetValue(sides, out var artInfo))
            {
                Console.WriteLine($"{OrdinalNumber(rollNumber)} roll ({sides}-sided die shows {result}):");
                // The placeholder in DrawArt is "X" (uppercase) by default
                DrawArt(artInfo.path, artInfo.color, "X", result.ToString());
            }
            else
            {
                Console.WriteLine($"{OrdinalNumber(rollNumber)} roll is: {result}");
            }
        }

        static void DrawArt(string path, ConsoleColor color, string placeholder = "", string replacement = "")
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            
            try
            {
                if (File.Exists(path))
                {
                    string[] artLines = File.ReadAllLines(path);
                    foreach (string line in artLines)
                    {
                        if (!string.IsNullOrEmpty(placeholder) && !string.IsNullOrEmpty(replacement))
                        {
                            // Simplified replacement logic: PadLeft(2) ensures a 2-char width string (" 7" or "12")
                            // This replaces the single character placeholder (e.g., "X") with the 2-char number.
                            Console.WriteLine(line.Replace(placeholder, replacement.PadLeft(2)));
                        }
                        else
                        {
                            Console.WriteLine(line);
                        }
                    }
                }
                else
                {
                    if (path != "DiceLogo.txt") // Only show missing file for actual dice art
                       Console.WriteLine($"[Art file missing: {path}]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Art error reading {path}: {ex.Message}]");
            }
            
            Console.ForegroundColor = originalColor; // Reset to original color
            Console.WriteLine(); // Add a blank line for spacing after art
        }

        static void IsStandardDiceNotation(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Input cannot be empty.");

            string tempText = text.ToLower(); // Use lowercase for 'd' detection
            int dIndex = tempText.IndexOf('d');
            if (dIndex == -1)
                throw new ArgumentException("Missing 'd' in dice notation (e.g., 2d6, d20).");

            // Validate number of rolls (if specified)
            if (dIndex > 0) // If there's something before 'd'
            {
                if (!int.TryParse(tempText[..dIndex], out int rolls) || rolls <= 0)
                    throw new ArgumentException("Invalid number of rolls. Must be a positive number.");
            }

            string afterD = tempText[(dIndex + 1)..];
            if (string.IsNullOrWhiteSpace(afterD))
                throw new ArgumentException("Missing number of sides after 'd' (e.g., d6, d20).");

            int opIndex = afterD.IndexOfAny(DiceSplitChars); // Check for '+' or '-'
            string sidesStr = opIndex != -1 ? afterD[..opIndex] : afterD;

            if (!int.TryParse(sidesStr, out int sides) || sides <= 0)
                throw new ArgumentException("Invalid number of sides. Must be a positive number.");

            // Validate bonus (if specified)
            if (opIndex != -1)
            {
                if (opIndex == afterD.Length - 1) // Operator is last char e.g. "2d6+"
                    throw new ArgumentException("Missing bonus value after operator '+' or '-'.");
                if (!int.TryParse(afterD[(opIndex)..], out _)) // Check characters after operator
                    throw new ArgumentException("Invalid bonus value. Must be a number after '+' or '-'.");
            }
        }

        static string OrdinalNumber(int number)
        {
            if (number <= 0) return number.ToString(); // Handle non-positives if they ever occur
            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return $"{number}th";
            }
            switch (number % 10)
            {
                case 1: return $"{number}st";
                case 2: return $"{number}nd";
                case 3: return $"{number}rd";
                default: return $"{number}th";
            }
        }
    }
}
