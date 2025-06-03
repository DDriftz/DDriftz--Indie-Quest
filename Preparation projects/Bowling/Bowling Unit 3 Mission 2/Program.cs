// Mission 2: BOSS LEVEL - Objective: Bowling pins
class BowlingGameMission2
{
    static Random rand = new Random();

    static void Main()
    {
        int[][] rolls = new int[10][];
        List<bool>[] pinsStanding = new List<bool>[10];
        for (int frame = 0; frame < 10; frame++)
        {
            rolls[frame] = new int[3];
            for (int i = 0; i < 3; i++) rolls[frame][i] = -1;
            pinsStanding[frame] = new List<bool>(new bool[10]);
            for (int i = 0; i < 10; i++) pinsStanding[frame][i] = true;
        }

        for (int frame = 0; frame < 10; frame++)
        {
            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            DisplayScoreSheet(rolls, frame);
            DisplayPins(pinsStanding[frame]);
            Console.WriteLine("Press enter to roll!");
            Console.ReadLine();

            rolls[frame][0] = RollFirst(pinsStanding[frame]);
            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
            DisplayScoreSheet(rolls, frame);
            DisplayPins(pinsStanding[frame]);

            if (frame < 9 && rolls[frame][0] != 10)
            {
                Console.WriteLine("Press enter to roll!");
                Console.ReadLine();
                rolls[frame][1] = RollSecond(rolls[frame][0], pinsStanding[frame]);
                Console.Clear();
                Console.WriteLine($"FRAME {frame + 1}");
                Console.WriteLine($"Roll 1: {rolls[frame][0]}");
                Console.WriteLine($"Roll 2: {((rolls[frame][0] + rolls[frame][1] == 10) ? "/" : (rolls[frame][1] == -1 ? "-" : rolls[frame][1].ToString()))}");
                DisplayScoreSheet(rolls, frame);
                DisplayPins(pinsStanding[frame]);
            }
            else if (frame == 9)
            {
                if (rolls[frame][0] == 10 || rolls[frame][0] + rolls[frame][1] == 10)
                {
                    Console.WriteLine("Press enter to roll!");
                    Console.ReadLine();
                    rolls[frame][1] = rolls[frame][0] == 10 ? RollFirst(pinsStanding[frame]) : RollSecond(rolls[frame][0], pinsStanding[frame]);
                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
                    Console.WriteLine($"Roll 2: {(rolls[frame][0] + rolls[frame][1] == 10 ? "/" : rolls[frame][1] == 10 ? "X" : rolls[frame][1].ToString())}");
                    DisplayScoreSheet(rolls, frame);
                    DisplayPins(pinsStanding[frame]);

                    Console.WriteLine("Press enter to roll!");
                    Console.ReadLine();
                    rolls[frame][2] = rolls[frame][0] == 10 && rolls[frame][1] == 10 ? RollFirst(pinsStanding[frame]) : RollSecond(rolls[frame][0] + rolls[frame][1], pinsStanding[frame]);
                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
                    Console.WriteLine($"Roll 2: {(rolls[frame][0] + rolls[frame][1] == 10 ? "/" : rolls[frame][1] == 10 ? "X" : rolls[frame][1].ToString())}");
                    Console.WriteLine($"Roll 3: {rolls[frame][2].ToString()}");
                    DisplayScoreSheet(rolls, frame);
                    DisplayPins(pinsStanding[frame]);
                }
            }

            Console.WriteLine("End of frame!");
            int framePins = rolls[frame][0] == 10 ? 10 : rolls[frame][0] + (rolls[frame][1] == -1 ? 0 : rolls[frame][1]);
            if (frame == 9 && rolls[frame][2] != -1) framePins += rolls[frame][2];
            Console.WriteLine($"Knocked pins: {framePins}");
            Console.ReadLine();
        }

        Console.Clear();
        Console.WriteLine("FINAL SCORE:");
        DisplayScoreSheet(rolls, 9);
    }

    static int RollFirst(List<bool> pins)
    {
        int pinsKnocked = rand.Next(0, 11);
        for (int i = 0; i < 10 && pinsKnocked > 0; i++)
        {
            if (pins[i] && rand.Next(0, 2) == 1)
            {
                pins[i] = false;
                pinsKnocked--;
            }
        }
        return 10 - pins.FindAll(p => p).Count;
    }

    static int RollSecond(int firstRoll, List<bool> pins)
    {
        int remaining = 10 - firstRoll;
        if (remaining == 0) return -1;
        int pinsKnocked = rand.Next(0, remaining + 1);
        for (int i = 0; i < 10 && pinsKnocked > 0; i++)
        {
            if (pins[i] && rand.Next(0, 2) == 1)
            {
                pins[i] = false;
                pinsKnocked--;
            }
        }
        return 10 - firstRoll - pins.FindAll(p => p).Count;
    }

    static void DisplayScoreSheet(int[][] rolls, int currentFrame)
    {
        int[] pointsGained = new int[10];
        int[] frameScores = new int[10];
        CalculateScores(rolls, pointsGained, frameScores, currentFrame);

        Console.WriteLine("┌─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┬─┐");
        Console.Write("│ ");
        for (int frame = 0; frame < 10; frame++)
        {
            if (frame > currentFrame) 
            {
                Console.Write(frame < 9 ? "│ │ " : "│ │ │");
                continue;
            }
            if (frame < 9)
            {
                if (rolls[frame][0] == 10)
                    Console.Write("│X│ ");
                else if (rolls[frame][1] != -1 && rolls[frame][0] + rolls[frame][1] == 10)
                    Console.Write($"│{rolls[frame][0]}│/│ ");
                else
                    Console.Write($"│{(rolls[frame][0] == -1 ? " " : rolls[frame][0].ToString())}│{(rolls[frame][1] == -1 ? " " : rolls[frame][1].ToString())}│ ");
            }
            else
            {
                if (rolls[frame][0] == 10)
                    Console.Write($"│X│{(rolls[frame][1] == -1 ? " " : (rolls[frame][1] == 10 ? "X" : rolls[frame][1].ToString()))}│{(rolls[frame][2] == -1 ? " " : rolls[frame][2].ToString())}│");
                else if (rolls[frame][1] != -1 && rolls[frame][0] + rolls[frame][1] == 10)
                    Console.Write($"│{rolls[frame][0]}│/│{(rolls[frame][2] == -1 ? " " : rolls[frame][2].ToString())}│");
                else
                    Console.Write($"│{(rolls[frame][0] == -1 ? " " : rolls[frame][0].ToString())}│{(rolls[frame][1] == -1 ? " " : rolls[frame][1].ToString())}│{(rolls[frame][2] == -1 ? " " : rolls[frame][2].ToString())}│");
            }
        }
        Console.WriteLine();
        Console.WriteLine("│ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┤ └─┴─┴─┤");
        Console.Write("│");
        for (int frame = 0; frame < 10; frame++)
        {
            if (frameScores[frame] == 0 && (frame > currentFrame || (frame == currentFrame && rolls[frame][0] == -1)))
                Console.Write(frame < 9 ? "     │" : "       │");
            else
                Console.Write(frame < 9 ? $" {frameScores[frame],3} │" : $" {frameScores[frame],4}  │");
        }
        Console.WriteLine();
        Console.WriteLine("└─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴───────┘");
    }

    static void CalculateScores(int[][] rolls, int[] pointsGained, int[] frameScores, int currentFrame)
    {
        int totalScore = 0;
        for (int frame = 0; frame <= currentFrame; frame++)
        {
            int framePins = rolls[frame][0] == 10 ? 10 : rolls[frame][0] + (rolls[frame][1] == -1 ? 0 : rolls[frame][1]);
            if (frame < 9)
            {
                if (rolls[frame][0] == 10)
                {
                    int nextTwo = GetNextTwoRolls(rolls, frame, currentFrame);
                    if (nextTwo != -1) pointsGained[frame] = 10 + nextTwo;
                    else pointsGained[frame] = 0;
                }
                else if (rolls[frame][1] != -1 && rolls[frame][0] + rolls[frame][1] == 10)
                {
                    int nextRoll = GetNextRoll(rolls, frame, currentFrame);
                    if (nextRoll != -1) pointsGained[frame] = 10 + nextRoll;
                    else pointsGained[frame] = 0;
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
            if (pointsGained[frame] != 0)
            {
                totalScore += pointsGained[frame];
                frameScores[frame] = totalScore;
            }
        }
    }

    static int GetNextRoll(int[][] rolls, int currentFrame, int maxFrame)
    {
        if (currentFrame + 1 > maxFrame || rolls[currentFrame + 1][0] == -1) return -1;
        if (currentFrame + 1 < 9 && rolls[currentFrame + 1][0] == 10) return 10;
        return rolls[currentFrame + 1][0] + (rolls[currentFrame + 1][1] == -1 ? 0 : rolls[currentFrame + 1][1]);
    }

    static int GetNextTwoRolls(int[][] rolls, int currentFrame, int maxFrame)
    {
        int sum = 0;
        if (currentFrame + 1 > maxFrame || rolls[currentFrame + 1][0] == -1) return -1;
        if (currentFrame + 1 < 9 && rolls[currentFrame + 1][0] == 10)
        {
            sum += 10;
            if (currentFrame + 2 > maxFrame || rolls[currentFrame + 2][0] == -1) return -1;
            sum += rolls[currentFrame + 2][0];
        }
        else
        {
            sum += rolls[currentFrame + 1][0];
            if (rolls[currentFrame + 1][1] == -1) return -1;
            sum += rolls[currentFrame + 1][1];
        }
        return sum;
    }

    static void DisplayPins(List<bool> pins)
    {
        Console.WriteLine("Current pins:");
        Console.WriteLine(pins[6] ? "O" : " " + "   " + (pins[7] ? "O" : " ") + "   " + (pins[8] ? "O" : " ") + "   " + (pins[9] ? "O" : " "));
        Console.WriteLine("  " + (pins[3] ? "O" : " ") + "   " + (pins[4] ? "O" : " ") + "   " + (pins[5] ? "O" : " "));
        Console.WriteLine("    " + (pins[1] ? "O" : " ") + "   " + (pins[2] ? "O" : " "));
        Console.WriteLine("      " + (pins[0] ? "O" : " "));
        Console.WriteLine("—");
    }
}