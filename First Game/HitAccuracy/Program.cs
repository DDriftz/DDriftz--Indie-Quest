
    {
        Random random = new Random();
        int totalShots = random.Next(10, 21);
        int hitsMade = random.Next(0, totalShots + 1);
        double accuracy = ((double)hitsMade / totalShots) * 100;
        Console.WriteLine($"Total shots: {totalShots}");
        Console.WriteLine($"Hits made: {hitsMade}");
        Console.WriteLine($"Hit accuracy: {accuracy}%");
        Console.ReadLine();
    }
