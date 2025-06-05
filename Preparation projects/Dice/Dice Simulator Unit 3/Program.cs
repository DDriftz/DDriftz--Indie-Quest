﻿using System;
using System.Text.RegularExpressions; // Required for Regex

namespace DiceSimulator
{
    internal partial class Program // Made partial for generated Regex methods
    {
        // Static Random instance for all dice rolls for better randomness
        static Random random = new Random();

        static void Main()
        {
            Console.WriteLine("--- Dice Roller with Exception Handling ---");
            // Test cases from user's Program.cs for Dice Simulator 3
            // Should throw dice
            MakeDiceRoll("1d6+10");
            MakeDiceRoll("1d6-10");
            Console.WriteLine("NO BONUS");
            MakeDiceRoll("2d6");
            MakeDiceRoll("10d6");
            MakeDiceRoll("10d10");
            MakeDiceRoll("6d10");

            Console.WriteLine("POSITIVE BONUS");
            MakeDiceRoll("2d6+1");
            MakeDiceRoll("2d6+10");
            MakeDiceRoll("10d6+1");
            MakeDiceRoll("10d6+10");
            MakeDiceRoll("2d10+1");
            MakeDiceRoll("2d10+10");
            MakeDiceRoll("10d10+1");
            MakeDiceRoll("10d10+10");

            Console.WriteLine("NEGATIVE BONUS");
            MakeDiceRoll("2d6-1");
            MakeDiceRoll("2d6-10");
            MakeDiceRoll("10d6-1");
            MakeDiceRoll("10d6-10");
            MakeDiceRoll("2d10-1");
            MakeDiceRoll("2d10-10");
            MakeDiceRoll("10d10-1");
            MakeDiceRoll("10d10-10");

            Console.WriteLine("EXCEPTIONS (Expected error messages below):");
            // should throw special exceptions
            MakeDiceRoll("34");
            MakeDiceRoll("-12");
            MakeDiceRoll("ad6");
            MakeDiceRoll("-3d6");
            MakeDiceRoll("0d6");
            MakeDiceRoll("d+");    // Expect "Number of dice sides () is not an integer." or similar
            MakeDiceRoll("2d-4");  // Expect "Number of dice sides (-4) has to be positive."
            MakeDiceRoll("2d2.5");// Expect "Number of dice sides (2.5) is not an integer."
            MakeDiceRoll("2d$");   // Expect "Number of dice sides ($) is not an integer."
            // Additional tests from previous version
            MakeDiceRoll("1d");        // Test invalid: 'd' at end -> "Number of dice sides () is not an integer."
            MakeDiceRoll("d");         // Test invalid: only 'd' -> "Number of dice sides () is not an integer."
            MakeDiceRoll("2d+3");      // Test invalid: no sides after d -> "Number of dice sides () is not an integer."
            MakeDiceRoll("2d6++3");    // Expect "Fixed bonus (+3) is not an integer." (or "Roll description..." if '+' in sides part)
            MakeDiceRoll("2d6--3");    // Similar to above
            MakeDiceRoll("2d6+-3");    // Similar to above

            Console.WriteLine("\nPress Enter to continue to Dice Notation Counter...");
            Console.ReadLine();
            Console.Clear();

            Console.WriteLine("--- Dice Notation Counter ---");
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
            // Basic validation (already done by IsStandardDiceNotation, but good for direct calls)
            if (numberOfRolls <= 0 || diceSides <= 0)
            {
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
        /// Parses a validated dice notation string and calculates the roll.
        /// Assumes IsStandardDiceNotation has already validated the format and components.
        /// </summary>
        /// <param name="diceNotation">The string representing the dice roll.</param>
        /// <returns>The result of the dice roll.</returns>
        static int DiceRoll(string diceNotation)
        {
            // splits the diceNotation string based on 'd'
            string[] partsByD = diceNotation.Split('d');
            
            // Number of rolls: if part before 'd' is empty (e.g., "d6"), default to 1.
            int numberOfRolls = string.IsNullOrEmpty(partsByD[0]) ? 1 : int.Parse(partsByD[0]);

            // The part after 'd' contains sides and potentially bonus
            string sidesAndBonusPart = partsByD[1];
            string[] sidesAndBonusParts = sidesAndBonusPart.Split(new char[] { '+', '-' }, 2); // Split only on the first operator

            int diceSides = int.Parse(sidesAndBonusParts[0]);
            int fixedBonus = 0;

            if (sidesAndBonusParts.Length > 1) // Bonus part exists
            {
                // Determine if it was '+' or '-' by checking the character before the bonus number part
                // Find the index of the first '+' or '-' in the sidesAndBonusPart
                int operatorIndex = sidesAndBonusPart.IndexOfAny(new char[] { '+', '-' });
                if(operatorIndex != -1) // Should always be true if sidesAndBonusParts.Length > 1
                {
                    fixedBonus = int.Parse(sidesAndBonusParts[1]); // Parse the number part
                    if (sidesAndBonusPart[operatorIndex] == '-')
                    {
                        fixedBonus *= -1; // Make bonus negative if operator was '-'
                    }
                }
            }
            return DiceRoll(numberOfRolls, diceSides, fixedBonus);
        }

        /// <summary>
        /// Helper function to make 10 dice rolls for a given notation and display results.
        /// It uses IsStandardDiceNotation (via try-catch) to validate the input.
        /// </summary>
        /// <param name="diceNotation">The dice notation string.</param>
        static void MakeDiceRoll(string diceNotation)
        {
            try
            {
                // Validate the notation. This will throw ArgumentException if invalid.
                IsStandardDiceNotation(diceNotation);

                // If no exception, proceed to roll
                Console.Write($"Throwing {diceNotation} ... ");
                for (int i = 0; i < 10; i++)
                {
                    Console.Write($" {DiceRoll(diceNotation)}");
                }
                Console.WriteLine();
            }
            catch (ArgumentException ae)
            {
                // write error message if IsStandardDiceNotation throws an exception
                Console.WriteLine($"Can't throw \"{diceNotation}\" ... {ae.Message}");
            }
            catch (Exception ex) // Catch any other unexpected exceptions during rolling for robustness
            {
                Console.WriteLine($"An unexpected error occurred while processing \"{diceNotation}\": {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if a string is in standard dice notation by attempting to parse it.
        /// Throws ArgumentException with descriptive messages for invalid formats or values.
        /// Does not return a value; success is indicated by not throwing an exception.
        /// </summary>
        /// <param name="text">The string to check.</param>
        static void IsStandardDiceNotation(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Roll description cannot be empty or whitespace.");
            }

            string[] partsByD = text.Split('d');
            if (partsByD.Length != 2) // Must have exactly one 'd'
            {
                throw new ArgumentException($"Roll description ('{text}') is not in standard dice notation (missing or too many 'd' characters).");
            }

            string rollsPart = partsByD[0];
            string sidesAndBonusPart = partsByD[1];

            int numberOfRolls;
            if (string.IsNullOrEmpty(rollsPart))
            {
                numberOfRolls = 1; // Default to 1 roll if nothing before 'd'
            }
            else
            {
                try
                {
                    numberOfRolls = int.Parse(rollsPart);
                }
                catch (FormatException)
                {
                    throw new ArgumentException($"Number of rolls ('{rollsPart}') is not an integer.");
                }
                catch (OverflowException)
                {
                     throw new ArgumentException($"Number of rolls ('{rollsPart}') is too large or too small.");
                }
            }

            if (numberOfRolls <= 0)
            {
                throw new ArgumentException($"Number of rolls ('{rollsPart}') has to be positive.");
            }

            // Split the part after 'd' by the first '+' or '-'
            string[] sidesPartArray = sidesAndBonusPart.Split(new char[] { '+', '-' }, 2);
            string actualSidesStr = sidesPartArray[0];

            if (string.IsNullOrEmpty(actualSidesStr))
            {
                 throw new ArgumentException("Number of dice sides is missing.");
            }

            int diceSides;
            try
            {
                diceSides = int.Parse(actualSidesStr);
            }
            catch (FormatException)
            {
                throw new ArgumentException($"Number of dice sides ('{actualSidesStr}') is not an integer.");
            }
            catch (OverflowException)
            {
                 throw new ArgumentException($"Number of dice sides ('{actualSidesStr}') is too large or too small.");
            }


            if (diceSides <= 0)
            {
                throw new ArgumentException($"Number of dice sides ('{actualSidesStr}') has to be positive.");
            }

            if (sidesPartArray.Length > 1) // Bonus part exists
            {
                string bonusNumStr = sidesPartArray[1];
                 if (string.IsNullOrEmpty(bonusNumStr)) // e.g. "2d6+"
                {
                    throw new ArgumentException($"Fixed bonus value is missing after operator in '{text}'.");
                }
                try
                {
                    _ = int.Parse(bonusNumStr); // Just try to parse, value used in DiceRoll(string)
                }
                catch (FormatException)
                {
                    throw new ArgumentException($"Fixed bonus ('{bonusNumStr}') is not an integer.");
                }
                catch (OverflowException)
                {
                    throw new ArgumentException($"Fixed bonus ('{bonusNumStr}') is too large or too small.");
                }
            }
        }

        // Regex for RollsInDiceNotation as per user's Program.cs (Dice 3)
        [GeneratedRegex(@"(\d{0,2})?d\d")]
        private static partial Regex DiceRollRegexForCount(); // Renamed to avoid conflict if StandardDiceNotationRegex was kept

        /// <summary>
        /// Extracts the number of rolls from a dice notation string.
        /// Uses a Regex for simplicity in this specific extraction task.
        /// </summary>
        /// <param name="diceNotation">A dice notation string (ideally pre-validated).</param>
        /// <returns>The number of rolls.</returns>
        static int RollsInDiceNotation(string diceNotation)
        {
            // Regex match to check for dice rolls
            Match match = DiceRollRegexForCount().Match(diceNotation);

            // if where the dice roll should be (Group 1) is null or whitespace return 1 else return parsed dice rolls
            return string.IsNullOrWhiteSpace(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
        }

        /// <summary>
        /// Counts and prints the amount of dice notations and total rolls in a given text.
        /// Uses a Regex to find potential dice notations within text.
        /// </summary>
        /// <param name="text">The text to analyze.</param>
        static void DiceNotationsAndRollsInText(string text)
        {
            // Regex pattern from user's Program.cs (Dice 3) to find potential dice notations
            string regexPattern = @"\b((\d{1,2})?d\d+([+-]\d+)?)\b"; // Added \b for word boundaries
            MatchCollection matches = Regex.Matches(text, regexPattern, RegexOptions.IgnoreCase);

            int amountOfNotations = 0; // Correctly count validated notations
            int totalRolls = 0;

            Console.WriteLine("Found and validated Dice Notations:");
            foreach (Match match in matches)
            {
                string potentialNotation = match.Groups[1].Value; // Group 1 contains the full potential notation
                try
                {
                    IsStandardDiceNotation(potentialNotation); // Validate it
                    Console.WriteLine($"- \"{potentialNotation}\"");
                    amountOfNotations++;
                    totalRolls += RollsInDiceNotation(potentialNotation);
                }
                catch (ArgumentException)
                {
                    // This notation found by broad Regex was not valid, skip it.
                    // Optionally, log or indicate invalid substring: Console.WriteLine($" - Invalid substring found: \"{potentialNotation}\"");
                }
            }

            Console.WriteLine($"\n{amountOfNotations} standard dice notations present.");
            Console.WriteLine($"The player will have to perform {totalRolls} rolls.");
        }
    }
}
