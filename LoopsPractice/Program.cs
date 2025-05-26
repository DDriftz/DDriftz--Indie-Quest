using System;

class Program
{
    static void Main()
    {
        // Part 1: Drawing Shapes with Hashtags
        Console.Write("Enter a number (n): ");
        int n = int.Parse(Console.ReadLine()!);

        // 2. Draw a line n long
        Console.WriteLine("\n2. Draw a line n long:");
        Console.WriteLine(new string('#', n));

        // 3. Draw a square with side n
        Console.WriteLine("\n3. Draw a square with side n:");
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine(new string('#', n));
        }

        // 4. Draw a right triangle with legs size n
        Console.WriteLine("\n4. Draw a right triangle with legs size n:");
        for (int i = 1; i <= n; i++)
        {
            Console.WriteLine(new string('#', i));
        }

        // 5. Draw a parallelogram with height n
        Console.WriteLine("\n5. Draw a parallelogram with height n:");
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine(new string(' ', n - i - 1) + new string('#', n));
        }

        // 6. Draw an Isosceles triangle with height n and base 2n-1
        Console.WriteLine("\n6. Draw an Isosceles triangle with height n and base 2n-1:");
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine(new string(' ', n - i - 1) + new string('#', 2 * i + 1));
        }

        // 7. Draw a square with side n with every other row blank
        Console.WriteLine("\n7. Draw a square with side n with every other row blank:");
        for (int i = 0; i < n; i++)
        {
            if (i % 2 == 0)
                Console.WriteLine(new string('#', n));
            else
                Console.WriteLine(new string('.', n));
        }

        // 8. Draw a square with side n with every other column blank
        Console.WriteLine("\n8. Draw a square with side n with every other column blank:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (j % 2 == 0)
                    Console.Write('#');
                else
                    Console.Write('.');
            }
            Console.WriteLine();
        }

        // 9. Draw a grid with side n with a line on every other row and every other column
        Console.WriteLine("\n9. Draw a grid with side n with a line on every other row and every other column:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i % 2 == 0 || j % 2 == 0)
                    Console.Write('#');
                else
                    Console.Write('.');
            }
            Console.WriteLine();
        }

        // 10. Draw a fence with side n with a line on every other row and every other column
        Console.WriteLine("\n10. Draw a fence with side n with a line on every other row and every other column:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if ((i + j) % 2 == 0)
                    Console.Write('#');
                else
                    Console.Write('.');
            }
            Console.WriteLine();
        }

        // 11. Draw a chessboard with side n
        Console.WriteLine("\n11. Draw a chessboard with side n:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if ((i + j) % 2 == 0)
                    Console.Write('#');
                else
                    Console.Write('.');
            }
            Console.WriteLine();
        }

        // 12. Draw a slope that starts 1 wide and doubles the width in each row
        Console.WriteLine("\n12. Draw a slope that starts 1 wide and doubles the width in each row:");
        int width = 1;
        while (width < 80)
        {
            Console.WriteLine(new string('#', width));
            width *= 2;
        }

        // 13. Draw a reverse slope that starts at width 35 and decreases by n in every row
        Console.WriteLine("\n13. Draw a reverse slope that starts at width 35 and decreases by n in every row:");
        width = 35;
        while (width > 0)
        {
            Console.WriteLine(new string('#', width));
            width -= n;
        }

        // 14. Draw a cliff
        Console.WriteLine("\n14. Draw a cliff:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n - i; j++)
            {
                Console.WriteLine(new string('#', n - i));
            }
        }

        // 15. Draw a set of lines
        Console.WriteLine("\n15. Draw a set of lines:");
        for (int i = n; i > 0; i--)
        {
            for (int j = i; j > 0; j--)
            {
                Console.Write(new string('#', j) + ".");
            }
            Console.WriteLine();
        }
    }
}