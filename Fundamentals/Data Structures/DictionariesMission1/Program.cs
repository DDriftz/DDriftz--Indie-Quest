using System;
using System.Collections.Generic;

class OlympicsQuiz
{
    static void Main()
    {
        var hostCountries = new Dictionary<int, string>
        {
            { 2000, "Australia" },
            { 2004, "Greece" },
            { 2008, "China" },
            { 2012, "United Kingdom" },
            { 2016, "Brazil" },
            { 2020, "Japan" },
            { 2018, "South Korea" }, // Winter Olympics
            { 2014, "Russia" }       // Winter Olympics
        };

        var random = new Random();
        var years = new List<int>(hostCountries.Keys);
        int year = years[random.Next(years.Count)];

        Console.WriteLine($"Which country hosted the Olympic Games in {year}?");
        string answer = Console.ReadLine() ?? string.Empty;

        if (hostCountries[year].Equals(answer, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\nCorrect!");
        }
        else
        {
            Console.WriteLine($"\nIncorrect. It was {hostCountries[year]}.");
        }
    }
}
