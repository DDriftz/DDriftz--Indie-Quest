using System;

class BowlingGame
{
    // Static Random instance to be used throughout the game simulation
    static Random rand = new Random();

    static void Main()
    {
        Console.WriteLine("SIMULATING 10 BOWLING FRAMES ...");
        // Simulate all rolls for a 10-frame game
        int[][] rolls = SimulateGame();
        Console.WriteLine("DONE!");
        Console.WriteLine("\nResults:\n");

        // Arrays to store points gained per frame and cumulative scores
        int[] pointsGained = new int[10];
        int[] frameScores = new int[10];

        // Calculate the scores based on the simulated rolls
        CalculateScores(rolls, pointsGained, frameScores);

        // Display the detailed game report (rolls, pins, points, scores per frame)
        DisplayGame(rolls, pointsGained, frameScores);

        // Display the final scoresheet using box-drawing characters
        Console.WriteLine("\nFinal Score Sheet:\n");
        DisplayScoreSheet(rolls, frameScores);
    }

    /// <summary>
    /// Simulates all rolls for a 10-frame bowling game.
    /// </summary>
    /// <returns>A jagged array where rolls[i] contains the pins knocked down in frame i.</returns>
    static int[][] SimulateGame()
    {
        // Initialize a jagged array for 10 frames. Each frame can have a different number of rolls.
        int[][] rolls = new int[10][]; 

        for (int frame = 0; frame < 10; frame++)
        {
            if (frame < 9) // Frames 1 through 9
            {
                rolls[frame] = new int[2]; // Each of the first 9 frames can have up to 2 rolls initially
                rolls[frame][0] = rand.Next(0, 11); // Roll 1: 0-10 pins

                if (rolls[frame][0] == 10) // Strike
                {
                    rolls[frame][1] = -1; // Mark second roll as not applicable (-1)
                }
                else // Not a strike, so proceed to roll 2
                {
                    int remainingPins = 10 - rolls[frame][0];
                    rolls[frame][1] = rand.Next(0, remainingPins + 1); // Roll 2: 0 to remaining pins
                }
            }
            else // 10th frame (special rules)
            {
                rolls[frame] = new int[3]; // 10th frame can have up to 3 rolls
                rolls[frame][0] = rand.Next(0, 11); // Roll 1

                if (rolls[frame][0] == 10) // Strike on first roll of 10th frame
                {
                    rolls[frame][1] = rand.Next(0, 11); // Roll 2 (bonus)
                    // Roll 3 (bonus): if roll 2 was a strike, can knock 10 again, else remaining pins
                    rolls[frame][2] = rolls[frame][1] == 10 ? rand.Next(0, 11) : rand.Next(0, 10 - rolls[frame][1] + 1);
                }
                else // Not a strike on the first roll of 10th frame
                {
                    int remainingPins = 10 - rolls[frame][0];
                    rolls[frame][1] = rand.Next(0, remainingPins + 1); // Roll 2

                    if (rolls[frame][0] + rolls[frame][1] == 10) // Spare in the 10th frame
                    {
                        rolls[frame][2] = rand.Next(0, 11); // Roll 3 (bonus)
                    }
                    else // Open frame in the 10th (no bonus roll 3)
                    {
                        rolls[frame][2] = -1; // Mark third roll as not applicable
                    }
                }
            }
        }
        return rolls;
    }

    /// <summary>
    /// Calculates the points gained for each frame and the cumulative score.
    /// </summary>
    /// <param name="rolls">The jagged array of rolls.</param>
    /// <param name="pointsGained">Array to store points gained in each frame.</param>
    /// <param name="frameScores">Array to store cumulative score at each frame.</param>
    static void CalculateScores(int[][] rolls, int[] pointsGained, int[] frameScores)
    {
        int totalScore = 0;
        for (int frame = 0; frame < 10; frame++)
        {
            int currentFramePoints = 0;

            // --- Calculate points for the current frame ---
            if (frame < 9) // Frames 1-9
            {
                // Check for Strike
                if (rolls[frame][0] == 10)
                {
                    currentFramePoints = 10 + GetNextTwoRolls(rolls, frame);
                }
                // Check for Spare
                else if (rolls[frame][0] + rolls[frame][1] == 10)
                {
                    currentFramePoints = 10 + GetNextRoll(rolls, frame);
                }
                // Open frame
                else
                {
                    currentFramePoints = rolls[frame][0] + rolls[frame][1];
                }
            }
            else // 10th Frame - scoring is just the sum of pins knocked down in this frame
            {
                currentFramePoints = rolls[frame][0];
                if(rolls[frame][1] != -1) currentFramePoints += rolls[frame][1];
                if(rolls[frame][2] != -1) currentFramePoints += rolls[frame][2];
            }

            pointsGained[frame] = currentFramePoints;
            totalScore += currentFramePoints;
            frameScores[frame] = totalScore;
        }
    }

    /// <summary>
    /// Gets the score of the next single roll after the current frame.
    /// Used for calculating spare bonuses.
    /// </summary>
    static int GetNextRoll(int[][] rolls, int currentFrame)
    {
        // Ensure we don't go out of bounds (shouldn't happen if called for frames < 9)
        if (currentFrame + 1 >= 10) return 0; // Should ideally be handled by caller for frame 9
        
        return rolls[currentFrame + 1][0]; // Pins from the first roll of the next frame
    }

    /// <summary>
    /// Gets the score of the next two rolls after the current frame.
    /// Used for calculating strike bonuses.
    /// </summary>
    static int GetNextTwoRolls(int[][] rolls, int currentFrame)
    {
        int bonus = 0;

        // Case 1: Strike in frame 9 (next two rolls are from frame 10)
        if (currentFrame == 8) 
        {
            bonus += rolls[9][0]; // First roll of 10th frame
            if (rolls[9][1] != -1) bonus += rolls[9][1]; // Second roll of 10th frame
            // No need to check rolls[9][2] for a strike in frame 8's bonus calculation,
            // as the "next two rolls" are strictly the first two of the 10th frame.
            // However, if the first roll of 10th is a strike, then rolls[9][1] is its own bonus roll.
            return bonus;
        }
        // Case 2: Strike in frames 1-8
        else if (currentFrame < 8)
        {
            // Bonus from the next frame (frame + 1)
            bonus += rolls[currentFrame + 1][0]; // First roll of next frame

            if (rolls[currentFrame + 1][0] == 10) // If next frame is also a strike
            {
                // Need to look at the frame after that (frame + 2) for the second bonus roll
                bonus += rolls[currentFrame + 2][0]; 
            }
            else // Next frame is not a strike
            {
                 if (rolls[currentFrame + 1][1] != -1) bonus += rolls[currentFrame + 1][1]; // Second roll of next frame
            }
        }
        return bonus;
    }

    /// <summary>
    /// Displays the detailed results of the game, frame by frame.
    /// </summary>
    static void DisplayGame(int[][] rolls, int[] pointsGained, int[] frameScores)
    {
        for (int frame = 0; frame < 10; frame++)
        {
            Console.WriteLine($"FRAME {frame + 1}");
            int knockedPinsThisFrame = 0;

            // Display Roll 1
            if (rolls[frame][0] == 10)
            {
                Console.WriteLine("Roll 1: X");
                knockedPinsThisFrame = 10;
            }
            else
            {
                Console.WriteLine($"Roll 1: {(rolls[frame][0] == 0 ? "-" : rolls[frame][0].ToString())}");
                knockedPinsThisFrame += rolls[frame][0];
            }

            // Display Roll 2 (if applicable for the frame type)
            if (rolls[frame][1] != -1) // If there was a second roll
            {
                if (rolls[frame][0] != 10) // Only if not a strike on first roll
                {
                    if (rolls[frame][0] + rolls[frame][1] == 10)
                    {
                        Console.WriteLine("Roll 2: /");
                        knockedPinsThisFrame = 10; // Total for spare is 10
                    }
                    else
                    {
                        Console.WriteLine($"Roll 2: {(rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString())}");
                        knockedPinsThisFrame += rolls[frame][1];
                    }
                }
                else if (frame == 9) // Strike in 10th frame, roll 2 is a bonus
                {
                     Console.WriteLine($"Roll 2: {(rolls[frame][1] == 10 ? "X" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString()))}");
                     knockedPinsThisFrame += rolls[frame][1];
                }
            }
            
            // Display Roll 3 (only for 10th frame if applicable)
            if (frame == 9 && rolls[frame][2] != -1)
            {
                 Console.WriteLine($"Roll 3: {(rolls[frame][2] == 10 ? "X" : (rolls[frame][2] == 0 ? "-" : rolls[frame][2].ToString()))}");
                 knockedPinsThisFrame += rolls[frame][2];
            }
            
            Console.WriteLine($"Knocked pins: {knockedPinsThisFrame}");
            Console.WriteLine($"Points gained: {pointsGained[frame]}");
            Console.WriteLine($"Score: {frameScores[frame]}\n");
        }
    }

    /// <summary>
    /// Displays the final scoresheet using box-drawing characters.
    /// </summary>
    static void DisplayScoreSheet(int[][] rolls, int[] frameScores)
    {
        // Top border
        Console.WriteLine("┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬───────┐");

        // Rolls line
        Console.Write("│");
        for (int frame = 0; frame < 10; frame++)
        {
            string r1 = " ";
            string r2 = " ";
            string r3 = ""; // Only for 10th frame

            // Frame 1-9 roll display logic
            if (frame < 9)
            {
                if (rolls[frame][0] == 10) // Strike
                {
                    r1 = " "; r2 = "X"; // Centered X
                }
                else
                {
                    r1 = rolls[frame][0] == 0 ? "-" : rolls[frame][0].ToString();
                    if (rolls[frame][0] + rolls[frame][1] == 10) // Spare
                    {
                        r2 = "/";
                    }
                    else // Open frame
                    {
                        r2 = rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString();
                    }
                }
                Console.Write($" {r1,1}│{r2,1} │");
            }
            // 10th frame roll display logic
            else
            {
                // Roll 1
                r1 = rolls[frame][0] == 10 ? "X" : (rolls[frame][0] == 0 ? "-" : rolls[frame][0].ToString());
                // Roll 2
                if(rolls[frame][0] == 10) // Strike on first
                {
                    r2 = rolls[frame][1] == 10 ? "X" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString());
                }
                else if (rolls[frame][0] + rolls[frame][1] == 10) // Spare
                {
                     r2 = "/";
                }
                else // Open
                {
                    r2 = rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString();
                }
                // Roll 3 (if applicable)
                if(rolls[frame][2] != -1)
                {
                    r3 = rolls[frame][2] == 10 ? "X" : (rolls[frame][2] == 0 ? "-" : rolls[frame][2].ToString());
                } else {
                    r3 = " "; // Ensure 3rd slot is blank if not used
                }
                Console.Write($" {r1,1}│{r2,1}│{r3,1} │"); // Special formatting for 10th frame
            }
        }
        Console.WriteLine();

        // Separator line / Score line
        Console.Write("│");
        for (int frame = 0; frame < 10; frame++)
        {
            if (frame < 9)
            {
                Console.Write($" {frameScores[frame],-3} │");
            }
            else // 10th frame score takes more space
            {
                Console.Write($"  {frameScores[frame],-3}  │");
            }
        }
        Console.WriteLine();

        // Bottom border
        Console.WriteLine("└─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴───────┘");
    }
}
