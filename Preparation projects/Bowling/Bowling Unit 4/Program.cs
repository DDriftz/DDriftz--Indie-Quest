using System;
using System.Collections.Generic; // Required for List<T>
using System.Linq; // Required for Enumerable.Repeat and Count()
using System.Threading; // Required for Thread.Sleep()

class BowlingGame
{
    // Static Random instance to be used throughout the game simulation
    static Random rand = new Random();
    const int MAX_CHAIN_REACTION_DEPTH = 4; // Limits the depth of recursive pin knocking

    // Defines the sequence of pins targeted by each of the 7 paths, from front to back.
    // Pin indices are 0-9 (Pin 1 is index 0, Pin 10 is index 9).
    static readonly List<List<int>> PinSequencesPerPath = new List<List<int>> {
        new List<int> { 6 },           // Path 0 (User chooses 1) -> Targets Pin 7 area
        new List<int> { 3, 7 },        // Path 1 (User chooses 2) -> Targets Pin 4 area, then Pin 8
        new List<int> { 1, 4, 8 },     // Path 2 (User chooses 3) -> Targets Pin 2 area, then Pin 5, then Pin 9
        new List<int> { 0 },           // Path 3 (User chooses 4) -> Targets Pin 1 (Headpin) area
        new List<int> { 2, 5, 9 },     // Path 4 (User chooses 5) -> Targets Pin 3 area, then Pin 6, then Pin 10
        new List<int> { 5, 8 },        // Path 5 (User chooses 6) -> Targets Pin 6 area, then Pin 9
        new List<int> { 9 }            // Path 6 (User chooses 7) -> Targets Pin 10 area
    };

    // Variables to hold game state, passed around for redraws during chain reaction
    static int[][]? currentRollsData;
    static int[]? currentFrameScoresData;
    static int currentFrameBeingPlayedData;


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

        // Update static references for potential redraws during recursion
        currentRollsData = rolls;
        currentFrameScoresData = frameScores;


        // Game loop for 10 frames
        for (int frame = 0; frame < 10; frame++)
        {
            currentFrameBeingPlayedData = frame; // Update static current frame
            // Initialize pin state for the current frame (all 10 pins standing)
            List<bool> currentPinsState = new List<bool>(Enumerable.Repeat(true, 10));

            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            DisplayScoreSheet(rolls, frameScores, frame);
            DisplayPins(currentPinsState);
            int chosenPath = GetPlayerPath();

            // Roll 1
            rolls[frame][0] = KnockPinOnPath(currentPinsState, chosenPath - 1);
            Console.Clear();
            Console.WriteLine($"FRAME {frame + 1}");
            Console.WriteLine($"Roll 1: {(rolls[frame][0] == 10 ? "X" : (rolls[frame][0] == 0 ? "-" : rolls[frame][0].ToString()))}");
            DisplayScoreSheet(rolls, frameScores, frame);
            DisplayPins(currentPinsState);

            // --- Handle Roll 2 (and Roll 3 for 10th frame) ---
            if (frame < 9) // Frames 1-9
            {
                if (rolls[frame][0] < 10) // Not a strike, so roll 2
                {
                    Console.WriteLine("\nRoll 2");
                    DisplayPins(currentPinsState); // Show pins before roll 2
                    chosenPath = GetPlayerPath();
                    rolls[frame][1] = KnockPinOnPath(currentPinsState, chosenPath - 1);
                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {(rolls[frame][0] == 0 ? "-" : rolls[frame][0].ToString())}");
                    Console.WriteLine($"Roll 2: {(rolls[frame][0] + rolls[frame][1] == 10 && rolls[frame][1] != -1 ? "/" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString()))}");
                    DisplayScoreSheet(rolls, frameScores, frame);
                    DisplayPins(currentPinsState);
                }
            }
            else // 10th Frame
            {
                // Roll 2
                bool r1WasStrike = rolls[frame][0] == 10;
                if (r1WasStrike) // Strike on first roll of 10th, reset pins for bonus
                {
                    currentPinsState = new List<bool>(Enumerable.Repeat(true, 10));
                    Console.WriteLine("\nBonus Roll 1 on fresh pins!");
                } else {
                    Console.WriteLine("\nRoll 2");
                }
                DisplayPins(currentPinsState);
                chosenPath = GetPlayerPath();
                rolls[frame][1] = KnockPinOnPath(currentPinsState, chosenPath - 1);
                
                Console.Clear();
                Console.WriteLine($"FRAME {frame + 1}");
                Console.WriteLine($"Roll 1: {(r1WasStrike ? "X" : (rolls[frame][0] == 0 ? "-" : rolls[frame][0].ToString()))}");
                string roll2Display;
                if (r1WasStrike) 
                    roll2Display = rolls[frame][1] == 10 ? "X" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString());
                else 
                    roll2Display = (rolls[frame][0] + rolls[frame][1] == 10 && rolls[frame][1]!=-1) ? "/" : (rolls[frame][1] == 0 ? "-" : rolls[frame][1].ToString());
                Console.WriteLine($"Roll 2: {roll2Display}");
                DisplayScoreSheet(rolls, frameScores, frame);
                DisplayPins(currentPinsState);

                // Roll 3 (if eligible)
                bool r2WasStrikeAfterR1Strike = r1WasStrike && rolls[frame][1] == 10;
                bool gotSpare = !r1WasStrike && rolls[frame][1]!=-1 && (rolls[frame][0] + rolls[frame][1] == 10);

                if (r1WasStrike || gotSpare) 
                {
                    if (r2WasStrikeAfterR1Strike || gotSpare) 
                    {
                        currentPinsState = new List<bool>(Enumerable.Repeat(true, 10));
                        Console.WriteLine(r2WasStrikeAfterR1Strike ? "\nBonus Roll 2 on fresh pins (after two strikes)!" : "\nBonus Roll after spare on fresh pins!");
                    } else { // R1 was strike, R2 was not. Pins for R3 are as left by R2.
                         Console.WriteLine("\nBonus Roll 2 (after strike, roll 2 was open)!");
                    }
                    DisplayPins(currentPinsState);
                    chosenPath = GetPlayerPath();
                    rolls[frame][2] = KnockPinOnPath(currentPinsState, chosenPath - 1);

                    Console.Clear();
                    Console.WriteLine($"FRAME {frame + 1}");
                    Console.WriteLine($"Roll 1: {(r1WasStrike ? "X" : (rolls[frame][0] == 0 ? "-" : rolls[frame][0].ToString()))}");
                    Console.WriteLine($"Roll 2: {roll2Display}");
                    Console.WriteLine($"Roll 3: {(rolls[frame][2] == 10 ? "X" : (rolls[frame][2] == 0 ? "-" : rolls[frame][2].ToString()))}");
                    DisplayScoreSheet(rolls, frameScores, frame);
                    DisplayPins(currentPinsState);
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
        DisplayScoreSheet(rolls, frameScores, 9);
        Console.WriteLine("\nGame Over. Press enter to exit.");
        Console.ReadLine();
    }
    
    /// <summary>
    /// Gets the player's chosen path (1-7).
    /// </summary>
    static int GetPlayerPath()
    {
        int pathChoice;
        Console.WriteLine("\nPaths:  1  2  3  4  5  6  7");
        Console.WriteLine("        ↑  ↑  ↑  ↑  ↑  ↑  ↑");
        Console.WriteLine("Pins: (7)(4)(2)(1)(3)(6)(10)"); // Approx pin under path start
        do
        {
            Console.Write("Choose a path (1-7) to aim for: ");
        } while (!int.TryParse(Console.ReadLine(), out pathChoice) || pathChoice < 1 || pathChoice > 7);
        return pathChoice;
    }

    /// <summary>
    /// Initiates knocking pins down a specific path.
    /// Finds the first standing pin in the path and starts the recursive knocking process.
    /// </summary>
    static int KnockPinOnPath(List<bool> pinsState, int pathIndex)
    {
        if (pathIndex < 0 || pathIndex >= PinSequencesPerPath.Count) return 0; // Invalid path

        int initialPinToHit = -1;
        // Find the first standing pin in the sequence for the chosen pathIndex
        foreach (int pinIdxCandidate in PinSequencesPerPath[pathIndex])
        {
            if (pinIdxCandidate >= 0 && pinIdxCandidate < 10 && pinsState[pinIdxCandidate])
            {
                initialPinToHit = pinIdxCandidate;
                break; // Target the first standing pin in this path's sequence
            }
        }

        if (initialPinToHit != -1)
        {
            // Start the recursive process. The 'pathIndex' passed here is the path of the ball for the initial hit.
            return KnockPinRecursive(pinsState, initialPinToHit, pathIndex, 0);
        }
        return 0; // No standing pin found in the direct path
    }

    /// <summary>
    /// Recursively knocks a targeted pin and simulates chain reactions.
    /// </summary>
    /// <param name="pinsState">The current state of all 10 pins.</param>
    /// <param name="pinIndexToTryHit">The specific pin index (0-9) being targeted by this impact.</param>
    /// <param name="incomingObjectPathIndex">The path index (0-6) of the object (ball/pin) that is causing this impact.</param>
    /// <param name="depth">Current depth of recursion to prevent infinite loops.</param>
    /// <returns>Total number of pins knocked by this impact and its subsequent chain reactions.</returns>
    static int KnockPinRecursive(List<bool> pinsState, int pinIndexToTryHit, int incomingObjectPathIndex, int depth)
    {
        // Check if the targeted pin is valid and standing
        if (pinIndexToTryHit < 0 || pinIndexToTryHit >= 10 || !pinsState[pinIndexToTryHit])
        {
            return 0; // Pin already down, out of bounds, or invalid
        }

        pinsState[pinIndexToTryHit] = false; // Knock this pin down
        int knockedInThisChain = 1;

        // --- Part 3 Improvement: Redraw pins and pause after each pin falls ---
        Console.Clear(); // Clear console for fresh display
        Console.WriteLine($"FRAME {currentFrameBeingPlayedData + 1} - Pinfall Animation");
        if(currentRollsData != null && currentFrameScoresData != null) {
             DisplayScoreSheet(currentRollsData, currentFrameScoresData, currentFrameBeingPlayedData); // Show current scores
        }
        DisplayPins(pinsState); // Show updated pin layout
        Console.WriteLine($"... Pin {(pinIndexToTryHit == 0 ? 1 : pinIndexToTryHit == 1 ? 2 : pinIndexToTryHit == 2 ? 3 : pinIndexToTryHit == 3 ? 4 : pinIndexToTryHit == 4 ? 5 : pinIndexToTryHit == 5 ? 6 : pinIndexToTryHit == 6 ? 7 : pinIndexToTryHit == 7 ? 8 : pinIndexToTryHit == 8 ? 9 : 10)} knocked! ..."); // User feedback. Note: Pin numbers are 1-10.
        Thread.Sleep(400); // Pause to see the pin fall
        // --- End of Part 3 Improvement ---

        if (depth < MAX_CHAIN_REACTION_DEPTH)
        {
            // Simulate two pieces of debris/ball continuing (repeat twice)
            for (int i = 0; i < 2; i++)
            {
                int randomPercentage = rand.Next(100);
                int deviatedPathIndex = incomingObjectPathIndex; // Start with the path of the object that hit this pin

                if (randomPercentage < 33) // 33% chance to go left
                {
                    deviatedPathIndex = Math.Max(0, incomingObjectPathIndex - 1);
                }
                else if (randomPercentage < 66) // 33% chance to go right (total 66%)
                {
                    deviatedPathIndex = Math.Min(PinSequencesPerPath.Count - 1, incomingObjectPathIndex + 1);
                }
                // Else (34% chance), it continues straight along incomingObjectPathIndex (deviatedPathIndex remains unchanged)

                // Find the next standing pin in the (potentially) deviated path for the chain reaction
                int nextPinInChainToTarget = -1;
                if (deviatedPathIndex >= 0 && deviatedPathIndex < PinSequencesPerPath.Count)
                {
                    foreach (int pinCandInChain in PinSequencesPerPath[deviatedPathIndex])
                    {
                        // Important: The chain reaction should ideally not re-hit the pin that JUST fell (pinIndexToTryHit)
                        // and it must be a standing pin.
                        if (pinCandInChain >= 0 && pinCandInChain < 10 && pinsState[pinCandInChain])
                        {
                            nextPinInChainToTarget = pinCandInChain;
                            break; // Target this next standing pin
                        }
                    }
                }

                if (nextPinInChainToTarget != -1)
                {
                    // Recursive call: the "incomingObjectPathIndex" for the next call is the "deviatedPathIndex" we just calculated.
                    knockedInThisChain += KnockPinRecursive(pinsState, nextPinInChainToTarget, deviatedPathIndex, depth + 1);
                }
            }
        }
        return knockedInThisChain;
    }

    /// <summary>
    /// Displays the current state of the 10 bowling pins.
    /// Pin numbers on diagram: 7 8 9 10 (back) / 4 5 6 / 2 3 / 1 (front)
    /// List<bool> pins maps: Pin 1->pins[0], Pin 2->pins[1], ..., Pin 10->pins[9]
    /// </summary>
    static void DisplayPins(List<bool> pins)
    {
        Console.WriteLine("\nCurrent pins:");
        // Row 4 (Indices: 6,7,8,9 for Pins: 7,8,9,10)
        Console.WriteLine($"{(pins[6] ? "O" : " ")}   {(pins[7] ? "O" : " ")}   {(pins[8] ? "O" : " ")}   {(pins[9] ? "O" : " ")}");
        // Row 3 (Indices: 3,4,5 for Pins: 4,5,6)
        Console.WriteLine($"  {(pins[3] ? "O" : " ")}   {(pins[4] ? "O" : " ")}   {(pins[5] ? "O" : " ")}");
        // Row 2 (Indices: 1,2 for Pins: 2,3)
        Console.WriteLine($"    {(pins[1] ? "O" : " ")}   {(pins[2] ? "O" : " ")}");
        // Row 1 (Index: 0 for Pin: 1)
        Console.WriteLine($"      {(pins[0] ? "O" : " ")}");
        Console.WriteLine("--------------------");
    }

    /// <summary>
    /// Calculates scores based on rolls and updates frameScores.
    /// Then displays the scoresheet.
    /// </summary>
    static void DisplayScoreSheet(int[][] rolls, int[] frameScores, int currentFrameInProgress)
    {
        int[] pointsGained = new int[10]; 
        CalculateScores(rolls, pointsGained, frameScores, currentFrameInProgress); 

        Console.WriteLine("\nScoresheet:");
        Console.WriteLine("┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┬───────┐");
        Console.Write("│"); 
        for (int f = 0; f < 10; f++)
        {
            string r1Display = " ";
            string r2Display = " ";
            string r3Display = " "; 

            if (rolls[f][0] != -1) 
            {
                r1Display = rolls[f][0] == 10 ? "X" : (rolls[f][0] == 0 ? "-" : rolls[f][0].ToString());
            }

            if (f < 9) 
            {
                if (rolls[f][0] == 10) 
                {
                    r1Display = " "; r2Display = "X"; 
                }
                else if (rolls[f][1] != -1) 
                {
                    r2Display = (rolls[f][0] + rolls[f][1] == 10) ? "/" : (rolls[f][1] == 0 ? "-" : rolls[f][1].ToString());
                }
                Console.Write($" {r1Display,1}│{r2Display,1} │");
            }
            else 
            {
                if (rolls[f][1] != -1) { 
                    if (rolls[f][0] == 10) 
                        r2Display = rolls[f][1] == 10 ? "X" : (rolls[f][1] == 0 ? "-" : rolls[f][1].ToString());
                    else 
                        r2Display = (rolls[f][0] + rolls[f][1] == 10 && rolls[f][1] != -1) ? "/" : (rolls[f][1] == 0 ? "-" : rolls[f][1].ToString());
                }
                if (rolls[f][2] != -1) { 
                     r3Display = rolls[f][2] == 10 ? "X" : (rolls[f][2] == 0 ? "-" : rolls[f][2].ToString());
                }
                Console.Write($" {r1Display,1}│{r2Display,1}│{r3Display,1} │");
            }
        }
        Console.WriteLine(); 

        Console.Write("│"); 
        for (int f = 0; f < 10; f++)
        {
            bool scoreIsReadyForDisplay = (pointsGained[f] > 0 || (f<9 && rolls[f][0] != -1 && (rolls[f][0]==10 || rolls[f][1]!=-1)) || (f==9 && rolls[f][0]!=-1 && rolls[f][1]!=-1 && ( (rolls[f][0]+rolls[f][1]>=10 && rolls[f][0]!=10) ? rolls[f][2]!=-1 : true ) || (f==9 && rolls[f][0]==10 && rolls[f][1]!=-1 && rolls[f][2]!=-1 ) ) );
            bool isStrikeCurrFramePending = f < 9 && rolls[f][0] == 10 && GetNextTwoRolls(rolls, f, currentFrameInProgress) == -1 && currentFrameInProgress >=f ;
            bool isSpareCurrFramePending = f < 9 && rolls[f][0] != -1 && rolls[f][1] != -1 && (rolls[f][0] + rolls[f][1] == 10) && GetNextRoll(rolls, f, currentFrameInProgress) == -1 && currentFrameInProgress >= f;
            
            if (f > currentFrameInProgress || (f == currentFrameInProgress && rolls[f][0] == -1) )
            {
                 Console.Write(f < 9 ? "     │" : "       │"); // Frame not yet played or started
            }
            else if(isStrikeCurrFramePending || isSpareCurrFramePending)
            {
                 Console.Write(f < 9 ? "     │" : "       │"); // Bonus is pending for a completed strike/spare
            }
             else if (f == currentFrameInProgress && rolls[f][0] != -1 && rolls[f][0] < 10 && rolls[f][1] == -1) {
                 Console.Write(f < 9 ? "     │" : "       │"); // Open frame, first ball rolled, score not yet final for display
             }
            else if (frameScores[f] == 0 && pointsGained[f] == 0 && rolls[f][0] != -1 && !(rolls[f][0]==0 && rolls[f][1]==0 && (f<9 ? rolls[f][1]!=-1 : true) ) ) {
                 // Special case for 0 score that might be pending vs actual 0
                 // If pointsGained is 0, and rolls happened, and it's not an actual 0,0 open frame, it might be pending
                 Console.Write(f < 9 ? "     │" : "       │");
            }
            else
            {
                Console.Write(f < 9 ? $" {frameScores[f],3} │" : $" {frameScores[f],4}  │");
            }
        }
        Console.WriteLine(); 
        Console.WriteLine("└─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┴───────┘");
    }

    /// <summary>
    /// Calculates points for each frame and cumulative scores up to currentFramePlayed.
    /// Scores are only calculated if all necessary rolls (including bonuses) are available.
    /// </summary>
    static void CalculateScores(int[][] rolls, int[] pointsGainedOutput, int[] frameScoresOutput, int currentFramePlayed)
    {
        Array.Clear(pointsGainedOutput, 0, pointsGainedOutput.Length);
        Array.Clear(frameScoresOutput, 0, frameScoresOutput.Length);
        int cumulativeScore = 0;

        for (int f = 0; f <= currentFramePlayed; f++)
        {
             bool frameScoreDeterminedThisPass = false;
            if (rolls[f][0] == -1 && f <= currentFramePlayed) { 
                 if (f > 0) frameScoresOutput[f] = frameScoresOutput[f-1]; 
                 else frameScoresOutput[f] = 0;
                 pointsGainedOutput[f] = 0;
                 continue;
            }

            int currentFrameRawPoints = 0;
            bool bonusFinalizedOrNotApplicable = true;

            if (f < 9) // Frames 1-9
            {
                if (rolls[f][0] == 10) // Strike
                {
                    currentFrameRawPoints = 10;
                    int bonus = GetNextTwoRolls(rolls, f, currentFramePlayed);
                    if (bonus != -1) currentFrameRawPoints += bonus;
                    else bonusFinalizedOrNotApplicable = false;
                }
                else if (rolls[f][1] != -1 && (rolls[f][0] + rolls[f][1] == 10)) // Spare
                {
                    currentFrameRawPoints = 10;
                    int bonus = GetNextRoll(rolls, f, currentFramePlayed);
                    if (bonus != -1) currentFrameRawPoints += bonus;
                    else bonusFinalizedOrNotApplicable = false;
                }
                else if (rolls[f][1] != -1) // Open frame, both rolls played
                {
                    currentFrameRawPoints = rolls[f][0] + rolls[f][1];
                }
                else // Only first roll played (not a strike), frame score not final yet for pointsGained
                {
                    bonusFinalizedOrNotApplicable = false; 
                }
            }
            else // 10th Frame - Score is just sum of pins for this frame.
            {
                // All rolls must be completed if eligible for bonus rolls for score to be final.
                if(rolls[f][0] != -1) currentFrameRawPoints += rolls[f][0];
                
                if(rolls[f][1] != -1) {
                    currentFrameRawPoints += rolls[f][1];
                    // If R1+R2 was strike/spare, R3 must be played or -1 for score to be final
                    if((rolls[f][0] == 10 || (rolls[f][0] + rolls[f][1] == 10)) && rolls[f][2] == -1 && currentFramePlayed == f){
                        // If eligible for 3rd roll but not taken yet, it's not final.
                        bonusFinalizedOrNotApplicable = false;
                    } else if (rolls[f][2] != -1) {
                        currentFrameRawPoints += rolls[f][2];
                    }
                } else if (rolls[f][0] != -1 && rolls[f][0] < 10 && currentFramePlayed ==f){ // R1 played, <10, R2 not yet
                     bonusFinalizedOrNotApplicable = false;
                } else if (rolls[f][0] == -1 && currentFramePlayed == f){ // R1 not yet played
                     bonusFinalizedOrNotApplicable = false;
                }
            }

            if (bonusFinalizedOrNotApplicable && rolls[f][0]!=-1)
            {
                pointsGainedOutput[f] = currentFrameRawPoints;
                frameScoreDeterminedThisPass = true;
            }
            else 
            {
                pointsGainedOutput[f] = 0; // Points for this frame are not yet final
            }
            
            // Update cumulative score
            // If this frame's points are determined, add them.
            // Otherwise, the cumulative score up to this frame is the same as the previous frame's.
            if (frameScoreDeterminedThisPass) {
                cumulativeScore += pointsGainedOutput[f];
                frameScoresOutput[f] = cumulativeScore;
            } else {
                 if (f > 0) frameScoresOutput[f] = frameScoresOutput[f-1];
                 else frameScoresOutput[f] = 0; // If first frame is not determined, its score is 0
            }
        }
    }

    static int GetNextRoll(int[][] rolls, int currentFrame, int maxFramePlayedActually)
    {
        if (currentFrame + 1 > 9) return -1; 
        if (currentFrame + 1 <= maxFramePlayedActually && rolls[currentFrame + 1][0] != -1)
        {
            return rolls[currentFrame + 1][0];
        }
        return -1; 
    }

    static int GetNextTwoRolls(int[][] rolls, int currentFrame, int maxFramePlayedActually)
    {
        if (currentFrame >= 9) return -1; 

        if (currentFrame == 8) // Strike in frame 9, bonus from 10th frame rolls 1 and 2
        {
            if (maxFramePlayedActually >= 9 && rolls[9][0] != -1 && rolls[9][1] != -1)
            {
                return rolls[9][0] + rolls[9][1];
            }
            return -1; 
        }
        
        // Strike in frames 1-8
        if (maxFramePlayedActually >= currentFrame + 1 && rolls[currentFrame + 1][0] != -1)
        {
            int bonus1 = rolls[currentFrame + 1][0];
            if (bonus1 == 10) // Next frame was also a strike
            {
                if (currentFrame + 2 > 9) return -1; // Cannot get second bonus from beyond 10th frame
                if (maxFramePlayedActually >= currentFrame + 2 && rolls[currentFrame + 2][0] != -1)
                {
                    return bonus1 + rolls[currentFrame + 2][0];
                }
                return -1; 
            }
            else // Next frame was not a strike
            {
                if (rolls[currentFrame + 1][1] != -1) 
                {
                    return bonus1 + rolls[currentFrame + 1][1];
                }
                return -1; 
            }
        }
        return -1; 
    }
}
