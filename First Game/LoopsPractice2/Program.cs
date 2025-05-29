using System;

class Program
{
    static void Main()
    {
        Console.Write("Enter a number (n): ");
        int n = int.Parse(Console.ReadLine()!);

        // 1. Measuring Tape
        Console.WriteLine("\n1. Measuring Tape:");
        for (int i = 0; i <= n * 10; i += 5)
        {
            Console.Write(i.ToString().PadRight(5));
        }
        Console.WriteLine();
        Console.WriteLine("|" + string.Concat(Enumerable.Repeat("____|", n * 2)));

        // 2. Castle with Guard Towers and a Gate
        Console.WriteLine("\n2. Castle with Guard Towers and a Gate:");
        for (int i = 0; i < n; i++)
        {
            Console.Write("[^^^] ");
        }
        Console.WriteLine();
        for (int i = 0; i < n; i++)
        {
            Console.Write("|   | ");
        }
        Console.WriteLine();
        for (int i = 0; i < n; i++)
        {
            Console.Write("|  /|\\  | ");
        }
        Console.WriteLine();
        for (int i = 0; i < n; i++)
        {
            Console.Write("|__|||__| ");
        }
        Console.WriteLine();

        // 3. LCD-style last digit of (n % 10)
        int digit = n % 10;
        Console.WriteLine("\n3. LCD-style Display of Last Digit:");
        string[] lcd_digits = {
            " _ \n| |\n|_|", // 0
            "   \n  |\n  |", // 1
            " _ \n _|\n|_ ", // 2
            " _ \n _|\n _|", // 3
            "   \n|_|\n  |", // 4
            " _ \n|_ \n _|", // 5
            " _ \n|_ \n|_|", // 6
            " _ \n  |\n  |", // 7
            " _ \n|_|\n|_|", // 8
            " _ \n|_|\n _|"  // 9
        };
        Console.WriteLine(lcd_digits[digit]);

        // 4. Swedish Flag
        Console.WriteLine("\n4. Swedish Flag:");
        int flagHeight = 2 * n + 1;
        Console.WriteLine("() " + new string('_', flagHeight));
        for (int i = 0; i < flagHeight; i++)
        {
            if (i == n)
                Console.WriteLine("||__| |____| ||___   ______|");
            else
                Console.WriteLine("||  | |    | ||   | |      |");
        }
        Console.WriteLine(" ||___|_|______| ");
        Console.WriteLine(" || ");

        // 5. United Kingdom Flag
        Console.WriteLine("\n5. United Kingdom Flag:");
        Console.WriteLine("() " + new string('_', 2 * n + 1));
        for (int i = 0; i < 2 * n + 1; i++)
        {
            Console.Write("||");
            for (int j = 0; j < 2 * n + 1; j++)
            {
                if (i == j || i + j == 2 * n)
                    Console.Write("/");
                else if (i == n || j == n)
                    Console.Write("|");
                else
                    Console.Write(" ");
            }
            Console.WriteLine("||");
        }
        Console.WriteLine(" ||");

        // 6. American Flag with n lines of stars
        Console.WriteLine("\n6. American Flag:");
        Console.WriteLine("() " + new string('_', 12));
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine("||* * * | #####|");
            Console.WriteLine("|| * *  | #####|");
        }
        Console.WriteLine("||######|");
        Console.WriteLine("||      |");
    }
}
