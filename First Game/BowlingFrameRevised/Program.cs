    {
        Random random = new Random();
        int firstRoll = random.Next(0, 11); // Between 0 and 10
        if (firstRoll == 10)
        {
            Console.WriteLine("First roll: X");
            Console.WriteLine("Knocked pins: 10");
        }
        else
        {
            int secondRoll = random.Next(0, 11 - firstRoll); // Remaining pins
            string firstRollDisplay = firstRoll == 0 ? "-" : firstRoll.ToString();
            string secondRollDisplay;
            if (firstRoll + secondRoll == 10)
            {
                secondRollDisplay = "/";
            }
            else
            {
                secondRollDisplay = secondRoll == 0 ? "-" : secondRoll.ToString();
            }
            int totalPins = firstRoll + secondRoll;
            Console.WriteLine($"First roll: {firstRollDisplay}");
            Console.WriteLine($"Second roll: {secondRollDisplay}");
            Console.WriteLine($"Knocked pins: {totalPins}");
        }
        Console.ReadLine();
    }