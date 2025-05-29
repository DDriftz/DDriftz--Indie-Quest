using System;

public class Program
{
    public static void Main()
    {
        // Set the dimensions of the map.
        int width = 20;
        int height = 10;
        bool[,] roads = new bool[width, height];
        Random rand = new Random();
        
        // Generate several random intersections.
        for (int i = 0; i < 3; i++)
        {
            int x = rand.Next(0, width);
            int y = rand.Next(0, height);
            CityGenerator.GenerateIntersection(roads, x, y);
        }
        
        // Draw the resulting city map.
        CityGenerator.DrawRoads(roads);
    }
}

public class CityGenerator
{
    // Generates a road in the specified direction starting at (startX, startY)
    public static void GenerateRoad(bool[,] roads, int startX, int startY, int direction)
    {
        int width = roads.GetLength(0);
        int height = roads.GetLength(1);
        int x = startX, y = startY;
        // Mark the starting point as road.
        roads[x, y] = true;

        // Determine the increments based on the direction:
        // 0 = right, 1 = down, 2 = left, 3 = up.
        int dx = 0, dy = 0;
        switch (direction)
        {
            case 0: dx = 1; break;   // right
            case 1: dy = 1; break;   // down
            case 2: dx = -1; break;  // left
            case 3: dy = -1; break;  // up
        }

        // Extend the road until the edge of the map is reached.
        while (true)
        {
            int nextX = x + dx;
            int nextY = y + dy;
            if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height)
                break;
            roads[nextX, nextY] = true;
            x = nextX;
            y = nextY;
        }
    }
    
    // Generates an intersection at (x, y) with a 70% chance to extend a road in each direction.
    public static void GenerateIntersection(bool[,] roads, int x, int y)
    {
        // Mark the intersection as a road.
        roads[x, y] = true;
        Random rand = new Random();
        
        // Check each direction (0 = right, 1 = down, 2 = left, 3 = up) with a 70% chance.
        for (int direction = 0; direction < 4; direction++)
        {
            if (rand.NextDouble() < 0.7)
            {
                GenerateRoad(roads, x, y, direction);
            }
        }
    }
    
    // Helper method to draw the roads map using '#' for roads and '.' for empty spaces.
    public static void DrawRoads(bool[,] roads)
    {
        int width = roads.GetLength(0);
        int height = roads.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Console.Write(roads[x, y] ? "#" : ".");
            }
            Console.WriteLine();
        }
    }
}
