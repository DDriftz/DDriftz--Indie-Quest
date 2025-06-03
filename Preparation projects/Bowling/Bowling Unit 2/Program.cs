using System;

class BowlingGame
{
    static Random rand = new Random();

    static void Main()
    {
        Console.WriteLine("SIMULATING 10 BOWLING FRAMES ...");
        int[][] rolls = SimulateGame();
        Console.WriteLine("DONE!");
        Console.WriteLine("Results:");

        int[] pointsGained = new int[10];
        int[] frameScores = new int[10];
        CalculateScores(rolls, pointsGained, frameScores);

        DisplayGame(rolls, pointsGained, frameScores);
        DisplayScoreSheet(rolls, frameScores);
    }

    static int[][] SimulateGame()
    {
        int[][] rolls = new int[10][];
        for (int frame = 0; frame < 10; frame++)
        {
            if (frame < 9)
            {
                rolls[frame] = new int[2];
                rolls[frame][0] = rand.Next(0, 11);
                if (rolls[frame][0] == 10)
                {
                    rolls[frame][1] = -1;
                }
                else
                {
                    int remainingPins = 10 - rolls[frame][0];
                    rolls[frame][1] = rand.Next(0, remainingPins + 1);
                }
            }
            else
            {
                rolls[frame] = new int[3];
                rolls[frame][0] = rand.Next(0, 11);
                if (rolls[frame][0] == 10)
                {
                    rolls[frame][1] = rand.Next(0, 11);
                    rolls[frame][2] = rolls[frame][1] == 10 ? rand.Next(0, 11) : rand.Next(0, 11 - rolls[frame][1] + 1);
                }
                else
                {
                    int remainingPins = 10 - rolls[frame][0];
                    rolls[frame][1] = rand.Next(0, remainingPins + 1);
                    if (rolls[frame][0] + rolls[frame][1] == 10)
                    {
                        rolls[frame][2] = rand.Next(0, 11);
                    }
                    else
                    {
                        rolls[frame][2] = -1;
                    }
                }
            }
        }
        return rolls;
    }

    static void CalculateScores(int[][] rolls, int[] pointsGained, int[] frameScores)
    {
        int totalScore = 0;
        for (int frame = 0; frame < 10; frame++)
        {
            int framePins = rolls[frame][0] == 10 ? 10 : rolls[frame][0] + (rolls[frame][1] == -1 ? 0 : rolls[frame][1]);
            if (frame < 9)
            {
                if (rolls[frame][0] == 10)
                {
                    pointsGained[frame] = 10 + GetNextTwoRolls(rolls, frame);
                }
                else if (rolls[frame][0] + rolls[frame][1] == 10)
                {
                    pointsGained[frame] = 10 + GetNextRoll(rolls, frame);
                }
                else
                {
                    pointsGained[frame] = framePins;
                }
            }
            else
            {
                pointsGained[frame] = framePins + (rolls[frame][2] == -1 ? 0 : rolls[frame][2]);
            }
            totalScore += pointsGained[frame];
            frameScores[frame] = totalScore;
        }
    }

    static int GetNextRoll(int[][] rolls, int currentFrame)
    {
        if (currentFrame + 1 >= 10) return rolls[10][0];
        if (rolls[currentFrame + 1][0] == 10) return 10;
        return rolls[currentFrame + 1][0] + (rolls[currentFrame + 1][1] == -1 ? 0 : rolls[currentFrame + 1][1]);
    }

    static int GetNextTwoRolls(int[][] rolls, int currentFrame)
    {
        int sum = 0;
        if (currentFrame + 1 >= 10)
        {
            sum += rolls[9][0];
            sum += rolls[9][1] == -1 ? 0 : rolls[9][1];
            return sum;
        }
        if (rolls[currentFrame + 1][0] == 10)
        {
            sum += 10;
            if (currentFrame + 2 < 10)
            {
                sum += rolls[currentFrame + 2][0];
            }
            else
            {
                sum += rolls[9][1];
            }
        }
        else
        {
            sum += rolls[currentFrame + 1][0];
            sum += rolls[currentFrame + 1][1] == -1 ? 0 : rolls[currentFrame + 1][1];
        }
        return sum;
    }

    static void DisplayGame(int[][] rolls, int[] pointsGained, int[] frameScores)
    {
        for (int frame = 0; frame < 10; frame++)
        {
            Console.WriteLine($"FRAME {frame + 1}");
            for (int roll = 0; roll < rolls[frame].Length; roll++)
            {
                if (rolls[frame][roll] == -1)
                {
                    if (roll < 2 || frame < 9) Console.WriteLine($"Roll {roll + 1}: -");
                }
                else if (roll == 0 && rolls[frame][roll] == 10)
                {
                    Console.WriteLine("Roll 1: X");
                }
                else if (roll == 1 && rolls[frame][0] + rolls[frame][1] == 10)
                {
                    Console.WriteLine("Roll 2: /");
                }
                else
                {
                    Console.WriteLine($"Roll {roll + 1}: {rolls[frame][roll]}");
                }
            }
            int framePins = rolls[frame][0] == 10 ? 10 : rolls[frame][0] + (rolls[frame][1] == -1 ? 0 : rolls[frame][1]);
            if (frame == 9 && rolls[frame][2] != -1) framePins += rolls[frame][2];
            Console.WriteLine($"Knocked pins: {framePins}");
            Console.WriteLine($"Points gained: {pointsGained[frame]}");
            Console.WriteLine($"Score: {frameScores[frame]}");
        }
    }

    static void DisplayScoreSheet(int[][] rolls, int[] frameScores)
    {
        Console.WriteLine("┌─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┐");
        Console.Write("│ ");
        for (int frame = 0; frame < 10; frame++)
        {
            if (frame < 9)
            {
                if (rolls[frame][0] == 10)
                    Console.Write("│X│ ");
                else if (rolls[frame][0] + rolls[frame][1] == 10)
                    Console.Write($"│{rolls[frame][0]}│/│ ");
                else
                    Console.Write($"│{(rolls[frame][0] == -1 ? "-" : rolls[frame][0].ToString())}│{(rolls[frame][1] == -1 ? "-" : rolls[frame][1].ToString())}│ ");
            }
            else
            {
                if (rolls[frame][0] == 10)
                    Console.Write($"│X│{(rolls[frame][1] == 10 ? "X" : rolls[frame][1].ToString())}│{(rolls[frame][2] == 10 ? "X" : rolls[frame][2].ToString())}│");
                else if (rolls[frame][0] + rolls[frame][1] == 10)
                    Console.Write($"│{rolls[frame][0]}│/│{rolls[frame][2]}│");
                else
                    Console.Write($"│{rolls[frame][0]}│{rolls[frame][1]}│{(rolls[frame][2] == -1 ? "-" : rolls[frame][2].ToString())}│");
            }
        }
        Console.WriteLine();
        Console.WriteLine("│ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┴─┤");
        Console.Write("│");
        for (int frame = 0; frame < 10; frame++)
        {
            Console.Write(frame < 9 ? $" {frameScores[frame],3} │" : $" {frameScores[frame],4}  │");
        }
        Console.WriteLine();
        Console.WriteLine("└─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴───────┘");
    }
}