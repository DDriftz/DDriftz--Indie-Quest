﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// Enum for armor categories as specified in Monster manual 4.docx
enum ArmorCategory
{
    Light,
    Medium,
    Heavy
}

// Enhanced ArmorTypeId enum to match ArmorTypes.txt and Monster manual 3.docx
enum ArmorTypeId
{
    Unspecified,
    Natural,
    Leather,
    StuddedLeather,
    Hide,
    ChainShirt,
    ChainMail,
    ScaleMail,
    Plate,
    Other
}

// Class to hold armor type details as per Monster manual 4.docx
class ArmorType
{
    public string DisplayName { get; set; }
    public ArmorCategory Category { get; set; }
    public int Weight { get; set; }
}

// Class to hold monster data as per Monster manual 1.docx and Monster manual 3.docx
class MonsterType
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Alignment { get; set; }
    public string HitPointsRoll { get; set; }
    public int ArmorClass { get; set; }
    public ArmorTypeId ArmorType { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Load armor types and monsters
        var armorTypes = GenerateAllArmorTypesFromFile("ArmorTypes.txt");
        var monsters = GenerateMonstersFromFile("MonsterManual.txt");

        // Display title as per Monster manual 2.docx
        Console.WriteLine("MONSTER MANUAL");

        while (true)
        {
            // Prompt for search type as per Monster manual 3.docx
            Console.WriteLine("Do you want to search by (n)ame or (a)rmor type?");
            string searchChoice = Console.ReadLine()?.Trim().ToLowerInvariant();

            List<MonsterType> queryResults = new List<MonsterType>();
            if (searchChoice == "n")
            {
                // Search by name as per Monster manual 2.docx
                Console.WriteLine("Enter a query to search monsters by name:");
                string searchTerm = Console.ReadLine()?.Trim().ToLowerInvariant();

                queryResults = monsters.Where(m => m.Name.ToLowerInvariant().Contains(searchTerm)).ToList();

                if (queryResults.Count == 0)
                {
                    Console.WriteLine("No monsters were found. Try again:");
                    continue;
                }
            }
            else if (searchChoice == "a")
            {
                // Search by armor type as per Monster manual 3.docx
                Console.WriteLine("Which armor type do you want to display?");
                var armorTypeNames = Enum.GetNames<ArmorTypeId>();
                for (int i = 0; i < armorTypeNames.Length; i++)
                {
                    Console.WriteLine($"  {i + 1}: {armorTypeNames[i]}");
                }

                string input = Console.ReadLine()?.Trim();
                if (!int.TryParse(input, out int armorChoice) || armorChoice < 1 || armorChoice > armorTypeNames.Length)
                {
                    Console.WriteLine("Invalid input. Try again:");
                    continue;
                }

                ArmorTypeId selectedArmorType = (ArmorTypeId)(armorChoice - 1);
                queryResults = monsters.Where(m => m.ArmorType == selectedArmorType).ToList();

                if (queryResults.Count == 0)
                {
                    Console.WriteLine("No monsters were found for this armor type. Try again:");
                    continue;
                }
            }
            else
            {
                Console.WriteLine("Invalid choice. Please enter 'n' or 'a':");
                continue;
            }

            // If only one match, auto-select as per Monster manual 2.docx
            MonsterType selectedMonster;
            if (queryResults.Count == 1)
            {
                selectedMonster = queryResults[0];
            }
            else
            {
                // Display list for selection
                Console.WriteLine("Which monster did you want to look up?");
                for (int i = 0; i < queryResults.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}: {queryResults[i].Name}");
                }

                string numberInput = Console.ReadLine()?.Trim();
                int numberInputAsIndex;
                while (string.IsNullOrEmpty(numberInput) || !int.TryParse(numberInput, out numberInputAsIndex) || 
                       numberInputAsIndex < 1 || numberInputAsIndex > queryResults.Count)
                {
                    Console.WriteLine("Invalid input. Please enter a valid number:");
                    numberInput = Console.ReadLine()?.Trim();
                }

                selectedMonster = queryResults[numberInputAsIndex - 1];
            }

            // Display monster details as per Monster manual 4.docx
            Console.WriteLine($"Displaying information for {selectedMonster.Name}.");
            Console.WriteLine($"Name: {selectedMonster.Name}");
            Console.WriteLine($"Description: {selectedMonster.Description}");
            Console.WriteLine($"Alignment: {selectedMonster.Alignment}");
            Console.WriteLine($"Hit points roll: {selectedMonster.HitPointsRoll}");
            Console.WriteLine($"Armor class: {selectedMonster.ArmorClass}");
            if (selectedMonster.ArmorType != ArmorTypeId.Unspecified)
            {
                var armor = armorTypes[selectedMonster.ArmorType];
                Console.WriteLine($"Armor type: {armor.DisplayName}");
                Console.WriteLine($"Armor category: {armor.Category}");
                Console.WriteLine($"Armor weight: {armor.Weight} lb.");
            }

            // Ask if user wants to search again
            Console.WriteLine("\nDo you want to search again? (y/n)");
            if (Console.ReadLine()?.Trim().ToLowerInvariant() != "y")
            {
                break;
            }
        }
    }

    static Dictionary<ArmorTypeId, ArmorType> GenerateAllArmorTypesFromFile(string armorTypesPath)
    {
        var armorTypes = new Dictionary<ArmorTypeId, ArmorType>();

        if (!File.Exists(armorTypesPath))
        {
            Console.WriteLine($"File {armorTypesPath} not found.");
            return armorTypes;
        }

        string[] lines = File.ReadAllLines(armorTypesPath);
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length != 4) continue;

            string typeName = parts[0].Trim();
            if (!Enum.TryParse<ArmorTypeId>(typeName, true, out var armorTypeId))
            {
                armorTypeId = ArmorTypeId.Other;
            }

            armorTypes[armorTypeId] = new ArmorType
            {
                DisplayName = parts[1].Trim(),
                Category = Enum.Parse<ArmorCategory>(parts[2].Trim()),
                Weight = int.Parse(parts[3].Trim())
            };
        }

        return armorTypes;
    }

    static List<MonsterType> GenerateMonstersFromFile(string monsterManualPath)
    {
        var monsters = new List<MonsterType>();
        if (!File.Exists(monsterManualPath))
        {
            Console.WriteLine($"File {monsterManualPath} not found.");
            return monsters;
        }

        string[] lines = File.ReadAllLines(monsterManualPath);
        MonsterType currentMonster = null;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                if (currentMonster != null)
                {
                    monsters.Add(currentMonster);
                    currentMonster = null;
                }
                continue;
            }

            if (currentMonster == null)
            {
                currentMonster = new MonsterType { Name = line };
                continue;
            }

            if (line.Contains(","))
            {
                string[] parts = line.Split(',');
                currentMonster.Description = parts[0].Trim();
                currentMonster.Alignment = parts[1].Trim();
            }
            else if (line.StartsWith("Hit Points:"))
            {
                string hpInfo = line.Substring("Hit Points:".Length).Trim();
                var match = Regex.Match(hpInfo, @"\d+\s*\((.*?)\)");
                if (match.Success)
                {
                    currentMonster.HitPointsRoll = match.Groups[1].Value;
                }
            }
            else if (line.StartsWith("Armor Class:"))
            {
                string acInfo = line.Substring("Armor Class:".Length).Trim();
                var match = Regex.Match(acInfo, @"(\d+)\s*(?:\((.*?)\))?");
                if (match.Success)
                {
                    currentMonster.ArmorClass = int.Parse(match.Groups[1].Value);
                    string armorTypeText = match.Groups[2].Success ? match.Groups[2].Value : "";
                    currentMonster.ArmorType = ParseArmorType(armorTypeText);
                }
            }
        }

        if (currentMonster != null)
        {
            monsters.Add(currentMonster);
        }

        return monsters;
    }

    static ArmorTypeId ParseArmorType(string armorTypeText)
    {
        if (string.IsNullOrEmpty(armorTypeText))
            return ArmorTypeId.Unspecified;

        armorTypeText = armorTypeText.Split(',')[0].Trim(); // Handle cases like "Natural Armor, 11 While Prone"
        if (Enum.TryParse<ArmorTypeId>(armorTypeText.Replace(" ", ""), true, out var armorType))
        {
            return armorType;
        }

        return ArmorTypeId.Other;
    }
}