﻿using System;
using System.Text.RegularExpressions; // Required for Regex

namespace DiceSimulator // Changed from Dice2 to keep original Canvas namespace, adjust if not desired
{
    internal partial class Program // Made partial for generated Regex methods
    {
        // Static Random instance for all dice rolls for better randomness
        static Random random = new Random();

        static void Main()
        {
            Console.WriteLine("--- Mission 1 & 2: Upgraded Dice Roller & Notation Check ---");
            // Test cases from "Dice simulator 2.docx" for Mission 1 & 2
            MakeDiceRoll("d6");
            MakeDiceRoll("2d4");
            MakeDiceRoll("d8+12");
            MakeDiceRoll("2d4-1"); // Example with negative bonus

            // Additional test cases including those from the uploaded Program.cs
            MakeDiceRoll("d8-12"); // Testing negative bonus further
            MakeDiceRoll("2d4+1");

            // Should not throw dice (invalid notations)
            MakeDiceRoll("34");
            MakeDiceRoll("ad");        // Changed from "ad6" to "ad" to match Program.cs test
            MakeDiceRoll("33d4*2");
            MakeDiceRoll("1d");        // Test invalid: 'd' at end
            MakeDiceRoll("d");         // Test invalid: only 'd'
            MakeDiceRoll("2d+3");      // Test invalid: no sides
            MakeDiceRoll("2d6++3");    // Test invalid: double plus
            MakeDiceRoll("2d6--3");    // Test invalid: double minus
            MakeDiceRoll("2d6+-3");    // Test invalid: mixed operators

            Console.WriteLine("\nPress Enter to continue to Dice Notation Counter...");
            Console.ReadLine();
            Console.Clear();

            Console.WriteLine("--- Mission 3: Standard Dice Notation Counter ---");
            string textToAnalyze = "To use the magic potion of Dragon Breath, first roll d8. If you roll 2 or higher, you manage to open the potion. Now roll 5d4+5 to see how many seconds the spell will last. Finally, the damage of the flames will be 2d6 per second.";
            Console.WriteLine($"Analyzing text: \"{textToAnalyze}\"\n");
            DiceNotationsAndRollsInText(textToAnalyze);

            // to keep console open
            Console.WriteLine("\nPress Enter to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Simulates rolling a set of dice and adds a fixed bonus.
        /// </summary>
        /// <param name="numberOfRolls">The number of dice to roll.</param>
        /// <param name="diceSides">The number of sides on each die.</param>
        /// <param name="fixedBonus">A fixed amount to add to the total roll. Defaults to 0.</param>
        /// <returns>The total result of the dice rolls plus the bonus.</returns>
        static int DiceRoll(int numberOfRolls, int diceSides, int fixedBonus = 0)
        {
            // set total to fixedBonus immediately, because it will always be added
            int total = fixedBonus;
            // Basic validation
            if (numberOfRolls <= 0 || diceSides <= 0)
            {
                // If invalid number of rolls or sides, return just the bonus (or 0 if no bonus)
                // This handles cases where parsed values are invalid before reaching here.
                return fixedBonus;
            }

            // for every roll
            for (int i = 0; i < numberOfRolls; i++)
            {
                // add a number from 1-diceSides to the total
                total += random.Next(1, diceSides + 1); // random.Next max value is exclusive
            }
            // return the result of the roll
            return total;
        }

        /// <summary>
        /// Parses a dice notation string (e.g., "2d6+3", "d10-2") and calculates the roll.
        /// Returns null if the notation is invalid or parsing fails.
        /// </summary>
        /// <param name="diceNotation">The string representing the dice roll.</param>
        /// <returns>The result of the dice roll, or null if parsing fails.</returns>
        static int? DiceRoll(string diceNotation)
        {
            if (string.IsNullOrWhiteSpace(diceNotation) || !IsStandardDiceNotation(diceNotation))
            {
                return null; // Return null for invalid notation
            }

            // Use the Regex to capture parts, IsStandardDiceNotation already validated the format
            Match match = StandardDiceNotationRegex().Match(diceNotation);

            // Extract number of rolls. If group 1 is empty (e.g., "d6"), default to 1.
            int numberOfRolls = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);

            // Extract dice sides from group 2.
            int diceSides = int.Parse(match.Groups[2].Value);

            int fixedBonus = 0;
            // Check if bonus group (group 3, which includes operator and number) exists.
            if (match.Groups[3].Success && !string.IsNullOrEmpty(match.Groups[3].Value))
            {
                string bonusPart = match.Groups[3].Value; // e.g., "+12" or "-1"
                // The operator is part of group 3. Group 4 is just the number part of bonus.
                // Group 3 ensures an operator is present.
                fixedBonus = int.Parse(bonusPart); // int.Parse handles leading '+' or '-'
            }

            return DiceRoll(numberOfRolls, diceSides, fixedBonus);
        }


        /// <summary>
        /// Helper function to make 10 dice rolls for a given notation and display results.
        /// It uses IsStandardDiceNotation to validate the input.
        /// </summary>
        /// <param name="diceNotation">The dice notation string.</param>
        static void MakeDiceRoll(string diceNotation)
        {
            // if not standard dice notation
            if (!IsStandardDiceNotation(diceNotation))
            {
                // write error message
                Console.WriteLine($"Can't throw \"{diceNotation}\", it's not in standard dice notation.");
                // early return to stop dice from being thrown
                return;
            }
            // prints what diceNotation we're rolling
            Console.Write($"Throwing {diceNotation} ... ");
            for (int i = 0; i < 10; i++)
            {
                //simulating 10 rolls and prints the results
                int? result = DiceRoll(diceNotation); // DiceRoll(string) now returns int?
                if (result.HasValue)
                {
                    Console.Write($" {result.Value}");
                }
                else
                {
                    // This case should ideally be caught by IsStandardDiceNotation above,
                    // but as a fallback:
                    Console.Write(" [Error]");
                }
            }
            // prints new line for separation
            Console.WriteLine();
        }

        /// <summary>
        /// Checks if a string is in standard dice notation.
        /// Examples: "d6", "2d4", "10d20", "d8+12", "2d4-1", "12d100+50".
        /// Number of rolls can be 1 or 2 digits (or omitted, defaulting to 1).
        /// Dice sides can be 1 or 2 digits (or more, regex supports \d+).
        /// Bonus can be 1 or 2 digits (or more, regex supports \d+), preceded by + or -.
        /// </summary>
        /// <param name="text">The string to check.</param>
        /// <returns>True if in standard dice notation, false otherwise.</returns>
        static bool IsStandardDiceNotation(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            // regex match with standard dice notation
            // ^ : start of string
            // (\d{1,2})? : optional one or two digits for number of rolls (Group 1)
            // d : literal 'd'
            // (\d+) : one or more digits for dice sides (Group 2) - allows for d10, d20, d100 etc.
            // ([+-]\d+)? : optional bonus part (Group 3)
            //   [+-] : a plus or a minus sign
            //   \d+ : one or more digits for the bonus amount
            // $ : end of string
            Match match = StandardDiceNotationRegex().Match(text);
            return match.Success;
        }

        /// <summary>
        /// Extracts the number of rolls from a valid dice notation string.
        /// Assumes diceNotation is already validated by IsStandardDiceNotation.
        /// </summary>
        /// <param name="diceNotation">A valid dice notation string.</param>
        /// <returns>The number of rolls.</returns>
        static int RollsInDiceNotation(string diceNotation)
        {
            // Regex to find the number of rolls part.
            // (\d{1,2})? : Optional one or two digits at the beginning, before 'd'. (Group 1)
            // d : Literal 'd'.
            // \d+ : Dice sides.
            // Optional bonus part is not needed for just counting rolls.
            Match match = RollsInDiceNotationRegex().Match(diceNotation);

            if (match.Success)
            {
                // if Group 1 (number of rolls) is empty or whitespace, default to 1 roll (e.g., "d6")
                return string.IsNullOrWhiteSpace(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
            }
            // This should not be reached if IsStandardDiceNotation was called first.
            return 0; // Should ideally throw an error if notation is invalid.
        }

        // GeneratedRegex for IsStandardDiceNotation
        // Allows 1 or 2 digits for rolls (optional), 1 or more for sides, 1 or more for bonus.
        [GeneratedRegex(@"^(\d{1,2})?d(\d+)([+-]\d+)?$", RegexOptions.IgnoreCase)]
        private static partial Regex StandardDiceNotationRegex();

        // GeneratedRegex for RollsInDiceNotation
        // Specifically targets the number of rolls part.
        [GeneratedRegex(@"^(\d{1,2})?d\d+([+-]\d+)?$", RegexOptions.IgnoreCase)]
        private static partial Regex RollsInDiceNotationRegex();


        /// <summary>
        /// Counts and prints the amount of dice notations and total rolls in a given text.
        /// </summary>
        /// <param name="text">The text to analyze.</param>
        static void DiceNotationsAndRollsInText(string text)
        {
            // Regex pattern to find dice notations within a larger text.
            // This pattern is less strict with ^$ anchors because it's looking for substrings.
            // (\b(\d{1,2})?d\d+([+-]\d+)?\b) - \b ensures word boundaries
            string regexPattern = @"\b((\d{1,2})?d\d+([+-]\d+)?)\b";
            MatchCollection matches = Regex.Matches(text, regexPattern, RegexOptions.IgnoreCase);

            int amountOfNotations = matches.Count;
            int totalRolls = 0;

            Console.WriteLine("Found Dice Notations:");
            foreach (Match match in matches)
            {
                string notation = match.Groups[1].Value; // Group 1 is the full dice notation found
                Console.WriteLine($"- \"{notation}\"");
                if (IsStandardDiceNotation(notation)) // Double check it (Regex.Matches can be broad)
                {
                    totalRolls += RollsInDiceNotation(notation);
                }
            }

            Console.WriteLine($"\n{amountOfNotations} standard dice notations present.");
            Console.WriteLine($"The player will have to perform {totalRolls} rolls.");
        }
    }
}
