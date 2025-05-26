using System;

class Program
{
    static void Main()
    {
        Random random = new Random();

        int firstRoll = random.Next(1, 7);
        int secondRoll = random.Next(1, 7);
        int thirdRoll = random.Next(1, 7);

        int totalScore = (firstRoll + secondRoll + (thirdRoll * 3)) * 2;

        Console.WriteLine($"You roll: {firstRoll}, {secondRoll}, {thirdRoll}");
        Console.WriteLine($"The total score is: {totalScore}");
     
        Console.ReadLine();
    }
}
