using System;
using System.Collections.Generic;

class CapitalsQuiz
{
    static void Main()
    {
        var capitals = new Dictionary<string, string>
        {
            { "Sweden", "Stockholm" },
            { "Norway", "Oslo" },
            { "Finland", "Helsinki" },
            { "Denmark", "Copenhagen" },
            { "Iceland", "Reykjavik" },
            { "Slovenia", "Ljubljana" },
            { "Austria", "Vienna" },
            { "Poland", "Warsaw" },
            { "Germany", "Berlin" }
        };

        var random = new Random();
        var countries = new List<string>(capitals.Keys);
        string country = countries[random.Next(countries.Count)];

        Console.WriteLine($"What is the capital of {country}?");
        string answer = Console.ReadLine() ?? string.Empty;

        if (capitals[country].Equals(answer, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\nCorrect!");
        }
        else
        {
            Console.WriteLine($"\nIncorrect. It is {capitals[country]}.");
        }
    }
}
