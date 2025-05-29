using System;

class Program
{
    static void Main()
    {
        Random random = new Random();

        int firstRoll = random.Next(1, 7);
        int secondRoll = random.Next(1, 7);
        int thirdRoll = random.Next(1, 7);

        Console.WriteLine("First dice throw is " + firstRoll + ".");
        Console.WriteLine("Second dice throw is " + secondRoll + ".");
        Console.WriteLine("Third dice throw is " + thirdRoll + ".");

        Console.ReadLine();
    }
}
