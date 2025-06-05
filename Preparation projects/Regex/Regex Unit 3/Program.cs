﻿using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Regex_Unit_3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var manual = File.ReadAllLines("MonsterManual.txt");

            var monsterNames = new List<string>();
            int monsterNameLine = 0;
            string currentName = "";

            var namesByAlignment = new List<string>[3, 3];
            var namesOfUnaligned = new List<string>();
            var namesOfAnyAlignment = new List<string>();
            var namesOfSpecialCases = new List<string>();

            string[] typesOfAlignmentAxis1 = ["lawful", "chaotic", "neutral"];
            string[] typesOfAlignmentAxis2 = ["evil", "good", "neutral"];

            for (int axis1 = 0; axis1 < 3; axis1++)
            {
                for (int axis2 = 0; axis2 < 3; axis2++)
                {
                    namesByAlignment[axis1, axis2] = new List<string>();
                }
            } // creates 9 lists with different combinations of 3x3 alignment types

            foreach (string line in manual)
            {
                if (monsterNameLine == 0)
                {
                    monsterNames.Add(line);
                    currentName = line;
                }

                if (line.Length == 0)
                {
                    monsterNameLine = 0;
                }

                else
                {
                    monsterNameLine++;
                }

                if (monsterNameLine == 2 && Regex.IsMatch(line, @"(lawful|chaotic|neutral)\s+(evil|good|neutral)"))
                {
                    Match match = Regex.Match(line, @"(lawful|chaotic|neutral)$?\s+?(evil|good|neutral)?"); // matches regex with Monster Manual
                    string axis1Text = match.Groups[1].Value; // adds matched Manual value to text
                    string axis2Text = match.Groups[2].Value; // adds matched Manual value to text

                    int axis1 = Array.IndexOf(typesOfAlignmentAxis1, axis1Text); // creates index i value for names by alignment
                    int axis2 = Array.IndexOf(typesOfAlignmentAxis2, axis2Text); // creates index j value for names by alignment

                    namesByAlignment[axis1, axis2].Add(currentName);

                }

                if (monsterNameLine == 2 && Regex.IsMatch(line, @"neutral$"))
                {
                    namesByAlignment[2, 2].Add(currentName);
                }

                else if (monsterNameLine == 2 && Regex.IsMatch(line, @"unaligned"))
                {
                    namesOfUnaligned.Add(currentName);
                }

                else if (monsterNameLine == 2 && Regex.IsMatch(line, @"any alignment"))
                {
                    namesOfAnyAlignment.Add(currentName);
                }

                else if (monsterNameLine == 2 && Regex.IsMatch(line, @"any"))
                {
                    namesOfSpecialCases.Add(currentName);
                }

            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.WriteLine($"Monsters with alignment {typesOfAlignmentAxis1[i]} {typesOfAlignmentAxis2[j]} are:");
                    foreach (string name in namesByAlignment[i, j])
                    {
                        Console.WriteLine(name);
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Unaligned Monsters are:");
            foreach (string name in namesOfUnaligned)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();

            Console.WriteLine("Monsters of any alignment are:");
            foreach (string name in namesOfAnyAlignment)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();

            Console.WriteLine("Special case Monsters are:");
            foreach (string name in namesOfSpecialCases)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();

        }

    }
}