using System.Text.RegularExpressions;

public class Regex2
{
    /// <summary>
    /// Entry point that runs all three missions
    /// </summary>
    public void Run()
    {
        Mission1();
        Console.WriteLine();
        Mission2();
        Console.WriteLine();
        Mission3();
    }
    
    /// <summary>
    /// Dummy implementation of MonsterManual1 to resolve missing reference error.
    /// Replace this with your actual implementation if needed.
    /// </summary>
    public class MonsterManual1
    {
        public List<Monster> ParseMonsterData(string filePath)
        {
            // Dummy data for demonstration; replace with actual file parsing logic.
            return new List<Monster>
            {
                new Monster { Name = "Goblin", Alignment = "neutral evil" },
                new Monster { Name = "Dragon", Alignment = "chaotic good" },
                new Monster { Name = "Golem", Alignment = "unaligned" }
            };
        }
    }
    
    public class Monster
    {
        public string? Name { get; set; }
        public string? Alignment { get; set; }
    }

    /// <summary>
    /// Mission 1: Hexadecimal numbers
    /// </summary>
    private void Mission1()
    {
        Console.WriteLine("Mission 1");
        var regex = new Regex("^0x[0-9A-Fa-f]+");
        var inputs = new[]
        {
            // (input string, is hexadecimal?)
            ("42", false),
            ("0x42", true),
            ("0x2A", true),
            ("0xFFEE", true),
            ("x2A", false),
            ("0b2A", false),
            ("0x2GA", false),
            ("10x2A", false),
        };

        foreach (var (input, isHexadecimal) in inputs)
        {
            if (regex.IsMatch(input) == isHexadecimal)
            {
                Console.WriteLine($"Input {input} was correctly identified as {(isHexadecimal ? "hexadecimal" : "NOT hexadecimal")}");
            }
            else
            {
                Console.WriteLine($"Input {input} was incorrectly identified as {(isHexadecimal ? "hexadecimal" : "NOT hexadecimal")}");
            }
        }
    }

    /// <summary>
    /// Mission 2: Dice notation
    /// </summary>
    private void Mission2()
    {
        Console.WriteLine("Mission 2");
        var diceNotation = new Regex("^[1-9][0-9]*[dD][1-9][0-9]*$");
        var inputs = new[]
        {
            // (input string, is valid dice notation?)
            ("1d20", true),
            ("6d6", true),
            ("9321d43", true),
            ("1d1", true),
            ("1d", false),
            ("d1", false),
            ("1d20d", false),
        };

        foreach (var (input, isValid) in inputs)
        {
            if (diceNotation.IsMatch(input) == isValid)
            {
                Console.WriteLine($"Notation {input} was correctly identified as {(isValid ? "valid" : "NOT valid")}");
            }
            else
            {
                Console.WriteLine($"Notation {input} was incorrectly identified as {(isValid ? "NOT valid" : "valid")}");
            }
        }
    }

    /// <summary>
    /// Mission 3: Monster alignment
    /// </summary>
    private void Mission3()
    {
        Console.WriteLine("Mission 3");

        // We defer to the alignment extraction in MonsterManual1
        var monsters = new MonsterManual1().ParseMonsterData("MonsterManual.txt");

        foreach (var monster in monsters)
        {
            if (monster.Alignment != "unaligned")
            {
                Console.WriteLine($"{monster.Name} ({monster.Alignment})");
            }
        }
    }
}