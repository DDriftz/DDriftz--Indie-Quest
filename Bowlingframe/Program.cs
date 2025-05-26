using System;
    {
        Random random = new Random();
        int totalPins = 10;
        int firstRoll = random.Next(0, totalPins + 1);
        int pinsLeft = totalPins - firstRoll;
        int secondRoll = random.Next(0, pinsLeft + 1);
        Console.WriteLine($"First roll: {firstRoll}");
        Console.WriteLine($"Second roll: {secondRoll}");
        Console.ReadLine();
    }

