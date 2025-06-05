﻿using System;
using System.Text; // Required for StringBuilder in Mission 1 (alternative)

namespace DiceSimulator
{
    internal class Program
    {
        static Random random = new Random(); // Static Random instance for dice rolls

        static void Main()
        {
            Console.WriteLine("--- Mission 1: Hidden Message ---");
            // initiate an array of ints with the numbers on the small note
            int[] smallNote = {
                                87,  97, 110, 116,  32, 116, 111,  32, 109, 101,
                                101, 116,  32, 102, 111, 114,  32, 108, 117, 110,
                                 99, 104,  63,  32,  73,  39, 108, 108,  32, 108,
                                101,  97, 118, 101,  32, 116, 104, 101,  32, 114,
                                101, 115, 116,  97, 117, 114,  97, 110, 116,  32,
                                 97, 100, 100, 114, 101, 115, 115,  32, 105, 110,
                                 32, 116, 104, 101,  32, 115, 111, 117, 116, 104,
                                 32, 109,  97, 105, 110, 116, 101, 110,  97, 110,
                                 99, 101,  32,  99, 108, 111, 115, 101, 116,  46,
                                 32,  66, 114, 105, 110, 103,  32,  97, 110,  32,
                                 65,  83,  67,  73,  73,  32,  99, 104,  97, 114,
                                116,  44,  32, 116, 104, 101,  32, 109, 101, 115,
                                115,  97, 103, 101,  32, 119, 105, 108, 108,  32,
                                 98, 101,  32,  99, 111, 100, 101, 100,  46
                            };

            // initiate an empty string to store the converted values
            // Using StringBuilder for better performance with string concatenation in a loop
            StringBuilder decipheredMessageBuilder = new StringBuilder();

            /* going through every value in the smallNote array,
             * we add the converted value to the decipheredNote string */
            foreach (int value in smallNote)
            {
                decipheredMessageBuilder.Append(Convert.ToChar(value));
            }
            string decipheredNote = decipheredMessageBuilder.ToString();

            // write the decipheredNote to the console
            Console.WriteLine("Deciphered Message:");
            Console.WriteLine(decipheredNote);
            Console.WriteLine("\nPress Enter to continue to ASCII Chart...");
            Console.ReadLine();

            Console.Clear(); // Clear console for next mission
            Console.WriteLine("--- Mission 2: ASCII Chart ---");
            Console.WriteLine("ASCII Chart (Codes 0-255):");
            // print every number and corresponding conversion (even the messed up ones)
            for (int i = 0; i < 256; i++)
            {
                char c = Convert.ToChar(i);
                string displayChar;
                // Handle non-printable characters for cleaner output
                if (char.IsControl(c) || char.IsWhiteSpace(c) && i != 32) // Keep space ' ' visible
                {
                    switch (i) { // Common control characters with names
                        case 0: displayChar = "[NUL]"; break;
                        case 7: displayChar = "[BEL]"; break;
                        case 8: displayChar = "[BS]";  break;
                        case 9: displayChar = "[HT]";  break; // Horizontal Tab
                        case 10: displayChar = "[LF]"; break; // Line Feed
                        case 13: displayChar = "[CR]"; break; // Carriage Return
                        default: displayChar = $"[CTRL_{i}]"; break; // Generic control
                    }
                } else {
                    displayChar = c.ToString();
                }
                Console.WriteLine($"{i} = {displayChar}");
                // Pause briefly every 32 characters to make it readable
                if ((i + 1) % 32 == 0 && i < 255) {
                    Console.WriteLine("Press Enter to see more...");
                    Console.ReadLine();
                }
            }
            Console.WriteLine("\nPress Enter to continue to Dice Roller...");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine("--- Mission 3: Standard Dice Notation ---");
            string[] testNotations = { "1d6", "2d8", "3d6+8", "1d4+4" };

            foreach (string notation in testNotations)
            {
                Console.Write($"Throwing {notation} ... ");
                for (int i = 0; i < 10; i++) // Display results of 10 calls
                {
                    Console.Write($"{DiceRoll(notation)} ");
                }
                Console.WriteLine();
            }

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
            int total = 0;
            if (numberOfRolls <= 0 || diceSides <= 0) { // Basic validation
                return fixedBonus; // Or throw an exception, or return error code
            }
            for (int i = 0; i < numberOfRolls; i++)
            {
                total += random.Next(1, diceSides + 1); // random.Next max is exclusive
            }
            return total + fixedBonus;
        }

        /// <summary>
        /// Simulates dice rolls based on standard dice notation string (e.g., "2d6+3").
        /// Supports single-digit numbers for rolls, sides, and bonus (1-9).
        /// </summary>
        /// <param name="diceNotation">The string representing the dice roll.</param>
        /// <returns>The result of the dice roll, or 0 if parsing fails.</returns>
        static int DiceRoll(string diceNotation)
        {
            if (string.IsNullOrWhiteSpace(diceNotation)) return 0;

            diceNotation = diceNotation.ToLower(); // Standardize to lowercase 'd'

            int numberOfRolls = 0;
            int diceSides = 0;
            int fixedBonus = 0;

            // Find 'd' separator
            int dIndex = diceNotation.IndexOf('d');
            if (dIndex == -1 || dIndex == 0) return 0; // 'd' not found or at the beginning

            // Extract numberOfRolls (before 'd')
            // Assuming single digit as per instructions: "numbers 1 to 9"
            if (!int.TryParse(diceNotation[0].ToString(), out numberOfRolls) || numberOfRolls < 1 || numberOfRolls > 9)
            {
                // If diceNotation starts with 'd' (e.g. "d6"), assume 1 die.
                if (dIndex == 0 && diceNotation.Length > 1 && char.IsDigit(diceNotation[1]))
                {
                    numberOfRolls = 1; // Implicit 1 die
                }
                else if (dIndex > 0 && int.TryParse(diceNotation.Substring(0, dIndex), out int parsedRolls))
                {
                     // Allow for multi-digit rolls if the problem implies it,
                     // but hint says "first, third, and fifth spot" suggesting single digits.
                     // For now, sticking to single digit per explicit hint.
                     if (parsedRolls >=1 && parsedRolls <=9) numberOfRolls = parsedRolls; else return 0;
                }
                 else
                {
                    return 0; // Invalid number of rolls
                }
            }


            // Find '+' separator for bonus (if any)
            int plusIndex = diceNotation.IndexOf('+');

            string sidesPart;
            if (plusIndex != -1) // Bonus exists
            {
                if (plusIndex <= dIndex + 1) return 0; // '+' is before or immediately after 'd'
                sidesPart = diceNotation.Substring(dIndex + 1, plusIndex - (dIndex + 1));
                
                // Extract fixedBonus (after '+') - assuming single digit
                string bonusPart = diceNotation.Substring(plusIndex + 1);
                 if (!int.TryParse(bonusPart, out fixedBonus) || fixedBonus < 0 || fixedBonus > 9) // Bonus can be 0
                {
                     // Allow for multi-digit bonus as well, but sticking to single for now.
                     if(int.TryParse(bonusPart, out int parsedBonus) && parsedBonus >=0) fixedBonus = parsedBonus; else return 0;
                }
            }
            else // No bonus
            {
                sidesPart = diceNotation.Substring(dIndex + 1);
            }

            // Extract diceSides - assuming single digit
            if (!int.TryParse(sidesPart, out diceSides) || diceSides < 1 || diceSides > 9)
            {
                 // Allow for multi-digit sides
                 if(int.TryParse(sidesPart, out int parsedSides) && parsedSides >=1 ) diceSides = parsedSides; else return 0;
            }
            
            // As per hint for "numbers 1 to 9":
            // "numbers will always appear in the first, third, and fifth spot"
            // This implies a strict format like "1d6" or "1d6+1"
            // Let's refine parsing if strictly adhering to the single-digit hint.

            // Re-parsing strictly based on the hint about fixed positions:
            // Format: XdY or XdY+Z where X, Y, Z are single digits 1-9.

            if (diceNotation.Length < 3 || diceNotation[1] != 'd') return 0; // Basic "XdY" check

            if (!char.IsDigit(diceNotation[0]) || !char.IsDigit(diceNotation[2])) return 0;
            
            numberOfRolls = diceNotation[0] - '0'; // Convert char to int
            diceSides = diceNotation[2] - '0';

            if (numberOfRolls < 1 || numberOfRolls > 9 || diceSides < 1 || diceSides > 9) return 0;

            fixedBonus = 0; // Reset bonus, parse if present
            if (diceNotation.Length == 5 && diceNotation[3] == '+')
            {
                if (!char.IsDigit(diceNotation[4])) return 0;
                fixedBonus = diceNotation[4] - '0';
                if (fixedBonus < 0 || fixedBonus > 9) return 0; // Bonus can be 0, but problem says 1-9 for numbers
            }
            else if (diceNotation.Length != 3) // Not XdY and not XdY+Z
            {
                // This handles cases like "1d6+ " or "1d" or "1d6+" if not caught above
                // The previous more flexible parsing is better if "1-9" isn't strict for ALL parts
                // For now, this strict parsing matches the hint more closely.
                // Let's revert to the slightly more flexible parsing above,
                // as "1d4+4" is an example (4 is a valid bonus).
                // The "1-9" might refer to the number of dice and sides primarily.
                // The hint about first/third/fifth spot is for the simplest case (all single digits).

                // Re-activating the more flexible parsing from before this block:
                // (The code above this `if (diceNotation.Length < 3 ...)` block is the flexible one)
                // To make the flexible parser work with the example "1d4+4", the digit constraint on bonus should allow 0-9.
                // And for "2d8", sides can be > 9 if not for the "1-9" constraint.
                // The problem states "numbers 1 to 9 for now". Let's assume this applies to numberOfRolls and diceSides only.
                // Bonus can be 0-9.
            }

            // Final check on parsed values based on "1-9 for now" instruction for rolls/sides
            if (numberOfRolls < 1 || numberOfRolls > 9 || diceSides < 1 || diceSides > 9) {
                // The example output has "2d8", where 8 is fine.
                // Let's assume the "1-9" means individual *digits* within the string,
                // not the value of the numbers themselves for sides/bonus.
                // The original parsing for numberOfRolls, diceSides, fixedBonus should be okay
                // if we only constrain the *input characters* as per the "1-9" hint for *parsing strategy*,
                // rather than the *magnitude* of diceSides or fixedBonus.

                // The most robust way to parse is splitting, but the hint suggests character access.
                // Re-evaluating based on hint "numbers will always appear in the first, third, and fifth spot"
                // This suggests the diceNotation is *always* XdY or XdY+Z with X,Y,Z being single digits.

                // Strict parsing to match the specific hint about character positions:
                numberOfRolls = 0; diceSides = 0; fixedBonus = 0; // Reset for strict parse

                if (diceNotation.Length >= 3 && diceNotation[1] == 'd' &&
                    char.IsDigit(diceNotation[0]) && char.IsDigit(diceNotation[2]))
                {
                    numberOfRolls = diceNotation[0] - '0';
                    diceSides = diceNotation[2] - '0';

                    if (numberOfRolls < 1 || numberOfRolls > 9 || diceSides < 1 || diceSides > 9) return 0; // Invalid single digit

                    if (diceNotation.Length == 3) // Format XdY
                    {
                        // Values already set
                    }
                    else if (diceNotation.Length == 5 && diceNotation[3] == '+' && char.IsDigit(diceNotation[4])) // Format XdY+Z
                    {
                        fixedBonus = diceNotation[4] - '0';
                        if (fixedBonus < 0 || fixedBonus > 9) return 0; // Bonus must be a single digit 0-9
                    }
                    else if (diceNotation.Length > 3) // Invalid format if longer than XdY but not XdY+Z
                    {
                        return 0; 
                    }
                }
                else
                {
                    return 0; // Does not match basic XdY structure
                }
            }


            return DiceRoll(numberOfRolls, diceSides, fixedBonus);
        }
    }
}
