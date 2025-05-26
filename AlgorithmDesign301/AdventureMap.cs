using System;
using System.Collections.Generic;

class AdventureMap
{
    // Global map parameters and grid
    private static int mapWidth;
    private static int mapHeight;
    private static char[,] map = new char[1, 1]!;
    private static readonly Random random = new Random();

    // Global lists holding precalculated positions for curved elements
    private static List<int> riverPath = new List<int>();
    private static List<int> wallPath = new List<int>();

    // Bridge information (for connecting the horizontal road)
    private static int bridgeY;
    private static int extendedBridgeStartX;
    private static int extendedBridgeEndX;

    static void Main()
    {
        // For example, generate a 75x20 map
        Map(75, 20);
    }

    /// <summary>
    /// Top-level function that generates the entire map.
    /// It follows the preparation and drawing phases as per our algorithm design.
    /// </summary>
    static void Map(int width, int height)
    {
        // ===== INITIALIZATION PHASE =====
        mapWidth = width;
        mapHeight = height;
        InitializeMap();

        // ===== STATIC ELEMENTS: Draw initial borders =====
        DrawMapBoundaries();
        DisplayTitle();  // Title is drawn early but may be overwritten by features

        // ===== PREPARATION PHASE: Calculate positions for dynamic elements =====
        riverPath = GenerateCurve(
            startX: mapWidth * 3 / 4,
            minX: mapWidth / 2,
            maxX: mapWidth - 2,
            curveChance: 0.3,
            length: mapHeight);

        wallPath = GenerateCurve(
            startX: mapWidth / 4,
            minX: 2,
            maxX: mapWidth / 2,
            curveChance: 0.1,
            length: mapHeight);

        // ===== WALL & FOREST PHASE =====
        GenerateWall();                // Draws the left-side wall with an opening
        GenerateForestInsideLeftOfWall();

        // ===== OPTIONAL TERRAIN FEATURES =====
        GenerateMountains();
        GenerateRiver();               // Draws the river along riverPath
        GenerateBridge();              // Aligns with horizontal road
        GenerateHorizontalRoad();      // Now includes forced crossing over the wall and the bridge
        GenerateVerticalRoad();        // Additional road near the river

        // ===== FEATURE PHASE: Place additional map structures =====
        GenerateVillagesAndHouses();
        GenerateCastles();
        GenerateDungeons();
        GenerateRuins();
        GenerateMarkets();
        GenerateShrines();
        GenerateBanks();
        GenerateLibraries();

        // ===== FINAL ADJUSTMENTS =====
        ReinforceMapBorders(); // In case features overwrote them
        DisplayTitle();        // Re-draw the title last so it stays visible

        // ===== DRAWING PHASE =====
        DrawMap();
        DrawLegend();
    }

    /// <summary>
    /// Initializes the map grid with blank spaces.
    /// </summary>
    private static void InitializeMap()
    {
        map = new char[mapHeight, mapWidth];
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                map[y, x] = ' ';
    }

    /// <summary>
    /// Draws the outer borders of the map.
    /// The top and bottom rows are drawn as horizontal borders ('-'),
    /// while the vertical borders (for rows between) are drawn as '|'.
    /// </summary>
    private static void DrawMapBoundaries()
    {
        // Draw horizontal borders on top and bottom.
        for (int x = 0; x < mapWidth; x++)
        {
            map[0, x] = '-';
            map[mapHeight - 1, x] = '-';
        }
        // Draw vertical borders only for the rows that are not the top or bottom.
        for (int y = 1; y < mapHeight - 1; y++)
        {
            map[y, 0] = '|';
            map[y, mapWidth - 1] = '|';
        }
    }

    /// <summary>
    /// Reapplies the borders in case any elements overwrote them.
    /// </summary>
    private static void ReinforceMapBorders()
    {
        DrawMapBoundaries();
    }

    /// <summary>
    /// Displays the map title centered on the second row.
    /// </summary>
    private static void DisplayTitle()
    {
        string title = "THE CITY OF MITHRANDIR";
        int startX = (mapWidth - title.Length) / 2;
        for (int i = 0; i < title.Length; i++)
            map[1, startX + i] = title[i];
    }

    /// <summary>
    /// Abstract method to generate a curved path.
    /// Returns a list of x‑coordinates for each row.
    /// </summary>
    private static List<int> GenerateCurve(int startX, int minX, int maxX, double curveChance, int length)
    {
        List<int> path = new List<int>();
        int currentX = startX;
        for (int i = 0; i < length; i++)
        {
            path.Add(currentX);
            double curveDirection = random.NextDouble();
            if (curveDirection < curveChance && currentX > minX)
                currentX--;
            else if (curveDirection > 1 - curveChance && currentX < maxX)
                currentX++;
        }
        return path;
    }

    /// <summary>
    /// Helper to draw a curve by setting map[y, curve[y]] = element.
    /// Used to draw the river.
    /// </summary>
    private static void DrawCurve(List<int> curve, char element)
    {
        for (int y = 0; y < curve.Count; y++)
        {
            int x = curve[y];
            if (x > 0 && x < mapWidth - 1 && y > 0 && y < mapHeight - 1)
                map[y, x] = element;
        }
    }

    /// <summary>
    /// Generates the wall using wallPath.
    /// In rows other than the wall opening, the wall is drawn as two adjacent columns.
    /// The character is chosen based on curvature: '\' if curving right, '/' if curving left, '|' if vertical.
    ///
    /// For the opening row (y == wallOpeningY), we leave the gap open
    /// and place turrets '[' and ']' side by side at wallX and wallX+1,
    /// matching the screenshot snippet you provided.
    /// </summary>
    private static void GenerateWall()
    {
        int wallOpeningY = mapHeight / 2;
        for (int y = 0; y < wallPath.Count; y++)
        {
            int wallX = wallPath[y];

            // If we're on the row of the wall opening:
            if (y == wallOpeningY)
            {
                // Place adjacent turrets [ and ] at wallX, wallX+1
                if (wallX >= 1 && wallX < mapWidth)
                    map[y, wallX] = '[';
                if (wallX + 1 < mapWidth)
                    map[y, wallX + 1] = ']';
                // No wall drawn in these columns (the opening)
            }
            else
            {
                // Normal row: two adjacent wall columns
                char wallChar = '|'; // default vertical
                if (y > 0)
                {
                    int dx = wallPath[y] - wallPath[y - 1];
                    if (dx > 0)      wallChar = '\\';
                    else if (dx < 0) wallChar = '/';
                    else             wallChar = '|';
                }
                if (wallX >= 0 && wallX < mapWidth)
                    map[y, wallX] = wallChar;
                if (wallX + 1 < mapWidth)
                    map[y, wallX + 1] = wallChar;
            }
        }
    }

    /// <summary>
    /// Plants a denser forest on the inside of the left side of the wall.
    /// For each row, for x from 1 up to the wall's x‑coordinate,
    /// it plants a tree ('T' or '@') with a 40% chance.
    /// </summary>
    private static void GenerateForestInsideLeftOfWall()
    {
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int wallX = wallPath[y];
            for (int x = 1; x < wallX; x++)
            {
                if (random.NextDouble() < 0.4)
                    map[y, x] = (random.Next(2) == 0) ? 'T' : '@';
            }
        }
    }

    /// <summary>
    /// (Optional) Generates mountains in the rightmost quarter.
    /// </summary>
    private static void GenerateMountains()
    {
        int mountainStart = mapWidth * 3 / 4;
        for (int y = 2; y < mapHeight - 2; y++)
            for (int x = mountainStart; x < mapWidth - 2; x++)
                if (random.NextDouble() > 0.85)
                    map[y, x] = '^';
    }

    /// <summary>
    /// Draws the river along the precalculated riverPath.
    /// </summary>
    private static void GenerateRiver()
    {
        DrawCurve(riverPath, '~');
    }

    /// <summary>
    /// Generates a bridge over the river on the same row as the horizontal road.
    /// The bridge spans from extendedBridgeStartX to extendedBridgeEndX.
    /// </summary>
    private static void GenerateBridge()
    {
        bridgeY = mapHeight / 2;
        int bridgeStartX = 0, bridgeEndX = 0;
        for (int x = 0; x < mapWidth; x++)
        {
            if (map[bridgeY, x] == '~')
            {
                bridgeStartX = x;
                while (x < mapWidth && map[bridgeY, x] == '~')
                    x++;
                bridgeEndX = x - 1;
                break;
            }
        }
        int extension = 3;
        extendedBridgeStartX = Math.Max(0, bridgeStartX - extension);
        extendedBridgeEndX = Math.Min(mapWidth - 1, bridgeEndX + extension);
        for (int x = extendedBridgeStartX; x <= extendedBridgeEndX; x++)
        {
            if (x >= 0 && x < mapWidth)
            {
                map[bridgeY, x] = '#';
                if (bridgeY > 0) map[bridgeY - 1, x] = '=';
                if (bridgeY < mapHeight - 1) map[bridgeY + 1, x] = '=';
            }
        }
    }

    /// <summary>
    /// Generates the horizontal road in multiple segments:
    /// 1) Left meandering
    /// 2) Forced through wall opening
    /// 3) Middle meandering
    /// 4) Forced across the bridge
    /// 5) Right meandering
    /// </summary>
    private static void GenerateHorizontalRoad()
    {
        int baseRoadY = mapHeight / 2;
        int startX = 1;
        int endX = mapWidth - 2;

        // Where the wall is in the middle row
        int wallXAtOpening = wallPath[baseRoadY];
        int wallForcedStart = Math.Max(startX, wallXAtOpening - 3);
        int wallForcedEnd = Math.Min(endX, wallXAtOpening + 3);

        // The forced section for the bridge
        int bridgeForcedStart = extendedBridgeStartX;
        int bridgeForcedEnd = extendedBridgeEndX;

        int roadY = baseRoadY;
        int changeInterval = 5;
        int changeChance = 80;
        int maxChange = 2;

        // --- Segment 1: Left meandering ---
        for (int x = startX; x < wallForcedStart; x++)
        {
            if (x % changeInterval == 0 && random.Next(100) < changeChance)
            {
                int direction = random.Next(-maxChange, maxChange + 1);
                roadY += direction;
                roadY = Math.Max(1, Math.Min(roadY, mapHeight - 2));
            }
            map[roadY, x] = '#';
        }

        // --- Segment 2: Forced across the wall opening ---
        for (int x = wallForcedStart; x <= wallForcedEnd; x++)
        {
            if (map[baseRoadY, x] != '[' && map[baseRoadY, x] != ']')
                map[baseRoadY, x] = '#';
        }

        // --- Segment 3: Middle meandering (before the bridge) ---
        roadY = baseRoadY;
        for (int x = wallForcedEnd + 1; x < bridgeForcedStart; x++)
        {
            if (x % changeInterval == 0 && random.Next(100) < changeChance)
            {
                int direction = random.Next(-maxChange, maxChange + 1);
                roadY += direction;
                roadY = Math.Max(1, Math.Min(roadY, mapHeight - 2));
            }
            map[roadY, x] = '#';
        }

        // --- Segment 4: Forced across the bridge ---
        for (int x = bridgeForcedStart; x <= bridgeForcedEnd; x++)
        {
            map[baseRoadY, x] = '#';
        }

        // --- Segment 5: Right meandering ---
        roadY = baseRoadY;
        for (int x = bridgeForcedEnd + 1; x <= endX; x++)
        {
            if (x % changeInterval == 0 && random.Next(100) < changeChance)
            {
                int direction = random.Next(-maxChange, maxChange + 1);
                roadY += direction;
                roadY = Math.Max(1, Math.Min(roadY, mapHeight - 2));
            }
            map[roadY, x] = '#';
        }
    }

    /// <summary>
    /// Generates a vertical road near the river (using the riverPath).
    /// </summary>
    private static void GenerateVerticalRoad()
    {
        for (int y = bridgeY; y < mapHeight - 2; y++)
        {
            int x = riverPath[y] - 5;
            if (x > 0 && x < mapWidth)
                map[y, x] = '#';
        }
    }

    // ---------- FEATURE FUNCTIONS ----------

    private static void GenerateVillagesAndHouses()
    {
        // Place 3 villages
        for (int i = 0; i < 3; i++)
        {
            int vx = random.Next(3, mapWidth - 3);
            int vy = random.Next(3, mapHeight - 3);
            if (map[vy, vx] == ' ')
            {
                map[vy, vx] = 'H';
                // Each village gets 5 houses
                for (int h = 0; h < 5; h++)
                {
                    int hx = vx + random.Next(-3, 4);
                    int hy = vy + random.Next(-2, 3);
                    if (hy >= 0 && hy < mapHeight &&
                        hx >= 0 && hx < mapWidth &&
                        map[hy, hx] == ' ')
                    {
                        map[hy, hx] = 'h';
                    }
                }
            }
        }
    }

    private static void GenerateCastles()
    {
        // Place 4 castles
        for (int i = 0; i < 4; i++)
        {
            int x = random.Next(3, mapWidth - 3);
            int y = random.Next(3, mapHeight - 3);
            if (map[y, x] == ' ')
                map[y, x] = 'K';
        }
    }

    private static void GenerateDungeons()
    {
        // Place 5 dungeons
        for (int i = 0; i < 5; i++)
        {
            int x = random.Next(3, mapWidth - 3);
            int y = random.Next(3, mapHeight - 3);
            if (map[y, x] == ' ')
                map[y, x] = 'D';
        }
    }

    private static void GenerateRuins()
    {
        // Place 12 ruins
        for (int i = 0; i < 12; i++)
        {
            int x = random.Next(5, mapWidth - 5);
            int y = random.Next(5, mapHeight - 5);
            if (map[y, x] == ' ')
                map[y, x] = 'R';
        }
    }

    private static void GenerateMarkets()
    {
        // Place 4 markets
        for (int i = 0; i < 4; i++)
        {
            int x = random.Next(3, mapWidth - 3);
            int y = random.Next(3, mapHeight - 3);
            if (map[y, x] == ' ')
                map[y, x] = 'M';
        }
    }

    private static void GenerateShrines()
    {
        // Place 4 shrines
        for (int i = 0; i < 4; i++)
        {
            int x = random.Next(3, mapWidth - 3);
            int y = random.Next(3, mapHeight - 3);
            if (map[y, x] == ' ')
                map[y, x] = 'S';
        }
    }

    private static void GenerateBanks()
    {
        // Place 4 banks
        for (int i = 0; i < 4; i++)
        {
            int x = random.Next(3, mapWidth - 3);
            int y = random.Next(3, mapHeight - 3);
            if (map[y, x] == ' ')
                map[y, x] = 'B';
        }
    }

    private static void GenerateLibraries()
    {
        // Place 4 libraries
        for (int i = 0; i < 4; i++)
        {
            int x = random.Next(3, mapWidth - 3);
            int y = random.Next(3, mapHeight - 3);
            if (map[y, x] == ' ')
                map[y, x] = 'L';
        }
    }

    /// <summary>
    /// Draws the final map with colors based on element types.
    /// </summary>
    private static void DrawMap()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Draw borders in bright yellow.
                if (y == 0 || y == mapHeight - 1)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(map[y, x]);
                    continue;
                }
                if (x == 0 || x == mapWidth - 1)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(map[y, x]);
                    continue;
                }
                // Draw the title in cyan.
                string title = "THE CITY OF MITHRANDIR";
                int titleStartX = (mapWidth - title.Length) / 2;
                if (y == 1 && x >= titleStartX && x < titleStartX + title.Length)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(map[y, x]);
                    x += title.Length - 1;
                    continue;
                }
                // Set colors for each element.
                switch (map[y, x])
                {
                    case 'T':
                    case '@':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                        break;
                    case '^':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.Gray : ConsoleColor.DarkGray;
                        break;
                    case '~':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.Blue : ConsoleColor.DarkBlue;
                        break;
                    case '#':
                    case '=':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.White : ConsoleColor.Gray;
                        break;
                    case '|':
                    case '[':
                    case ']':
                    case '\\':
                    case '/':
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case 'H':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.Yellow : ConsoleColor.White;
                        break;
                    case 'h':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.DarkYellow : ConsoleColor.Yellow;
                        break;
                    case 'K':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta;
                        break;
                    case 'D':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.DarkMagenta : ConsoleColor.Red;
                        break;
                    case 'R':
                        Console.ForegroundColor = random.Next(2) == 0 ? ConsoleColor.Red : ConsoleColor.DarkRed;
                        break;
                    case 'M':
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case 'S':
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        break;
                    case 'B':
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case 'L':
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                }
                Console.Write(map[y, x]);
            }
            Console.WriteLine();
        }
        Console.ResetColor();
    }

    /// <summary>
    /// Draws the legend showing the characters and their corresponding features.
    /// </summary>
    private static void DrawLegend()
    {
        Console.WriteLine("\nLegend (with color variations):");
        Console.WriteLine("  Boundaries: '-' and '|'  [Yellow]");
        Console.WriteLine("  Title: \"THE CITY OF MITHRANDIR\"  [Cyan]");
        Console.WriteLine("  Forest (inside left of wall): 'T', '@'  [Green / DarkGreen]");
        Console.WriteLine("  Mountains: '^'  [Gray / DarkGray]");
        Console.WriteLine("  River: '~'  [Blue / DarkBlue]");
        Console.WriteLine("  Roads/Bridge: '#' and '='  [White / Gray]");
        Console.WriteLine("  Wall: drawn with '\\', '|', '/' and Turrets: '[' and ']'  [DarkGray]");
        Console.WriteLine("  Villages: 'H', Houses: 'h'  [Yellow / White / DarkYellow]");
        Console.WriteLine("  Castles: 'K'  [Magenta / DarkMagenta]");
        Console.WriteLine("  Dungeons: 'D'  [DarkMagenta / Red]");
        Console.WriteLine("  Ruins: 'R'  [Red / DarkRed]");
        Console.WriteLine("  Market: 'M'  [DarkCyan]");
        Console.WriteLine("  Shrine: 'S'  [DarkBlue]");
        Console.WriteLine("  Bank: 'B'  [Cyan]");
        Console.WriteLine("  Library: 'L'  [DarkGreen]");
    }
}
