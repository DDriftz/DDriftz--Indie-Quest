    {
        Random random = new Random();
        int warriorHP = random.Next(1, 101);
        Console.WriteLine($"Warrior HP: {warriorHP}");
        Console.WriteLine("The Regenerate spell is cast!");
        while (warriorHP < 90)
        {
            warriorHP += 10;
            Console.WriteLine($"Warrior HP: {warriorHP}");
        }
        Console.WriteLine("The Regenerate spell is complete.");
        Console.ReadLine();
    }
