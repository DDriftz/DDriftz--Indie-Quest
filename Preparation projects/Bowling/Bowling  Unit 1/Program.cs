using System;

class BowlingDisplay
{
    static void Main()
    {
        // You can set this between 1 and 10 as per the document
        int totalFrames = 5; 
        Random rand = new Random();
        string[] rollDisplays = new string[totalFrames]; // Stores the 5-character display string for each frame's rolls

        // Simulate rolls and prepare display strings for each frame
        for (int i = 0; i < totalFrames; i++)
        {
            // Initialize the 5-character array for the frame's roll display part
            // Format: " |R1|R2|" or " |X| "
            char[] frameChars = new char[5];
            frameChars[0] = ' '; // Left padding
            frameChars[1] = '|'; // Separator before Roll 1 / Strike symbol
            frameChars[3] = '|'; // Separator after Roll 1 / Strike symbol, before Roll 2 / Spare

            int roll1 = rand.Next(0, 11); // Pins for first roll (0-10)

            if (roll1 == 10) // Strike
            {
                frameChars[2] = 'X'; // 'X' in the first roll's visual slot
                frameChars[4] = ' '; // Second roll's visual slot is empty for a strike
            }
            else // Not a strike (could be an open frame or a spare)
            {
                // Set character for the first roll
                // (0 is displayed as '-', 1-9 as the digit)
                frameChars[2] = (roll1 == 0) ? '-' : roll1.ToString()[0]; 

                int roll2 = rand.Next(0, 11 - roll1); // Pins for second roll (0 to 10-roll1)

                if (roll1 + roll2 == 10) // Spare
                {
                    frameChars[4] = '/'; // '/' in the second roll's visual slot
                }
                else // Open frame for the second roll
                {
                    // Set character for the second roll
                    // (0 is displayed as '-', 1-9 as the digit)
                    frameChars[4] = (roll2 == 0) ? '-' : roll2.ToString()[0];
                }
            }
            rollDisplays[i] = new string(frameChars); // Store the 5-character string " |R1|R2|" or " |X| "
        }

        // --- Display the scoresheet ---

        // Top border line
        for (int i = 0; i < totalFrames; i++)
        {
            Console.Write("+-----");
        }
        Console.WriteLine("+");

        // First data row: Rolls display
        for (int i = 0; i < totalFrames; i++)
        {
            Console.Write($"|{rollDisplays[i]}"); // rollDisplays[i] is the 5-char string like " |7|2|" or " |X| "
        }
        Console.WriteLine("|");

        // Second data row: Fixed separator graphic
        for (int i = 0; i < totalFrames; i++)
        {
            Console.Write("| ----");
        }
        Console.WriteLine("|");

        // Third data row: Empty (for scores, which are not displayed as per instructions)
        for (int i = 0; i < totalFrames; i++)
        {
            Console.Write("|     ");
        }
        Console.WriteLine("|");

        // Bottom border line
        for (int i = 0; i < totalFrames; i++)
        {
            Console.Write("+-----");
        }
        Console.WriteLine("+");
    }
}
