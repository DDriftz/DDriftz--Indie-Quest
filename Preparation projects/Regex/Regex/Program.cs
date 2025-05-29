using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

public class Monster
{
    public required string Name { get; set; }
    public string? Speed { get; set; }
    public string? HitPointsRoll { get; set; }
}

public class MonsterManual1
{
    public List<Monster> ParseMonsterData(string filePath)
    {
        // Dummy implementation for demonstration; replace with actual file parsing logic
        // For now, returns a sample list of monsters
        return new List<Monster>
        {
            new Monster { Name = "Dragon", Speed = "fly 40 ft., walk 30 ft.", HitPointsRoll = "12d10+24" },
            new Monster { Name = "Goblin", Speed = "walk 30 ft.", HitPointsRoll = "2d6+2" },
            new Monster { Name = "Harpy", Speed = "fly 40 ft., walk 20 ft.", HitPointsRoll = "7d8+7" },
            new Monster { Name = "Pixie", Speed = "fly 30 ft.", HitPointsRoll = "1d4" }
        };
    }
}

public class Regex1
{
    private List<Monster> monsters;

    public Regex1()
    {
        monsters = new MonsterManual1().ParseMonsterData("MonsterManual.txt");
    }

    /// <summary>
    /// Regex1: Parses and prints monster names
    /// </summary>
    public void Run()
    {
        Mission1();
        Console.WriteLine();
        Mission2();
        Console.WriteLine();
        Mission3();
        Console.WriteLine();
        Mission4();
        Console.WriteLine();
    }

    /// <summary>
    /// Mission 1: Prints all monsters in the manual
    /// </summary>
    private void Mission1()
    {
        Console.WriteLine("Mission 1");
        Console.WriteLine("Monsters in the manual are:");
        foreach (var monster in monsters)
        {
            Console.WriteLine(monster.Name);
        }
    }

    /// <summary>
    /// Mission 2: Prints monsters that can fly
    /// </summary>
    private void Mission2()
    {
        Console.WriteLine("Mission 2");
        Console.WriteLine("Monsters in the manual are:");
        foreach (var monster in monsters)
        {
            // Check if the monster has a flying speed
            var canFly = monster.Speed?.Contains("fly ") ?? false;
            Console.WriteLine($"{monster.Name} - Can fly: {canFly}");
        }
    }

    /// <summary>
    /// Mission 3: Prints monsters with 10+ dice rolls
    /// </summary>
    private void Mission3()
    {
        Console.WriteLine("Mission 3");
        Console.WriteLine("Monsters in the manual are:");
        var hitPointsRoll = new Regex(@"^(\d+)[dD].+$");
        foreach (var monster in monsters)
        {
            var hpMatch = hitPointsRoll.Match(monster.HitPointsRoll ?? "");
            // Check if the monster has 10+ dice rolls
            var moreThan10 = hpMatch.Success && int.TryParse(hpMatch.Groups[1].Value, out var rolls) && rolls >= 10;

            Console.WriteLine($"{monster.Name} - 10+ dice rolls: {moreThan10}");
        }
    }

    /// <summary>
    /// Mission 4: Prints monsters that can fly 10-49 feet per turn
    /// </summary>
    private void Mission4()
    {
        Console.WriteLine("Mission 4");
        Console.WriteLine("Monsters that can fly 10-49 feet per turn:");
        var flyingSpeed = new Regex(@"fly (\d+) ft\.");
        foreach (var monster in monsters)
        {
            var flyingSpeedMatch = flyingSpeed.Match(monster.Speed ?? "");
            // Check if the monster has a flying speed
            int? flyingSpeedValue = flyingSpeedMatch.Success && int.TryParse(flyingSpeedMatch.Groups[1].Value, out var speed) ? speed : null;

            if (flyingSpeedValue >= 10 && flyingSpeedValue <= 49)
            {
                Console.WriteLine($"{monster.Name}");
            }
        }
    }
}