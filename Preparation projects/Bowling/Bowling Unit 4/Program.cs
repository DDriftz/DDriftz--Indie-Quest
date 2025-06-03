using System;
using System.Collections.Generic;
using System.Threading;

class BowlingGame
{
    static Random rand = new Random();
    static int[][]? rolls;
    static List<bool>[]? pinsStanding;

    static void Main()
    {
        rolls = new int[10][];
        pinsStanding = new List<bool>[10];
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
            Console.WriteLine("  1   2   3   4   5   6   7");
            int path = GetPlayerPath();
            rolls[frame][0] = KnockPinOnPath(path - 1);
            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
            DisplayScoreSheet(rolls, frame);
            DisplayPins(pinsStanding[frame]);

            if (frame < 9 && rolls[frame][0] != 10)
            {
                Console.WriteLine("  1   2   3   4   5   6   7");
                path = GetPlayerPath();
                rolls[frame][1] = KnockPinOnPath(path - 1);
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
                    Console.WriteLine("  1   2   3   4   5   6   7");
                    path = GetPlayerPath();
                    rolls[frame][1] = KnockPinOnPath(path - 1);
                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
                    Console.WriteLine($"Roll 2: {(rolls[frame][0] + rolls[frame][1] == 10 ? "/" : rolls[frame][1] == 10 ? "X" : rolls[frame][1].ToString())}");
                    DisplayScoreSheet(rolls, frame);
                    DisplayPins(pinsStanding[frame]);

                    Console.WriteLine("  1   2   3   4   5   6   7");
                    path = GetPlayerPath();
                    rolls[frame][2] = KnockPinOnPath(path - 1);
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

    static int GetPlayerPath()
    {
        int path;
        do
        {
            Console.Write("Choose a path (1-7): ");
        } while (!int.TryParse(Console.ReadLine(), out path) || path < 1 || path > 7);
        return path;
    }

    static int KnockPinOnPath(int path)
    {
        int currentFrame = 0;
        if (rolls == null)
            return 0;
        for (int i = 0; i < 10; i++)
        {
            if (rolls[i][0] == -1) break;
            if (i < 9 && rolls[i][1] != -1) { currentFrame = i + 1; continue; }
            if (i == 9 && rolls[i][2] != -1) { currentFrame = i + 1; continue; }
            currentFrame = i;
            break;
        }

        int knockedPinsCount = 0;
        int[] pinOrder = { 6, 3, 1, 0, 2, 4, 7, 5, 8, 9 };

        if (path < 0 || path > 6) return 0;

        for (int i = 0; i < pinOrder.Length; i++)
        {
            int pinIndex = pinOrder[i];
            if (pinsStanding != null && pinsStanding[currentFrame] != null && pinsStanding[currentFrame][pinIndex])
            {
                pinsStanding[currentFrame][pinIndex] = false;
                knockedPinsCount = 1;

                int random = rand.Next(0, 100);
                int newPath = path;
                if (random < 33) newPath = path - 1;
                else if (random < 66) newPath = path + 1;
                knockedPinsCount += KnockPinOnPath(newPath);

                random = rand.Next(0, 100);
                newPath = path;
                if (random < 33) newPath = path - 1;
                else if (random < 66) newPath = path + 1;
                knockedPinsCount += KnockPinOnPath(newPath);

                Console.Clear();
                Console.WriteLine($"FRAME {currentFrame + 1}");
                DisplayPins(pinsStanding[currentFrame]);
                Thread.Sleep(500);
                break;
            }
        }

        return knockedPinsCount;
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
        Console.WriteLine((pins[6] ? "O" : " ") + "   " + (pins[7] ? "O" : " ") + "   " + (pins[8] ? "O" : " ") + "   " + (pins[9] ? "O" : " "));
        Console.WriteLine("  " + (pins[3] ? "O" : " ") + "   " + (pins[4] ? "O" : " ") + "   " + (pins[5] ? "O" : " "));
        Console.WriteLine("    " + (pins[1] ? "O" : " ") + "   " + (pins[2] ? "O" : " "));
        Console.WriteLine("      " + (pins[0] ? "O" : " "));
        Console.WriteLine("—");
    }
}