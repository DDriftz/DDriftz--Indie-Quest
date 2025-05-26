using System;

public class Program
{
    public static void Main()
    {
        // Define the dimensions of the map.
        int width = 20;
        int height = 10;
        bool[,] roads = new bool[width, height];
        Random rand = new Random();
        
        // Generate several random intersections.
        for (int i = 0; i < 5; i++)
        {
            int x = rand.Next(0, width);
            int y = rand.Next(0, height);
            CityGenerator.GenerateIntersection(roads, x, y);
        }
        
        // Draw the city map using upgraded drawing logic.
        CityGenerator.DrawRoads(roads);
    }
}

public class CityGenerator
{
    // Generates a road in the specified direction starting at (startX, startY).
    public static void GenerateRoad(bool[,] roads, int startX, int startY, int direction)
    {
        int width = roads.GetLength(0);
        int height = roads.GetLength(1);
        int x = startX, y = startY;
        roads[x, y] = true;
        
        int dx = 0, dy = 0;
        switch (direction)
        {
            case 0: dx = 1; break;   // right
            case 1: dy = 1; break;   // down
            case 2: dx = -1; break;  // left
            case 3: dy = -1; break;  // up
        }
        
        // Extend the road until the map edge is reached.
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
        roads[x, y] = true;
        Random rand = new Random();
        
        // For each direction (0 = right, 1 = down, 2 = left, 3 = up), extend a road with 70% probability.
        for (int direction = 0; direction < 4; direction++)
        {
            if (rand.NextDouble() < 0.7)
            {
                GenerateRoad(roads, x, y, direction);
            }
        }
    }
    
    // Upgraded method to draw the roads map using special characters based on adjacent road cells.
    public static void DrawRoads(bool[,] roads)
    {
        int width = roads.GetLength(0);
        int height = roads.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (roads[x, y])
                {
                    // Check each neighboring cell.
                    bool up = (y > 0) && roads[x, y - 1];
                    bool down = (y < height - 1) && roads[x, y + 1];
                    bool left = (x > 0) && roads[x - 1, y];
                    bool right = (x < width - 1) && roads[x + 1, y];
                    
                    char roadChar = GetRoadChar(up, down, left, right);
                    Console.Write(roadChar);
                }
                else
                {
                    Console.Write(".");
                }
            }
            Console.WriteLine();
        }
    }
    
    // Determines the proper road character based on the presence of neighboring roads.
    private static char GetRoadChar(bool up, bool down, bool left, bool right)
    {
        // Intersection: all four directions.
        if (up && down && left && right)
            return '╬';
        // T-intersections:
        if (up && down && left && !right)
            return '╣';
        if (up && down && right && !left)
            return '╠';
        if (up && left && right && !down)
            return '╩';
        if (down && left && right && !up)
            return '╦';
        // Corners:
        if (up && left && !down && !right)
            return '╝';
        if (down && left && !up && !right)
            return '╗';
        if (up && right && !down && !left)
            return '╚';
        if (down && right && !up && !left)
            return '╔';
        // Straight roads:
        if (up && down && !left && !right)
            return '║';
        if (left && right && !up && !down)
            return '═';
        
        // For dead-ends or cells that don't match the above patterns, use a default.
        return '#';
    }
}
