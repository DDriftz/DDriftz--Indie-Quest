using System;

public class Program
{
    public static void Main()
    {
        // Execute Mission 2: Increasing Level Difficulty.
        MonsterLevels.PrintMonsterLevels();
    }
}

public class MonsterLevels
{
    public static void PrintMonsterLevels()
    {
        int[] monsters = new int[100];
        Random rand = new Random();

        // Fill the array with random numbers between 1 and 50.
        for (int i = 0; i < monsters.Length; i++)
        {
            monsters[i] = rand.Next(1, 51); // 51 is exclusive.
        }

        // Sort the array so that the number of monsters is in increasing order.
        Array.Sort(monsters);

        // Output the results.
        Console.WriteLine("Number of monsters in levels: " + string.Join(", ", monsters));
    }
}
