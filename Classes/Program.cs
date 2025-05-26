using System;
using System.Collections.Generic;
using System.Linq;

class Location
{
    public string? Name;
    public string? Description;
    public List<Neighbor> Neighbors = new List<Neighbor>();
    public List<Path> ShortestPaths = new List<Path>();

    public override string ToString() => Name ?? string.Empty;
}

class Neighbor
{
    public Location? Location;
    public int Distance;
}

class Path
{
    public Location? Location;
    public int Distance;
    public List<string> StopNames = new List<string>();
}

class Program
{
    static void ConnectLocations(Location a, Location b, int distance)
    {
        a.Neighbors.Add(new Neighbor { Location = b, Distance = distance });
        b.Neighbors.Add(new Neighbor { Location = a, Distance = distance });
    }

    static void Dijkstra(List<Location> graph, Location source)
    {
        var dist = new Dictionary<Location, int>();
        var prev = new Dictionary<Location, Location?>();
        var Q = new List<Location>();

        foreach (var v in graph)
        {
            dist[v] = int.MaxValue;
            prev[v] = null;
            Q.Add(v);
        }

        dist[source] = 0;

        while (Q.Any())
        {
            Location u = Q.OrderBy(v => dist[v]).First();
            Q.Remove(u);

            foreach (var neighbor in u.Neighbors)
            {
                if (neighbor.Location == null) continue;
                int alt = dist[u] + neighbor.Distance;
                if (alt < dist[neighbor.Location])
                {
                    dist[neighbor.Location] = alt;
                    prev[neighbor.Location] = u;
                }
            }
        }

        foreach (Location other in graph)
        {
            if (other == source) continue;

            var path = new Path { Location = other, Distance = dist[other] };
            source.ShortestPaths.Add(path);

            Location? stop = prev[other];
            var stops = new List<string>();
            while (stop != null && stop != source)
            {
                if (stop.Name != null)
                    stops.Insert(0, stop.Name);
                stop = prev[stop];
            }

            path.StopNames = stops;
        }

        source.ShortestPaths.Sort((a, b) => a.Distance.CompareTo(b.Distance));
    }

    static void Main()
    {
        // Step 1: Create locations
        var winterfell = new Location { Name = "Winterfell", Description = "the capital of the Kingdom of the North" };
        var pyke = new Location { Name = "Pyke", Description = "the stronghold and seat of House Greyjoy" };
        var riverrun = new Location { Name = "Riverrun", Description = "a large castle located in the central-western part of the Riverlands" };
        var theTrident = new Location { Name = "The Trident", Description = "one of the largest and most well-known rivers on the continent of Westeros" };
        var kingsLanding = new Location { Name = "King's Landing", Description = "the capital, and largest city, of the Seven Kingdoms" };
        var highgarden = new Location { Name = "Highgarden", Description = "the seat of House Tyrell and the regional capital of the Reach" };

        var locations = new List<Location> { winterfell, pyke, riverrun, theTrident, kingsLanding, highgarden };

        // Step 2: Connect locations with distances
        ConnectLocations(winterfell, pyke, 18);
        ConnectLocations(winterfell, theTrident, 10);
        ConnectLocations(pyke, riverrun, 5);
        ConnectLocations(theTrident, riverrun, 2);
        ConnectLocations(theTrident, kingsLanding, 5);
        ConnectLocations(riverrun, highgarden, 3);
        ConnectLocations(riverrun, kingsLanding, 2);
        ConnectLocations(kingsLanding, highgarden, 8);

        // Step 3: Calculate shortest paths for each location
        foreach (var loc in locations)
            Dijkstra(locations, loc);

        // Step 4: Start at a location
        var currentLocation = winterfell;

        while (true)
        {
            Console.WriteLine($"\nWelcome to {currentLocation?.Name ?? "Unknown"}, {currentLocation?.Description ?? "No description"}.\n");

            Console.WriteLine("Possible destinations are:");
            if (currentLocation?.ShortestPaths != null)
            {
                for (int i = 0; i < currentLocation.ShortestPaths.Count; i++)
                {
                    var path = currentLocation.ShortestPaths[i];
                    string stops = path.StopNames.Count > 0 ? $" via {string.Join(", ", path.StopNames)}" : "";
                    string locationName = path.Location?.Name ?? "Unknown";
                    Console.WriteLine($"{i + 1}. {locationName} ({path.Distance}{stops})");
                }
            }

            Console.WriteLine("\nWhere do you want to travel? (Enter number or 0 to exit)");
            if (currentLocation?.ShortestPaths == null || !int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > currentLocation.ShortestPaths.Count)
            {
                Console.WriteLine("Invalid input.");
                continue;
            }

            if (choice == 0)
                break;

            currentLocation = currentLocation.ShortestPaths[choice - 1].Location;
        }
    }
}
