using System;

class Program
{
    static void Main()
    {
        int[] testNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 21, 53, 100, 112, 121,250, 371, 500, 1732, 4444, 15372, 1403721, };
        
        foreach (int number in testNumbers)
        {
            Console.WriteLine(OrdinalNumber(number));
        }
    }

    static string OrdinalNumber(int number)
    {
        int lastDigit = number % 10;
        int secondLastDigit = (number / 10) % 10;
        
        if (secondLastDigit == 1)
            return number + "th";
        
        return number + (lastDigit == 1 ? "st" :
                         lastDigit == 2 ? "nd" :
                         lastDigit == 3 ? "rd" : "th");
    }
}
