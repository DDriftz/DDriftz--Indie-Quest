﻿using System;
using System.IO;
using System.Threading;

namespace Dice4
{
    internal class Program
    {
        private static readonly char[] DiceSplitChars = { '+', '-' };

        static void Main()
        {
            Simulate();
        }

        static void DrawArt(string path, ConsoleColor foregroundColor, string toBeReplaced = "", string replaceWith = "")
        {
            Console.ForegroundColor = foregroundColor;
            string[] logoLines = File.ReadAllLines(path);
            foreach (var line in logoLines)
            {
                if (!string.IsNullOrWhiteSpace(toBeReplaced) && !string.IsNullOrWhiteSpace(replaceWith))
                {
                    Console.WriteLine(line.Replace(replaceWith.Length == 2 ? $"{toBeReplaced} " : toBeReplaced, replaceWith.ToString()));
                }
                else
                {
                    Console.WriteLine(line);
                }
            }
            Thread.Sleep(500);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void Simulate()
        {
            DrawArt("DiceLogo.txt", ConsoleColor.Magenta);
            bool isStandardNotation = false;
            string? notationInput = string.Empty;
            Console.WriteLine("Enter desired roll in standard dice notation:");
            while (!isStandardNotation)
            {
                notationInput = Console.ReadLine();
                Console.WriteLine();
                if (string.IsNullOrWhiteSpace(notationInput))
                {
                    Console.WriteLine("Input cannot be empty. Try again:");
                    continue;
                }
                try
                {
                    IsStandardDiceNotation(notationInput);
                    isStandardNotation = true;
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine($"{ae.Message} Try again:");
                }
            }

            if (!string.IsNullOrWhiteSpace(notationInput))
                Console.WriteLine($"You rolled {DiceRoll(notationInput)}.\n");

            while (true)
            {
                Console.WriteLine("Press 'R' to roll again, 'N' for new dice notation, or any other key to exit.");
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.R)
                {
                    if (!string.IsNullOrWhiteSpace(notationInput))
                        Console.WriteLine($"You rolled {DiceRoll(notationInput)}.\n");
                }
                else if (keyInfo.Key == ConsoleKey.N)
                {
                    isStandardNotation = false;
                    notationInput = string.Empty;
                    Console.WriteLine("Enter desired roll in standard dice notation:");
                    while (!isStandardNotation)
                    {
                        notationInput = Console.ReadLine();
                        Console.WriteLine();
                        if (string.IsNullOrWhiteSpace(notationInput))
                        {
                            Console.WriteLine("Input cannot be empty. Try again:");
                            continue;
                        }
                        try
                        {
                            IsStandardDiceNotation(notationInput);
                            isStandardNotation = true;
                        }
                        catch (ArgumentException ae)
                        {
                            Console.WriteLine($"{ae.Message} Try again:");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(notationInput))
                        Console.WriteLine($"You rolled {DiceRoll(notationInput)}.\n");
                }
                else
                {
                    return;
                }
            }
        }

        static int DiceRoll(int numberOfRolls, int diceSides, int fixedBonus = 0)
        {
            Random random = new();
            int total = fixedBonus;
            for (int i = 0; i < numberOfRolls; i++)
            {
                int randomNumber = random.Next(1, diceSides + 1);
                total += randomNumber;

                if (diceSides == 4)
                {
                    DrawArt("D4.txt", ConsoleColor.Red, "X", randomNumber.ToString());
                }
                else if (diceSides == 6)
                {
                    DrawArt("D6.txt", ConsoleColor.Green, "X", randomNumber.ToString());
                }
                else
                {
                    Console.WriteLine($"{OrdinalNumber(i + 1)} roll is: {randomNumber}");
                }
            }
            Console.WriteLine();
            return total;
        }
                static int DiceRoll(string diceNotation)
                {
                    // Example: 2d6+3 or d20 or 4d8-2
                    Console.WriteLine("Simulating ...\n");
                    int dIndex = diceNotation.IndexOf('d');
                    int numberOfRolls;
                    int diceSides;
                    int fixedBonus;
                    int sign;
        
                    if (dIndex > 0)
                        numberOfRolls = int.Parse(diceNotation[..dIndex]);
                    else
                        numberOfRolls = 1;
        
                    int plusIndex = diceNotation.IndexOf('+', dIndex);
                    int minusIndex = diceNotation.IndexOf('-', dIndex);
        
                    if (plusIndex > 0)
                    {
                        diceSides = int.Parse(diceNotation[(dIndex + 1)..plusIndex]);
                        fixedBonus = int.Parse(diceNotation[(plusIndex + 1)..]);
                        sign = 1;
                    }
                    else if (minusIndex > 0)
                    {
                        diceSides = int.Parse(diceNotation[(dIndex + 1)..minusIndex]);
                        fixedBonus = int.Parse(diceNotation[(minusIndex + 1)..]);
                        sign = -1;
                    }
                    else
                    {
                        diceSides = int.Parse(diceNotation[(dIndex + 1)..]);
                        fixedBonus = 0;
                        sign = 1;
                    }
                    return DiceRoll(numberOfRolls, diceSides, fixedBonus * sign);
                }
        
                static void IsStandardDiceNotation(string text)
                {
                    string[] textParts = text.Split('d');
                    if (textParts.Length <= 1)
                    {
                        throw new ArgumentException($"Roll description is not in standard dice notation.");
                    }
        
                    int numberOfRolls;
                    try
                    {
                        numberOfRolls = textParts[0].Length > 0 ? int.Parse(textParts[0]) : 1;
                    }
                    catch
                    {
                        throw new ArgumentException($"Number of rolls ({textParts[0]}) is not an integer.");
                    }
        
                if (numberOfRolls <= 0)
                {
                    throw new ArgumentException($"Number of rolls ({textParts[0]}) has to be positive.");
                }

                string[] diceParts = textParts[1].Split(DiceSplitChars, StringSplitOptions.RemoveEmptyEntries);
                int diceSides;
                try
                {
                    diceSides = int.Parse(diceParts[0]);
                }
                catch
                {
                    throw new ArgumentException($"Number of dice sides ({diceParts[0]}) is not an integer.");
                }

                if (diceSides <= 0)
                {
                    throw new ArgumentException($"Number of sides ({diceParts[0]}) has to be positive.");
                }

                if (diceParts.Length > 1)
                {
                    try
                    {
                        _ = int.Parse(diceParts[1]);
                    }
                    catch
                    {
                        throw new ArgumentException($"Fixed bonus ({diceParts[1]}) is not an integer.");
                    }
                }
            }

            static string OrdinalNumber(int number)
            {
                if (number > 10)
                {
                    int secondToLastDigit = number / 10 % 10;
                    if (secondToLastDigit == 1)
                    {
                        return $"{number}th";
                    }
                }
                int lastDigit = number % 10;
                if (lastDigit == 1)
                {
                    return $"{number}st";
                }
                if (lastDigit == 2)
                {
                    return $"{number}nd";
                }
                if (lastDigit == 3)
                {
                    return $"{number}rd";
                }
                return $"{number}th";
            }
        }
    }
