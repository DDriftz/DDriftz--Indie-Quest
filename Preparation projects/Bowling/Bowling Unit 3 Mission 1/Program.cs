using System;

class BowlingGame
{
    // Static Random instance to be used throughout the game simulation
    static Random rand = new Random();

    static void Main()
    {
        // Initialize jagged array for rolls. rolls[frame][roll_index]
        // Max 3 rolls for the 10th frame. Use -1 to indicate a roll hasn't happened.
        int[][] rolls = new int[10][];
        for (int i = 0; i < 10; i++)
        {
            rolls[i] = new int[3]; // 0: roll1, 1: roll2, 2: roll3 (for 10th frame)
            for (int j = 0; j < 3; j++)
            {
                rolls[i][j] = -1; // Initialize all rolls to "not played"
            }
        }

        // Arrays to store points gained per frame and cumulative scores
        int[] pointsGained = new int[10];
        int[] frameScores = new int[10];

        // Game loop for 10 frames
        for (int frame = 0; frame < 10; frame++)
        {
            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            DisplayScoreSheet(rolls, frameScores, frame); // Display sheet before first roll
            Console.WriteLine("\nPress enter to roll!");
            Console.ReadLine();

            // Roll 1
            rolls[frame][0] = rand.Next(0, 11); // 0-10 pins
            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
            DisplayScoreSheet(rolls, frameScores, frame); // Update and display after roll 1

            // --- Handle Roll 2 (and Roll 3 for 10th frame) ---
            if (frame < 9) // Frames 1-9
            {
                if (rolls[frame][0] < 10) // Not a strike, so roll 2
                {
                    Console.WriteLine("\nPress enter to roll!");
                    Console.ReadLine();
                    int remainingPins = 10 - rolls[frame][0];
                    rolls[frame][1] = rand.Next(0, remainingPins + 1);
                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {rolls[frame][0]}");
                    Console.WriteLine($"Roll 2: {(rolls[frame][0] + rolls[frame][1] == 10 ? "/" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString()))}");
                    DisplayScoreSheet(rolls, frameScores, frame);
                }
            }
            else // 10th Frame
            {
                // Roll 2 (guaranteed in 10th frame unless it's for a strike's bonus rolls)
                if (rolls[frame][0] == 10) // Strike on first roll of 10th
                {
                    Console.WriteLine("\nPress enter for bonus roll 1!");
                    Console.ReadLine();
                    rolls[frame][1] = rand.Next(0, 11); // Full set of pins
                }
                else // Not a strike on first roll, normal second roll
                {
                    Console.WriteLine("\nPress enter to roll!");
                    Console.ReadLine();
                    int remainingPins = 10 - rolls[frame][0];
                    rolls[frame][1] = rand.Next(0, remainingPins + 1);
                }
                Console.Clear();
                Console.WriteLine($"FRAME {frame + 1}");
                Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
                string roll2Display = "";
                if (rolls[frame][0] == 10) roll2Display = rolls[frame][1] == 10 ? "X" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString());
                else roll2Display = (rolls[frame][0] + rolls[frame][1] == 10) ? "/" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString());
                Console.WriteLine($"Roll 2: {roll2Display}");
                DisplayScoreSheet(rolls, frameScores, frame);

                // Roll 3 (if strike on roll 1, or spare after roll 2, or strike on roll 2 after strike on roll 1)
                if (rolls[frame][0] == 10 || (rolls[frame][0] + rolls[frame][1] == 10 && rolls[frame][1] != -1) )
                {
                    Console.WriteLine("\nPress enter for bonus roll!");
                    Console.ReadLine();
                    if (rolls[frame][0] == 10 && rolls[frame][1] == 10) // Two strikes
                    {
                        rolls[frame][2] = rand.Next(0, 11); // Full set for 3rd strike
                    }
                    else if (rolls[frame][0] == 10) // Strike on 1st, open/miss on 2nd
                    {
                         rolls[frame][2] = rand.Next(0, 10 - rolls[frame][1] + 1);
                    }
                    else // Spare after roll 1 and 2 (e.g. 3, /)
                    {
                        rolls[frame][2] = rand.Next(0, 11); // Full set for spare bonus
                    }
                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
                    Console.WriteLine($"Roll 2: {roll2Display}");
                    Console.WriteLine($"Roll 3: {(rolls[frame][2] == 10 ? "X" : (rolls[frame][2] == 0 ? "-" : rolls[frame][2].ToString()))}");
                    DisplayScoreSheet(rolls, frameScores, frame);
                }
            }

            // End of frame summary
            Console.WriteLine("\nEnd of frame!");
            int pinsThisFrame = rolls[frame][0];
            if (rolls[frame][1] != -1 && rolls[frame][0] != 10) pinsThisFrame = rolls[frame][0] + rolls[frame][1]; // For open/spare
            else if (rolls[frame][0] == 10) pinsThisFrame = 10; // Strike ensures 10 for the "base" of the frame before bonus rolls in 10th

            if(frame == 9) { // Special calculation for knocked pins in 10th
                pinsThisFrame = rolls[frame][0];
                if(rolls[frame][1] != -1) pinsThisFrame += rolls[frame][1];
                if(rolls[frame][2] != -1) pinsThisFrame += rolls[frame][2];
            } else { // Frames 1-9
                 if(rolls[frame][0] == 10) pinsThisFrame = 10;
                 else if (rolls[frame][1] != -1) pinsThisFrame = rolls[frame][0] + rolls[frame][1];
                 // if only roll1 is played and it's not a strike, pinsThisFrame is just roll1
            }


            Console.WriteLine($"Knocked pins this frame (raw): {pinsThisFrame}");
            if (frame < 9)
            {
                Console.WriteLine("Press enter for next frame...");
                Console.ReadLine();
            }
        }

        Console.Clear();
        Console.WriteLine("FINAL SCORE:");
        DisplayScoreSheet(rolls, frameScores, 9); // Show final state up to frame 9 (10th frame)
        Console.WriteLine("\nGame Over. Press enter to exit.");
        Console.ReadLine();
    }

    /// <summary>
    /// Calculates scores and then displays the scoresheet.
    /// </summary>
    /// <param name="rolls">Jagged array of rolls. rolls[frame][roll_idx], -1 if not played.</param>
    /// <param name="frameScores">Array to store cumulative scores, will be updated.</param>
    /// <param name="currentFrameInProgress">The current frame being played (0-9).</param>
    static void DisplayScoreSheet(int[][] rolls, int[] frameScores, int currentFrameInProgress)
    {
        int[] pointsGained = new int[10];
        CalculateScores(rolls, pointsGained, frameScores, currentFrameInProgress);

        // Scoresheet display logic from Bowling 2, adapted for clarity
        Console.WriteLine("\n┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬───────┐");
        Console.Write("│"); // Start of rolls line
        for (int f = 0; f < 10; f++)
        {
            string r1 = rolls[f][0] == -1 ? " " : (rolls[f][0] == 10 ? "X" : rolls[f][0].ToString());
            string r2 = " ";
            string r3 = " "; // Only for 10th frame

            if (f < 9) // Frames 1-9
            {
                if (rolls[f][0] == 10) // Strike
                {
                    r1 = " "; r2 = "X"; // Centered X as per doc
                }
                else if (rolls[f][1] != -1) // Second roll played
                {
                    r2 = (rolls[f][0] + rolls[f][1] == 10) ? "/" : rolls[f][1].ToString();
                }
                 if(rolls[f][0] == 0 && r1 == "0") r1 = "-";
                 if(rolls[f][1] == 0 && r2 == "0") r2 = "-";

                Console.Write($" {r1,1}│{r2,1} │");
            }
            else // 10th Frame
            {
                // Roll 1
                r1 = rolls[f][0] == -1 ? " " : (rolls[f][0] == 10 ? "X" : (rolls[f][0] == 0 ? "-" : rolls[f][0].ToString()));
                // Roll 2
                if (rolls[f][1] != -1) {
                    if (rolls[f][0] == 10) // Strike on 1st
                        r2 = rolls[f][1] == 10 ? "X" : (rolls[f][1] == 0 ? "-" : rolls[f][1].ToString());
                    else // Not a strike on 1st
                        r2 = (rolls[f][0] + rolls[f][1] == 10) ? "/" : (rolls[f][1] == 0 ? "-" : rolls[f][1].ToString());
                }
                // Roll 3
                if (rolls[f][2] != -1) {
                     r3 = rolls[f][2] == 10 ? "X" : (rolls[f][2] == 0 ? "-" : rolls[f][2].ToString());
                }
                Console.Write($" {r1,1}│{r2,1}│{r3,1} │");
            }
        }
        Console.WriteLine(); // End of rolls line

        // Separator and Score line
        Console.Write("│"); // Start of score line
        for (int f = 0; f < 10; f++)
        {
            // Condition to print blank based on Bowling 3.docx example:
            // Score is blank if pointsGained[f] is 0 AND it's not simply an open frame of 0,
            // meaning a bonus is pending or frame is truly incomplete for scoring.
            // More simply: if frameScores[f] is 0 and it's not the first frame that just started with 0.
            bool shouldBeBlank = frameScores[f] == 0 && !(f == 0 && rolls[f][0] != -1 && pointsGained[f] == 0 && (rolls[f][0] + (rolls[f][1] == -1 ? 0 : rolls[f][1])) == 0);
            if (f > currentFrameInProgress || (f == currentFrameInProgress && rolls[f][0] == -1) ) shouldBeBlank = true;
            if (pointsGained[f] == 0 && frameScores[f] == (f > 0 ? frameScores[f-1] : 0) && ( (rolls[f][0] == 10 || (rolls[f][0] + rolls[f][1] == 10 && rolls[f][1] != -1)) && f < 9) ) {
                // If it's a strike/spare and pointsGained is 0 (bonus pending), make blank.
                 if (f < currentFrameInProgress || (f == currentFrameInProgress && ( (rolls[f][0]==10 && GetNextTwoRolls(rolls, f, currentFrameInProgress) == -1) || ((rolls[f][0]+rolls[f][1]==10 && rolls[f][1]!=-1) && GetNextRoll(rolls,f,currentFrameInProgress)==-1  )   ) ) )
                    shouldBeBlank = true;
            }


            if (shouldBeBlank && f <= currentFrameInProgress && !(rolls[f][0] == 0 && rolls[f][1] == 0 && pointsGained[f] == 0 && frameScores[f] == (f > 0 ? frameScores[f-1] : 0) )) {
                 // This logic is tricky. For now, if frameScore is 0 and it's not an actual 0 score, print blank.
                 // If points for frame f are not determined (e.g. bonus pending) frameScores[f] might be same as frameScores[f-1] or 0.
                 // The example output is the guide: blanks for pending scores.
                 // If frameScores[f] is 0, and it's not because of an actual 0 score (e.g., 0,0 pins), assume it's pending.
                 // A simpler way from user's Program.cs: (frameScores[f] == 0 && (f > currentFrameInProgress || (f == currentFrameInProgress && rolls[f][0] == -1)))
                 // This simpler way doesn't quite cover pending bonuses correctly.
                 // Let's use: if pointsGained[f] is 0, AND it was a strike/spare, then blank until bonus resolved.
                 bool isStrike = rolls[f][0] == 10;
                 bool isSpare = rolls[f][1] != -1 && (rolls[f][0] + rolls[f][1] == 10);
                 if (pointsGained[f] == 0 && (isStrike || isSpare) && f < 9 && frameScores[f] == (f > 0 ? frameScores[f-1] : 0) ) {
                     Console.Write(f < 9 ? "     │" : "       │");
                 } else if (frameScores[f] == 0 && f > (f>0 ? frameScores[f-1] : -1) && !(rolls[f][0]==0 && rolls[f][1]==0) && f <= currentFrameInProgress && rolls[f][0] != -1) {
                     // If score is 0 but not because of 0,0 pins, and play has reached here.
                      Console.Write(f < 9 ? "     │" : "       │");
                 }
                 else if (frameScores[f] == 0 && f > currentFrameInProgress && rolls[f][0] == -1){
                      Console.Write(f < 9 ? "     │" : "       │");
                 }
                 else {
                    Console.Write(f < 9 ? $" {frameScores[f],3} │" : $" {frameScores[f],4}  │");
                 }

            } else { // Default: Print score if calculated
                 Console.Write(f < 9 ? $" {frameScores[f],3} │" : $" {frameScores[f],4}  │");
            }
        }
        Console.WriteLine(); // End of score line
        Console.WriteLine("└─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴───────┘");
    }


    /// <summary>
    /// Calculates points for each frame and cumulative scores up to currentFramePlayed.
    /// Scores are only calculated if all necessary rolls (including bonuses) are available.
    /// </summary>
    static void CalculateScores(int[][] rolls, int[] pointsGained, int[] frameScores, int currentFramePlayed)
    {
        Array.Clear(pointsGained, 0, pointsGained.Length);
        Array.Clear(frameScores, 0, frameScores.Length);
        int cumulativeScore = 0;

        for (int f = 0; f <= currentFramePlayed; f++)
        {
            if (rolls[f][0] == -1) continue; // Frame not started

            int currentFrameRawPoints = 0; // Pins in this frame only
            bool bonusFinalized = true; // Assume bonus can be calculated

            if (f < 9) // Frames 1-9
            {
                if (rolls[f][0] == 10) // Strike
                {
                    currentFrameRawPoints = 10;
                    int bonus = GetNextTwoRolls(rolls, f, currentFramePlayed);
                    if (bonus != -1) currentFrameRawPoints += bonus;
                    else bonusFinalized = false;
                }
                else if (rolls[f][1] != -1 && (rolls[f][0] + rolls[f][1] == 10)) // Spare
                {
                    currentFrameRawPoints = 10;
                    int bonus = GetNextRoll(rolls, f, currentFramePlayed);
                    if (bonus != -1) currentFrameRawPoints += bonus;
                    else bonusFinalized = false;
                }
                else if (rolls[f][1] != -1) // Open frame, both rolls played
                {
                    currentFrameRawPoints = rolls[f][0] + rolls[f][1];
                }
                else // Only first roll played, not a strike (frame incomplete for scoring)
                {
                    bonusFinalized = false; // Not yet a full open frame or strike/spare
                }
            }
            else // 10th Frame
            {
                currentFrameRawPoints = rolls[f][0];
                if (rolls[f][1] != -1) currentFrameRawPoints += rolls[f][1];
                if (rolls[f][2] != -1) currentFrameRawPoints += rolls[f][2];
                // No bonus from future frames for 10th frame itself
            }

            if (bonusFinalized && rolls[f][0] != -1) // If score for this frame is final
            {
                pointsGained[f] = currentFrameRawPoints;
            }
            else
            {
                pointsGained[f] = 0; // Mark as pending if bonus not finalized or frame incomplete for scoring
            }
            
            // Accumulate scores only if pointsGained is determined
            if(pointsGained[f] > 0 || (bonusFinalized && rolls[f][0] != -1 && (f < 9 ? rolls[f][1] != -1 || rolls[f][0] == 10 : true) ) ) {
                 cumulativeScore += pointsGained[f];
                 frameScores[f] = cumulativeScore;
            } else if (f > 0 && bonusFinalized == false) { // Carry previous score if current is pending
                 frameScores[f] = frameScores[f-1];
            } else if (f==0 && bonusFinalized == false){
                 frameScores[f] = 0;
            }
        }
    }

    /// <summary>
    /// Gets pins from the next single roll if available.
    /// Returns -1 if the roll is not available yet.
    /// </summary>
    static int GetNextRoll(int[][] rolls, int currentFrame, int maxFramePlayed)
    {
        if (currentFrame + 1 > 9) return 0; // No next frame after 9 for this kind of bonus
        if (currentFrame + 1 <= maxFramePlayed && rolls[currentFrame + 1][0] != -1)
        {
            return rolls[currentFrame + 1][0];
        }
        return -1; // Next roll not available
    }

    /// <summary>
    /// Gets pins from the next two rolls if available.
    /// Returns -1 if rolls are not available.
    /// </summary>
    static int GetNextTwoRolls(int[][] rolls, int currentFrame, int maxFramePlayed)
    {
        int bonus = 0;
        // Strike in frame 9 (rolls from 10th frame)
        if (currentFrame == 8)
        {
            if (maxFramePlayed >= 9 && rolls[9][0] != -1 && rolls[9][1] != -1)
            {
                return rolls[9][0] + rolls[9][1];
            }
            return -1; // Not all bonus rolls for 10th frame are available
        }

        // Strike in frames 1-8
        if (currentFrame < 8)
        {
            // Check first bonus roll (next frame, roll 1)
            if (maxFramePlayed >= currentFrame + 1 && rolls[currentFrame + 1][0] != -1)
            {
                bonus += rolls[currentFrame + 1][0];
                // If next frame was a strike
                if (rolls[currentFrame + 1][0] == 10)
                {
                    // Need roll from frame after that (currentFrame + 2, roll 1)
                    if (maxFramePlayed >= currentFrame + 2 && rolls[currentFrame + 2][0] != -1)
                    {
                        bonus += rolls[currentFrame + 2][0];
                        return bonus;
                    }
                    return -1; // Second bonus roll (from f+2) not available
                }
                // If next frame was not a strike, need its second roll
                else
                {
                    if (rolls[currentFrame + 1][1] != -1) // Check if second roll of next frame exists
                    {
                        bonus += rolls[currentFrame + 1][1];
                        return bonus;
                    }
                    return -1; // Second bonus roll (from f+1, r2) not available
                }
            }
            return -1; // First bonus roll not available
        }
        return -1; // Should not happen if called for frame < 9
    }
}
