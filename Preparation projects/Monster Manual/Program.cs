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
// Adding more specific types based on MonsterManual.txt data if they occur often
// and are not meant to be "Other".
enum ArmorTypeId
{
    Unspecified,    // No armor type specified
    Natural,        // e.g., "Natural Armor"
    Leather,        // e.g., "Leather Armor"
    StuddedLeather, // e.g., "Studded Leather"
    Hide,           // e.g., "Hide Armor"
    ChainShirt,     // e.g., "Chain Shirt"
    ChainMail,      // e.g., "Chain Mail"
    ScaleMail,      // e.g., "Scale Mail"
    Plate,          // e.g., "Plate"
    Splint,         // e.g., "Splint"
    Breastplate,    // e.g., "Breastplate"
    Patchwork,      // e.g., "Patchwork Armor"
    Shield,         // If "Shield" appears as a primary armor type, though usually it's an addition
    Other           // For types not fitting above or complex descriptions like "Mage Armor"
}

// Class to hold armor type details as per Monster manual 4.docx
class ArmorType // This is the class for detailed armor info
{
    public string DisplayName { get; set; }
    public ArmorCategory Category { get; set; }
    public int Weight { get; set; }
}

// Class to hold monster data as per Monster manual 1.docx and Monster manual 3.docx
class MonsterTypeData // Renamed to avoid conflict with common type naming, but user's file uses MonsterType
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Alignment { get; set; }
    public string HitPointsRoll { get; set; } // Stores the dice notation e.g., "18d10+36"
    public int ArmorClass { get; set; }
    public ArmorTypeId ArmorType { get; set; } // Enum for categorized armor type
    public string RawArmorTypeString { get; set; } // Stores the original string from parentheses
}

class Program
{
    static void Main(string[] args)
    {
        // Load armor types and monsters
        // Assumes ArmorTypes.txt and MonsterManual.txt are in the executable's directory
        var armorTypesData = GenerateAllArmorTypesFromFile("ArmorTypes.txt");
        var monsters = GenerateMonstersFromFile("MonsterManual.txt");

        // Display title as per Monster manual 2.docx
        Console.WriteLine("MONSTER MANUAL");
        Console.WriteLine("================\n");

        while (true) // Main application loop
        {
            // Prompt for search type, now including a quit option
            Console.WriteLine("Do you want to search by (n)ame or (a)rmor type? (Enter 'q' to quit)");
            string searchChoice = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (searchChoice == "q")
            {
                Console.WriteLine("Exiting Monster Manual. Goodbye!");
                break; // Exit the main loop, and thus the program
            }

            List<MonsterTypeData> queryResults = new List<MonsterTypeData>();

            if (searchChoice == "n")
            {
                Console.WriteLine("Enter a query to search monsters by name:");
                string searchTerm = Console.ReadLine()?.Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(searchTerm))
                {
                    Console.WriteLine("Search term cannot be empty. Try again.\n");
                    continue; // Go back to the start of the main loop
                }
                queryResults = monsters.Where(m => m.Name.ToLowerInvariant().Contains(searchTerm)).ToList();
            }
            else if (searchChoice == "a")
            {
                Console.WriteLine("Which armor type do you want to display?");
                var armorTypeEnumValues = Enum.GetValues<ArmorTypeId>();
                for (int i = 0; i < armorTypeEnumValues.Length; i++)
                {
                    Console.WriteLine($"  {i + 1}: {armorTypeEnumValues[i]}");
                }

                Console.Write("Enter number: ");
                string input = Console.ReadLine()?.Trim();
                if (!int.TryParse(input, out int armorChoiceIndex) || armorChoiceIndex < 1 || armorChoiceIndex > armorTypeEnumValues.Length)
                {
                    Console.WriteLine("Invalid input. Try again.\n");
                    continue; // Go back to the start of the main loop
                }

                ArmorTypeId selectedArmorEnumVal = armorTypeEnumValues[armorChoiceIndex - 1];
                queryResults = monsters.Where(m => m.ArmorType == selectedArmorEnumVal).ToList();
            }
            else
            {
                Console.WriteLine("Invalid choice. Please enter 'n', 'a', or 'q'.\n");
                continue; // Go back to the start of the main loop
            }

            // Handling search results
            if (queryResults.Count == 0)
            {
                Console.WriteLine("No monsters were found for your query.\n");
                // Loop will continue to the top automatically
                continue;
            }

            MonsterTypeData selectedMonster;
            if (queryResults.Count == 1)
            {
                selectedMonster = queryResults[0];
                Console.WriteLine($"Automatically selected: {selectedMonster.Name}\n");
            }
            else
            {
                Console.WriteLine("Which monster did you want to look up?");
                for (int i = 0; i < queryResults.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}: {queryResults[i].Name}");
                }

                Console.Write("Enter number: ");
                string numberInput = Console.ReadLine()?.Trim();
                // Loop for valid monster selection input
                while (string.IsNullOrEmpty(numberInput) ||
                       !int.TryParse(numberInput, out int numberInputAsIndex) ||
                       numberInputAsIndex < 1 || numberInputAsIndex > queryResults.Count)
                {
                    Console.WriteLine("Invalid input. Please enter a valid number from the list:");
                    numberInput = Console.ReadLine()?.Trim();
                }
                selectedMonster = queryResults[numberInputAsIndex - 1];
            }

            // Display monster details
            Console.WriteLine($"\n--- Displaying information for {selectedMonster.Name} ---");
            Console.WriteLine($"Name: {selectedMonster.Name}");
            Console.WriteLine($"Description: {selectedMonster.Description}");
            Console.WriteLine($"Alignment: {selectedMonster.Alignment}");
            Console.WriteLine($"Hit Points Roll: {selectedMonster.HitPointsRoll}");
            Console.WriteLine($"Armor Class: {selectedMonster.ArmorClass}");

            if (armorTypesData.TryGetValue(selectedMonster.ArmorType, out ArmorType specificArmorInfo))
            {
                Console.WriteLine($"Armor Type: {specificArmorInfo.DisplayName}");
                Console.WriteLine($"Armor Category: {specificArmorInfo.Category}");
                Console.WriteLine($"Armor Weight: {specificArmorInfo.Weight} lb.");
            }
            else if (selectedMonster.ArmorType != ArmorTypeId.Unspecified)
            {
                Console.WriteLine($"Armor Type: {selectedMonster.ArmorType}");
            }
            
            if (!string.IsNullOrEmpty(selectedMonster.RawArmorTypeString) &&
                (selectedMonster.ArmorType == ArmorTypeId.Other || selectedMonster.ArmorType == ArmorTypeId.Unspecified || !armorTypesData.ContainsKey(selectedMonster.ArmorType)))
            {
                Console.WriteLine($"Armor Details: ({selectedMonster.RawArmorTypeString})");
            }
            Console.WriteLine("---------------------------------------\n");
            // The loop will now naturally continue to the top, asking for search type again.
        }
    }

    static Dictionary<ArmorTypeId, ArmorType> GenerateAllArmorTypesFromFile(string armorTypesPath)
    {
        var armorTypes = new Dictionary<ArmorTypeId, ArmorType>();
        if (!File.Exists(armorTypesPath))
        {
            Console.WriteLine($"Warning: Armor types file not found at '{armorTypesPath}'. Detailed armor information will be limited.");
            return armorTypes;
        }

        string[] lines = File.ReadAllLines(armorTypesPath);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(',');
            if (parts.Length != 4)
            {
                Console.WriteLine($"Warning: Skipping malformed line in armor types file: {line}");
                continue;
            }
            try
            {
                if (!Enum.TryParse<ArmorTypeId>(parts[0].Trim(), true, out var armorId))
                {
                    Console.WriteLine($"Warning: Unknown ArmorTypeId '{parts[0]}' in armor types file. Skipping line: {line}");
                    continue;
                }
                if (!Enum.TryParse<ArmorCategory>(parts[2].Trim(), true, out var category))
                {
                    Console.WriteLine($"Warning: Unknown ArmorCategory '{parts[2]}' for '{parts[0]}' in armor types file. Skipping line: {line}");
                    continue;
                }
                armorTypes[armorId] = new ArmorType
                {
                    DisplayName = parts[1].Trim(),
                    Category = category,
                    Weight = int.Parse(parts[3].Trim())
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error parsing line in armor types file: '{line}'. Error: {ex.Message}");
            }
        }
        return armorTypes;
    }

    static List<MonsterTypeData> GenerateMonstersFromFile(string monsterManualPath)
    {
        var monsters = new List<MonsterTypeData>();
        if (!File.Exists(monsterManualPath))
        {
            Console.WriteLine($"Error: Monster manual file not found at '{monsterManualPath}'. Cannot load monsters.");
            return monsters;
        }

        var lines = File.ReadAllLines(monsterManualPath);
        List<string> currentBlock = new List<string>();

        foreach (string lineContent in lines)
        {
            string line = lineContent.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                if (currentBlock.Count > 0)
                {
                    var monster = ParseMonsterBlock(currentBlock);
                    if (monster != null) monsters.Add(monster);
                    currentBlock.Clear();
                }
            }
            else
            {
                currentBlock.Add(line);
            }
        }
        if (currentBlock.Count > 0) // Process the last block if file doesn't end with blank line
        {
            var monster = ParseMonsterBlock(currentBlock);
            if (monster != null) monsters.Add(monster);
        }
        return monsters;
    }

    static MonsterTypeData ParseMonsterBlock(List<string> block)
    {
        if (block.Count == 0) return null;

        var monster = new MonsterTypeData { Name = block[0] };

        if (block.Count > 1)
        {
            string[] descAlignParts = block[1].Split(new[] { ',' }, 2);
            monster.Description = descAlignParts[0].Trim();
            monster.Alignment = descAlignParts.Length > 1 ? descAlignParts[1].Trim() : "unknown";
        }

        foreach (string line in block)
        {
            if (line.StartsWith("Hit Points:", StringComparison.OrdinalIgnoreCase))
            {
                Match hpMatch = Regex.Match(line, @"\((.*?)\)");
                monster.HitPointsRoll = hpMatch.Success ? hpMatch.Groups[1].Value.Trim() : line.Substring("Hit Points:".Length).Trim();
            }
            else if (line.StartsWith("Armor Class:", StringComparison.OrdinalIgnoreCase))
            {
                Match acMatch = Regex.Match(line.Substring("Armor Class:".Length).Trim(), @"(\d+)\s*(?:\((.*?)\))?");
                if (acMatch.Success)
                {
                    monster.ArmorClass = int.Parse(acMatch.Groups[1].Value);
                    monster.RawArmorTypeString = acMatch.Groups[2].Success ? acMatch.Groups[2].Value.Trim() : string.Empty;
                    monster.ArmorType = ParseArmorType(monster.RawArmorTypeString);
                }
            }
        }
        return monster;
    }

    static ArmorTypeId ParseArmorType(string armorTypeText)
    {
        if (string.IsNullOrWhiteSpace(armorTypeText))
            return ArmorTypeId.Unspecified;

        // Take only the part before a comma for primary type matching
        string primaryArmorType = armorTypeText.Split(',')[0].Trim();
        string processedTypeName = primaryArmorType.Replace(" ", ""); // For "Chain Shirt" -> "ChainShirt"

        // Try parsing directly (case-insensitive)
        if (Enum.TryParse<ArmorTypeId>(processedTypeName, true, out var armorType))
        {
            return armorType;
        }

        // Handle specific known string variations not directly matching enum names
        if (primaryArmorType.Equals("Natural Armor", StringComparison.OrdinalIgnoreCase)) return ArmorTypeId.Natural;
        if (primaryArmorType.Equals("Leather Armor", StringComparison.OrdinalIgnoreCase)) return ArmorTypeId.Leather;
        if (primaryArmorType.Equals("Studded Leather Armor", StringComparison.OrdinalIgnoreCase)) return ArmorTypeId.StuddedLeather; // More specific
        if (primaryArmorType.Equals("Hide Armor", StringComparison.OrdinalIgnoreCase)) return ArmorTypeId.Hide;
        if (primaryArmorType.Equals("Patchwork Armor", StringComparison.OrdinalIgnoreCase)) return ArmorTypeId.Patchwork;
        // "Chain Shirt", "Scale Mail", "Chain Mail", "Plate", "Splint", "Breastplate" should be caught by Enum.TryParse if enums are named correctly.

        return ArmorTypeId.Other; // Fallback for unmatched types
    }
}
