using System;

class Program
{
    static void Main()
    {
        for (int i = 0; i <= 10; i++)
        {
            Console.WriteLine($"{i}! = {Factorial(i)}");
        }
    }

    static int Factorial(int n)
    {
        if (n == 0 || n == 1)
            return 1;
        
        return n * Factorial(n - 1);
    }
}
