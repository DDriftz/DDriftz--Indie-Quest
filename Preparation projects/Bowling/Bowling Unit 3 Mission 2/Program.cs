// Mission 2: BOSS LEVEL - Objective: Bowling pins
using System;
using System.Collections.Generic; // Required for List<T>
using System.Linq; // Required for Enumerable.Repeat and Count()

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

        // Array to store cumulative scores
        int[] frameScores = new int[10];

        // Game loop for 10 frames
        for (int frame = 0; frame < 10; frame++)
        {
            // Initialize pin state for the current frame (all 10 pins standing)
            List<bool> currentPinsState = new List<bool>(Enumerable.Repeat(true, 10));

            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            DisplayScoreSheet(rolls, frameScores, frame);
            DisplayPins(currentPinsState); // Display pins before first roll
            Console.WriteLine("\nPress enter to roll!");
            Console.ReadLine();

            // Roll 1
            rolls[frame][0] = SimulateRollAndUpdatePins(currentPinsState, 10); // Max 10 pins can be hit
            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
            DisplayScoreSheet(rolls, frameScores, frame);
            DisplayPins(currentPinsState); // Display pins after roll 1

            // --- Handle Roll 2 (and Roll 3 for 10th frame) ---
            if (frame < 9) // Frames 1-9
            {
                if (rolls[frame][0] < 10) // Not a strike, so roll 2
                {
                    Console.WriteLine("\nPress enter to roll!");
                    Console.ReadLine();
                    int pinsStandingForRoll2 = currentPinsState.Count(p => p);
                    rolls[frame][1] = SimulateRollAndUpdatePins(currentPinsState, pinsStandingForRoll2);
                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {rolls[frame][0]}");
                    Console.WriteLine($"Roll 2: {(rolls[frame][0] + rolls[frame][1] == 10 && rolls[frame][1] != -1 ? "/" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString()))}");
                    DisplayScoreSheet(rolls, frameScores, frame);
                    DisplayPins(currentPinsState); // Display pins after roll 2
                }
            }
            else // 10th Frame
            {
                // Roll 2
                if (rolls[frame][0] == 10) // Strike on first roll of 10th, reset pins for bonus
                {
                    currentPinsState = new List<bool>(Enumerable.Repeat(true, 10));
                    Console.WriteLine("\nBonus Roll! Press enter to roll.");
                    Console.ReadLine();
                    rolls[frame][1] = SimulateRollAndUpdatePins(currentPinsState, 10);
                }
                else // Not a strike on first roll, normal second roll (pins are as left by roll 1)
                {
                    Console.WriteLine("\nPress enter to roll!");
                    Console.ReadLine();
                    int pinsStandingForRoll2 = currentPinsState.Count(p => p);
                    rolls[frame][1] = SimulateRollAndUpdatePins(currentPinsState, pinsStandingForRoll2);
                }
                Console.Clear();
                Console.WriteLine($"FRAME {frame + 1}");
                Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
                string roll2Display = "";
                if (rolls[frame][0] == 10) // Original R1 was strike
                    roll2Display = rolls[frame][1] == 10 ? "X" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString());
                else // Original R1 was not strike
                    roll2Display = (rolls[frame][0] + rolls[frame][1] == 10 && rolls[frame][1]!=-1) ? "/" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString());
                Console.WriteLine($"Roll 2: {roll2Display}");
                DisplayScoreSheet(rolls, frameScores, frame);
                DisplayPins(currentPinsState); // Display pins after roll 2

                // Roll 3 (if eligible for a third roll in 10th frame)
                bool gotStrikeInRoll1 = rolls[frame][0] == 10;
                bool gotStrikeInRoll2AfterStrikeInRoll1 = gotStrikeInRoll1 && rolls[frame][1] == 10;
                bool gotSpare = !gotStrikeInRoll1 && rolls[frame][1]!=-1 && (rolls[frame][0] + rolls[frame][1] == 10);

                if (gotStrikeInRoll1 || gotSpare) // Eligible for 3rd roll if strike on 1st or spare on 1st two
                {
                    if (gotStrikeInRoll1 && rolls[frame][1] == 10) // Strike on R1, Strike on R2 -> reset for R3
                    {
                        currentPinsState = new List<bool>(Enumerable.Repeat(true, 10));
                    }
                    else if (gotSpare) // Spare on R1+R2 -> reset for R3
                    {
                         currentPinsState = new List<bool>(Enumerable.Repeat(true, 10));
                    }
                    // If Strike on R1, then non-Strike on R2, pins for R3 are as left by R2. No reset here.

                    Console.WriteLine("\nBonus Roll! Press enter to roll.");
                    Console.ReadLine();
                    int pinsStandingForRoll3 = currentPinsState.Count(p => p);
                    rolls[frame][2] = SimulateRollAndUpdatePins(currentPinsState, (gotStrikeInRoll1 && rolls[frame][1] == 10) || gotSpare ? 10 : pinsStandingForRoll3);

                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : rolls[frame][0].ToString())}");
                    Console.WriteLine($"Roll 2: {roll2Display}");
                    Console.WriteLine($"Roll 3: {(rolls[frame][2] == 10 ? "X" : (rolls[frame][2] == 0 ? "-" : rolls[frame][2].ToString()))}");
                    DisplayScoreSheet(rolls, frameScores, frame);
                    DisplayPins(currentPinsState); // Display pins after roll 3
                }
            }

            Console.WriteLine("\nEnd of frame!");
            if (frame < 9)
            {
                Console.WriteLine("Press enter for next frame...");
                Console.ReadLine();
            }
        }

        Console.Clear();
        Console.WriteLine("FINAL SCORE:");
        DisplayScoreSheet(rolls, frameScores, 9); // Show final state up to frame 9 (10th frame)
        // Display final pin state if desired, though game is over.
        // DisplayPins(currentPinsState); // If you want to show pins at the very end
        Console.WriteLine("\nGame Over. Press enter to exit.");
        Console.ReadLine();
    }

    /// <summary>
    /// Simulates a roll, updates the pin status, and returns the number of pins knocked.
    /// </summary>
    /// <param name="currentPinsStatus">List of booleans representing pin states (true=standing). This list IS modified.</param>
    /// <param name="maxPinsAvailableToHit">The number of pins currently standing that the ball is rolled against.</param>
    /// <returns>The number of pins knocked down in this roll.</returns>
    static int SimulateRollAndUpdatePins(List<bool> currentPinsStatus, int maxPinsAvailableToHit)
    {
        if (maxPinsAvailableToHit == 0) return 0; // No pins to hit

        // Determine how many pins the player attempts to knock in this roll
        // This can be from 0 up to the number of pins they are rolling against (e.g., 10 for a first ball, or fewer for a second ball)
        int pinsPlayerHits = rand.Next(0, maxPinsAvailableToHit + 1);
        
        int pinsKnockedThisAction = 0;
        List<int> standingPinIndices = new List<int>();
        for (int i = 0; i < currentPinsStatus.Count; ++i)
        {
            if (currentPinsStatus[i])
            {
                standingPinIndices.Add(i);
            }
        }

        // Shuffle the list of standing pin indices to randomize which ones get hit
        for (int i = 0; i < standingPinIndices.Count - 1; i++)
        {
            int j = rand.Next(i, standingPinIndices.Count);
            int temp = standingPinIndices[i];
            standingPinIndices[i] = standingPinIndices[j];
            standingPinIndices[j] = temp;
        }

        // Knock down pins from the (now randomized) standing list, up to pinsPlayerHits
        for (int i = 0; i < Math.Min(pinsPlayerHits, standingPinIndices.Count); i++)
        {
            currentPinsStatus[standingPinIndices[i]] = false; // Mark pin as knocked
            pinsKnockedThisAction++;
        }
        return pinsKnockedThisAction;
    }


    /// <summary>
    /// Displays the current state of the 10 bowling pins.
    /// Pin numbers on diagram:
    /// 7  8  9  10 (back row)
    ///   4  5  6
    ///     2  3
    ///       1 (front)
    /// List<bool> pins maps to: Pin 1 -> pins[0], Pin 2 -> pins[1], ..., Pin 10 -> pins[9]
    /// </summary>
    /// <param name="pins">A list of 10 booleans indicating if each pin is standing (true) or knocked (false).</param>
    static void DisplayPins(List<bool> pins)
    {
        Console.WriteLine("\nCurrent pins:");
        // Row 4 (Pins 7, 8, 9, 10 corresponding to indices 6, 7, 8, 9)
        Console.WriteLine($"{(pins[6] ? "O" : " ")}   {(pins[7] ? "O" : " ")}   {(pins[8] ? "O" : " ")}   {(pins[9] ? "O" : " ")}");
        // Row 3 (Pins 4, 5, 6 corresponding to indices 3, 4, 5)
        Console.WriteLine($"  {(pins[3] ? "O" : " ")}   {(pins[4] ? "O" : " ")}   {(pins[5] ? "O" : " ")}");
        // Row 2 (Pins 2, 3 corresponding to indices 1, 2)
        Console.WriteLine($"    {(pins[1] ? "O" : " ")}   {(pins[2] ? "O" : " ")}");
        // Row 1 (Pin 1 corresponding to index 0)
        Console.WriteLine($"      {(pins[0] ? "O" : " ")}");
        Console.WriteLine("--------------------"); // Separator
    }

    /// <summary>
    /// Calculates scores based on rolls and displays the scoresheet.
    /// </summary>
    static void DisplayScoreSheet(int[][] rolls, int[] frameScores, int currentFrameInProgress)
    {
        int[] pointsGained = new int[10]; // To hold points for current calculation
        CalculateScores(rolls, pointsGained, frameScores, currentFrameInProgress); // Calculate scores up to current frame

        Console.WriteLine("\n┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬───────┐");
        Console.Write("│"); // Start of rolls line
        for (int f = 0; f < 10; f++)
        {
            string r1Display = " ";
            string r2Display = " ";
            string r3Display = " "; // Only for 10th frame

            if (rolls[f][0] != -1) // If first roll happened
            {
                r1Display = rolls[f][0] == 10 ? "X" : (rolls[f][0] == 0 ? "-" : rolls[f][0].ToString());
            }

            if (f < 9) // Frames 1-9
            {
                if (rolls[f][0] == 10) // Strike
                {
                    r1Display = " "; r2Display = "X"; // Centered X
                }
                else if (rolls[f][1] != -1) // Second roll played
                {
                    r2Display = (rolls[f][0] + rolls[f][1] == 10) ? "/" : (rolls[f][1] == 0 ? "-" : rolls[f][1].ToString());
                }
                Console.Write($" {r1Display,1}│{r2Display,1} │");
            }
            else // 10th Frame
            {
                if (rolls[f][1] != -1) { // Second roll happened
                    if (rolls[f][0] == 10) // R1 was Strike
                        r2Display = rolls[f][1] == 10 ? "X" : (rolls[f][1] == 0 ? "-" : rolls[f][1].ToString());
                    else // R1 not strike
                        r2Display = (rolls[f][0] + rolls[f][1] == 10) ? "/" : (rolls[f][1] == 0 ? "-" : rolls[f][1].ToString());
                }
                if (rolls[f][2] != -1) { // Third roll happened
                     r3Display = rolls[f][2] == 10 ? "X" : (rolls[f][2] == 0 ? "-" : rolls[f][2].ToString());
                }
                Console.Write($" {r1Display,1}│{r2Display,1}│{r3Display,1} │");
            }
        }
        Console.WriteLine(); // End of rolls line

        Console.Write("│"); // Start of score line
        for (int f = 0; f < 10; f++)
        {
            bool scoreIsCalculatedForFrame = (pointsGained[f] > 0 || (f < 9 && rolls[f][0] != -1 && (rolls[f][0]==10 || rolls[f][1]!=-1)) || (f==9 && rolls[f][0]!=-1 && rolls[f][1]!=-1 && (rolls[f][0]+rolls[f][1] >=10 ? rolls[f][2]!=-1 : true ) ));
            bool isStrikePending = f < 9 && rolls[f][0] == 10 && GetNextTwoRolls(rolls, f, currentFrameInProgress) == -1;
            bool isSparePending = f < 9 && rolls[f][0] != -1 && rolls[f][1] != -1 && (rolls[f][0] + rolls[f][1] == 10) && GetNextRoll(rolls, f, currentFrameInProgress) == -1;
            
            if (f > currentFrameInProgress || (f == currentFrameInProgress && rolls[f][0] == -1) || (!scoreIsCalculatedForFrame && frameScores[f] == 0 && f > 0 && frameScores[f-1] == 0 && rolls[f-1][0] == -1 )) // Frame not reached or not started
            {
                 Console.Write(f < 9 ? "     │" : "       │");
            }
            else if(isStrikePending || isSparePending) // Bonus is pending for a strike/spare
            {
                 Console.Write(f < 9 ? "     │" : "       │");
            }
            else if (frameScores[f] == 0 && pointsGained[f] == 0 && rolls[f][0] != -1 && !(rolls[f][0]==0 && rolls[f][1]==0)) { // Likely pending or truly 0 but not yet finalized
                 bool isOpenFrameJustStarted = f < 9 && rolls[f][0] != -1 && rolls[f][0] < 10 && rolls[f][1] == -1;
                 if (isOpenFrameJustStarted && currentFrameInProgress == f) Console.Write(f < 9 ? "     │" : "       │"); // Open frame, first ball rolled, score not yet final
                 else if (frameScores[f] == (f>0 ? frameScores[f-1]:0) && (isStrikePending || isSparePending) ) Console.Write(f < 9 ? "     │" : "       │");
                 else Console.Write(f < 9 ? $" {frameScores[f],3} │" : $" {frameScores[f],4}  │"); // Print 0 if it's a genuine 0
            }
            else
            {
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
    static void CalculateScores(int[][] rolls, int[] pointsGainedOutput, int[] frameScoresOutput, int currentFramePlayed)
    {
        // Clear previous calculations for these output arrays
        Array.Clear(pointsGainedOutput, 0, pointsGainedOutput.Length);
        Array.Clear(frameScoresOutput, 0, frameScoresOutput.Length);
        int cumulativeScore = 0;

        for (int f = 0; f <= currentFramePlayed; f++)
        {
            if (rolls[f][0] == -1 && f <= currentFramePlayed) { // Frame not started or only partially played where score cannot yet be determined
                 if (f > 0) frameScoresOutput[f] = frameScoresOutput[f-1]; // Carry over score if frame not scorable
                 else frameScoresOutput[f] = 0;
                 pointsGainedOutput[f] = 0;
                 continue;
            }


            int currentFramePoints = 0;
            bool bonusFinalizedOrNotApplicable = true;

            if (f < 9) // Frames 1-9
            {
                if (rolls[f][0] == 10) // Strike
                {
                    currentFramePoints = 10;
                    int bonus = GetNextTwoRolls(rolls, f, currentFramePlayed);
                    if (bonus != -1) currentFramePoints += bonus;
                    else bonusFinalizedOrNotApplicable = false;
                }
                else if (rolls[f][1] != -1 && (rolls[f][0] + rolls[f][1] == 10)) // Spare
                {
                    currentFramePoints = 10;
                    int bonus = GetNextRoll(rolls, f, currentFramePlayed);
                    if (bonus != -1) currentFramePoints += bonus;
                    else bonusFinalizedOrNotApplicable = false;
                }
                else if (rolls[f][1] != -1) // Open frame, both rolls played
                {
                    currentFramePoints = rolls[f][0] + rolls[f][1];
                }
                else // Only first roll played, not a strike (frame incomplete for full scoring)
                {
                    bonusFinalizedOrNotApplicable = false;
                    currentFramePoints = rolls[f][0]; // Store partial, but pointsGained will be 0
                }
            }
            else // 10th Frame - Score is just sum of pins for this frame.
            {
                if (rolls[f][0] != -1) currentFramePoints += rolls[f][0];
                if (rolls[f][1] != -1) currentFramePoints += rolls[f][1];
                if (rolls[f][2] != -1) currentFramePoints += rolls[f][2];
            }

            if (bonusFinalizedOrNotApplicable && rolls[f][0]!=-1)
            {
                // For frames 1-9, if it's an open frame but roll 2 hasn't happened, points aren't "gained" yet for display.
                if (f < 9 && rolls[f][0] < 10 && rolls[f][1] == -1 && currentFramePlayed == f) {
                     pointsGainedOutput[f] = 0; // Not yet a full open frame score
                     if (f > 0) frameScoresOutput[f] = frameScoresOutput[f-1]; else frameScoresOutput[f] = 0;
                } else {
                    pointsGainedOutput[f] = currentFramePoints;
                    cumulativeScore += pointsGainedOutput[f];
                    frameScoresOutput[f] = cumulativeScore;
                }
            }
            else // Bonus not finalized or frame incomplete, carry score, pointsGained is 0 for this frame.
            {
                pointsGainedOutput[f] = 0;
                if (f > 0) frameScoresOutput[f] = frameScoresOutput[f-1];
                else frameScoresOutput[f] = 0;
            }
        }
    }

    /// <summary>
    /// Gets pins from the next single roll if available FOR BONUS CALCULATION.
    /// Returns -1 if the roll is not available yet.
    /// </summary>
    static int GetNextRoll(int[][] rolls, int currentFrame, int maxFramePlayedActually)
    {
        if (currentFrame + 1 > 9) return 0; // Should not happen for spare bonus calculation
        if (currentFrame + 1 <= maxFramePlayedActually && rolls[currentFrame + 1][0] != -1)
        {
            return rolls[currentFrame + 1][0];
        }
        return -1; // Next roll not available
    }

    /// <summary>
    /// Gets pins from the next two rolls if available FOR BONUS CALCULATION.
    /// Returns -1 if rolls are not available.
    /// </summary>
    static int GetNextTwoRolls(int[][] rolls, int currentFrame, int maxFramePlayedActually)
    {
        int bonus = 0;
        if (currentFrame >= 9) return 0; // No bonus from future frames for 10th, and strike in 9th is special.

        // Strike in frame 9 (bonus comes from first two rolls of 10th frame)
        if (currentFrame == 8)
        {
            if (maxFramePlayedActually >= 9 && rolls[9][0] != -1 && rolls[9][1] != -1)
            {
                return rolls[9][0] + rolls[9][1];
            }
            return -1; // Not all bonus rolls from 10th frame are available yet
        }

        // Strike in frames 1-8
        // Check first bonus roll (next frame, roll 1)
        if (maxFramePlayedActually >= currentFrame + 1 && rolls[currentFrame + 1][0] != -1)
        {
            bonus += rolls[currentFrame + 1][0];
            // If next frame was also a strike (e.g. X | X | ?)
            if (rolls[currentFrame + 1][0] == 10)
            {
                if (currentFrame + 2 > 9) return -1; // Cannot get second bonus roll if currentFrame+1 was frame 9
                // Need roll from frame after that (currentFrame + 2, roll 1)
                if (maxFramePlayedActually >= currentFrame + 2 && rolls[currentFrame + 2][0] != -1)
                {
                    bonus += rolls[currentFrame + 2][0];
                    return bonus;
                }
                return -1; // Second bonus roll (from f+2, r1) not available
            }
            // If next frame was not a strike (e.g. X | 7 / | ?) or (X | 7 2 | ?)
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
}
