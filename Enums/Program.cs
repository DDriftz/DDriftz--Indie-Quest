using System;
using System.Text;

enum Suit
{
    Heart,
    Spade,
    Diamond,
    Club
}

class CardDrawer
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Test DrawAce with each suit
        DrawAce(Suit.Spade);
        Console.WriteLine();
        DrawAce(Suit.Heart);
        Console.WriteLine();

        // Draw full suit from Ace (1) to King (13)
        for (int rank = 1; rank <= 13; rank++)
        {
            DrawCard(Suit.Spade, rank);
            Console.WriteLine();
        }
    }

    static void DrawAce(Suit suit)
    {
        char symbol = GetSuitSymbol(suit);

        Console.WriteLine("╭───────╮");
        Console.WriteLine($"│A      │");
        Console.WriteLine($"│{symbol}      │");
        Console.WriteLine($"│   {symbol}   │");
        Console.WriteLine($"│      {symbol}│");
        Console.WriteLine($"│      A│");
        Console.WriteLine("╰───────╯");
    }

    static void DrawCard(Suit suit, int rank)
    {
        string rankStr = GetRankLabel(rank);
        char symbol = GetSuitSymbol(suit);

        string[] template = new string[9];
        template[0] = "╭─────────╮";

        if (rank == 1) // Ace
        {
            template[1] = $"│{rankStr,-2}       │";
            template[2] = $"│{symbol}         │";
            template[3] = $"│         │";
            template[4] = $"│    {symbol}    │";
            template[5] = $"│         │";
            template[6] = $"│        {symbol}│";
            template[7] = $"│       {rankStr,2}│";
        }
        else if (rank >= 2 && rank <= 10)
        {
            // Create symbol positions based on rank (basic symmetrical layout)
            var grid = new string[5];
            for (int i = 0; i < grid.Length; i++) grid[i] = "         ";

            // Place symbols for numeric ranks (simplified symmetrical layout)
            List<(int, int)> positions = GetSymbolPositions(rank);
            char[,] matrix = new char[5, 9];
            for (int r = 0; r < 5; r++)
                for (int c = 0; c < 9; c++)
                    matrix[r, c] = ' ';

            foreach (var (r, c) in positions)
                if (r >= 0 && r < 5 && c >= 0 && c < 9)
                    matrix[r, c] = symbol;

            template[1] = $"│{rankStr,-2}{matrix[0, 4]}   {matrix[0, 8]}│";
            template[2] = $"│{matrix[1, 0]}         │";
            template[3] = $"│  {matrix[2, 3]}   {matrix[2, 5]}  │";
            template[4] = $"│    {matrix[3, 4]}    │";
            template[5] = $"│         {matrix[4, 8]}│";
            template[6] = $"│  {matrix[4, 3]}   {matrix[4, 5]} {rankStr,2}│";
            template[7] = $"│         │";
        }
        else // J, Q, K
        {
            string face = rankStr;
            template[1] = $"│{face}┌─────┐ │";
            template[2] = $"│{symbol}│{symbol}\\__/│ │";
            template[3] = $"│ │|(_/|│ │";
            template[4] = $"│ │}} / {{│ │";
            template[5] = $"│ │|/_)|│ │";
            template[6] = $"│ │/  \\{symbol}│{symbol}│";
            template[7] = $"│ └─────┘{face}│";
        }

        template[8] = "╰─────────╯";

        foreach (var line in template)
            Console.WriteLine(line);
    }

    static string GetRankLabel(int rank)
    {
        return rank switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => rank.ToString()
        };
    }

    static char GetSuitSymbol(Suit suit)
    {
        return suit switch
        {
            Suit.Heart => '♥',
            Suit.Spade => '♠',
            Suit.Diamond => '♦',
            Suit.Club => '♣',
            _ => '?'
        };
    }

    static List<(int, int)> GetSymbolPositions(int rank)
    {
        var positions = new List<(int, int)>();
        switch (rank)
        {
            case 2: positions.AddRange([(1, 0), (4, 8)]); break;
            case 3: positions.AddRange([(1, 0), (2, 4), (4, 8)]); break;
            case 4: positions.AddRange([(1, 0), (1, 8), (4, 0), (4, 8)]); break;
            case 5: positions.AddRange([(1, 0), (1, 8), (2, 4), (4, 0), (4, 8)]); break;
            case 6: positions.AddRange([(1, 0), (1, 8), (3, 0), (3, 8), (4, 0), (4, 8)]); break;
            case 7: positions.AddRange([(1, 0), (1, 8), (2, 4), (3, 0), (3, 8), (4, 0), (4, 8)]); break;
            case 8: positions.AddRange([(1, 0), (1, 8), (2, 4), (3, 0), (3, 8), (4, 0), (4, 8), (0, 4)]); break;
            case 9: positions.AddRange([(0, 4), (1, 0), (1, 8), (2, 2), (2, 6), (3, 0), (3, 8), (4, 2), (4, 6)]); break;
            case 10: positions.AddRange([(0, 1), (0, 4), (0, 7), (1, 0), (1, 8), (3, 1), (3, 4), (3, 7), (4, 0), (4, 8)]); break;
        }
        return positions;
    }
}
