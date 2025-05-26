using System;

public class Program
{
    public static void Main()
    {
        // Test the CreateDayDescription method with different inputs.
        Console.WriteLine(ArrayMissions.CreateDayDescription(7, 1, 134));   // Expected: 7th day of Summer in the year 134
        Console.WriteLine(ArrayMissions.CreateDayDescription(41, 3, 22));   // Expected: 41st day of Winter in the year 22
        Console.WriteLine(ArrayMissions.CreateDayDescription(3, 0, 1601));  // Expected: 3rd day of Spring in the year 1601
    }
}

public class ArrayMissions
{
    // Helper method to add ordinal suffixes (st, nd, rd, th) to day numbers.
    public static string GetOrdinal(int number)
    {
        if (number % 100 >= 11 && number % 100 <= 13)
            return number + "th";
        switch (number % 10)
        {
            case 1: return number + "st";
            case 2: return number + "nd";
            case 3: return number + "rd";
            default: return number + "th";
        }
    }

    // Method that creates a day description using an array of seasons.
    public static string CreateDayDescription(int day, int season, int year)
    {
        string[] Seasons = { "Spring", "Summer", "Fall", "Winter" };
        string dayOrdinal = GetOrdinal(day);
        return $"{dayOrdinal} day of {Seasons[season]} in the year {year}";
    }
}
