using System;

class BowlingDisplay
{
    static void Main()
    {
        int totalFrames = 5; // You can set this between 1 and 10
        Random rand = new Random();
        string[] rollDisplays = new string[totalFrames];

        for (int i = 0; i < totalFrames; i++)
        {
            int roll1 = rand.Next(0, 11);
            int roll2 = 0;
            string display = "";

            if (roll1 == 10)
            {
                display = "  X  ";
            }
            else
            {
                roll2 = rand.Next(0, 11 - roll1);
                string r1 = roll1 == 0 ? "-" : roll1.ToString();
                string r2 = roll2 == 0 ? "-" : (roll1 + roll2 == 10 ? "/" : roll2.ToString());
                display = $" {r1} {r2} ";
            }

            rollDisplays[i] = display;
        }

        // Top border
        for (int i = 0; i < totalFrames; i++) Console.Write("+-----");
        Console.WriteLine("+");

        // First row: Rolls
        for (int i = 0; i < totalFrames; i++) Console.Write($"|{rollDisplays[i]}");
        Console.WriteLine("|");

        // Second row: fixed graphic
        for (int i = 0; i < totalFrames; i++) Console.Write("| ----");
        Console.WriteLine("|");

        // Third row: empty bottom
        for (int i = 0; i < totalFrames; i++) Console.Write("|     ");
        Console.WriteLine("|");

        // Bottom border
        for (int i = 0; i < totalFrames; i++) Console.Write("+-----");
        Console.WriteLine("+");
    }
}
