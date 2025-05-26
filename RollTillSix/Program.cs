    {
        Random random = new Random();
        int roll;
        int totalScore = 0;
        do
        {
            roll = random.Next(1, 7);  
            Console.WriteLine("The player rolls: " + roll); 
            totalScore += roll; 
        }
        while (roll != 6); 
        Console.WriteLine("Total score: " + totalScore);
    }