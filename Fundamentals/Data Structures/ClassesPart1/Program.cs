using System;
using System.Collections.Generic;

class Location
{
    public required string Name;
    public required string Description;
}

class Program
{
    static void Main()
    {
        var winterfell = new Location { Name = "Winterfell", Description = "the capital of the Kingdom of the North" };
        var pyke = new Location { Name = "Pyke", Description = "the stronghold and seat of House Greyjoy" };
        var riverrun = new Location { Name = "Riverrun", Description = "a large castle located in the central-western part of the Riverlands" };
        var theTrident = new Location { Name = "The Trident", Description = "one of the largest and most well-known rivers on the continent of Westeros" };
        var kingsLanding = new Location { Name = "King's Landing", Description = "the capital, and largest city, of the Seven Kingdoms" };
        var highgarden = new Location { Name = "Highgarden", Description = "the seat of House Tyrell and the regional capital of the Reach" };

        var locations = new List<Location> { winterfell, pyke, riverrun, theTrident, kingsLanding, highgarden };

        // Test current location
        Location currentLocation = riverrun;

        Console.WriteLine($"Welcome to {currentLocation.Name}, {currentLocation.Description}");
    }
}
