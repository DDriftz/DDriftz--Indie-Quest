using System;
using System.Collections.Generic;

class Location
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<Location> Neighbors = new List<Location>();
}

class Program
{
    static void ConnectLocations(Location a, Location b)
    {
        a.Neighbors.Add(b);
        b.Neighbors.Add(a);
    }

    static void ShowLocation(Location location)
    {
        Console.WriteLine($"\nWelcome to {location.Name}, {location.Description}");
        Console.WriteLine("\nPossible destinations are:");
        for (int i = 0; i < location.Neighbors.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {location.Neighbors[i].Name}");
        }
    }

    static void Main()
    {
        var winterfell = new Location { Name = "Winterfell", Description = "the capital of the Kingdom of the North" };
        var pyke = new Location { Name = "Pyke", Description = "the stronghold and seat of House Greyjoy" };
        var riverrun = new Location { Name = "Riverrun", Description = "a large castle located in the central-western part of the Riverlands" };
        var theTrident = new Location { Name = "The Trident", Description = "one of the largest and most well-known rivers on the continent of Westeros" };
        var kingsLanding = new Location { Name = "King's Landing", Description = "the capital, and largest city, of the Seven Kingdoms" };
        var highgarden = new Location { Name = "Highgarden", Description = "the seat of House Tyrell and the regional capital of the Reach" };

        ConnectLocations(winterfell, pyke);
        ConnectLocations(winterfell, theTrident);
        ConnectLocations(theTrident, riverrun);
        ConnectLocations(theTrident, kingsLanding);
        ConnectLocations(riverrun, pyke);
        ConnectLocations(riverrun, highgarden);
        ConnectLocations(kingsLanding, highgarden);
        ConnectLocations(kingsLanding, riverrun);

        Location currentLocation = winterfell;
        ShowLocation(currentLocation);
    }
}
