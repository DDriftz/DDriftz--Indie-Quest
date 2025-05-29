﻿
﻿using System;
using System.ComponentModel.DataAnnotations;
using static enums.TheGame;

namespace enums
{
    internal class Program
    {
        static void Main(string[] args)
        {
          TheGame theGame = new TheGame();
            theGame.DrawAce(Suits.Spades);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    theGame.DrawCard((Suits)i, j);
                }
            }

        }
    }
    public class TheGame
    {
        public void DrawAce(Suits suit)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            switch (suit)
            {
                case Suits.Hearts:
                    Console.WriteLine("╭───────╮\r\n│A      │\r\n│♥      │\r\n│   ♥   │\r\n│      ♥│\r\n│      A│\r\n╰───────╯\r\n");
                    break;
                case Suits.Clubs:
                    Console.WriteLine("╭───────╮\r\n│A      │\r\n│♣      │\r\n│   ♣   │\r\n│      ♣│\r\n│      A│\r\n╰───────╯\r\n");
                    break;
                case Suits.Spades:
                    Console.WriteLine("╭───────╮\r\n│A      │\r\n│♠      │\r\n│   ♠   │\r\n│      ♠│\r\n│      A│\r\n╰───────╯\r\n");
                    break;
                case Suits.Diamonds:
                    Console.WriteLine("╭───────╮\r\n│A      │\r\n│♦      │\r\n│   ♦   │\r\n│      ♦│\r\n│      A│\r\n╰───────╯\r\n");
                    break;
            }
        }
        public void DrawCard(Suits suit, int rank)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            String[] cardNumbers = {
               "╭─────────╮\r\n│A        │\r\n│♠        │\r\n│         │\r\n│    ♠    │\r\n│         │\r\n│        ♠│\r\n│        A│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│2   ♠    │\r\n│♠        │\r\n│         │\r\n│         │\r\n│         │\r\n│        ♠│\r\n│    ♠   2│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│3   ♠    │\r\n│♠        │\r\n│         │\r\n│    ♠    │\r\n│         │\r\n│        ♠│\r\n│    ♠   3│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│4 ♠   ♠  │\r\n│♠        │\r\n│         │\r\n│         │\r\n│         │\r\n│        ♠│\r\n│  ♠   ♠ 4│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│5 ♠   ♠  │\r\n│♠        │\r\n│         │\r\n│    ♠    │\r\n│         │\r\n│        ♠│\r\n│  ♠   ♠ 5│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│6 ♠   ♠  │\r\n│♠        │\r\n│         │\r\n│  ♠   ♠  │\r\n│         │\r\n│        ♠│\r\n│  ♠   ♠ 6│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│7 ♠   ♠  │\r\n│♠        │\r\n│    ♠    │\r\n│  ♠   ♠  │\r\n│         │\r\n│        ♠│\r\n│  ♠   ♠ 7│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│8 ♠   ♠  │\r\n│♠        │\r\n│    ♠    │\r\n│  ♠   ♠  │\r\n│    ♠    │\r\n│        ♠│\r\n│  ♠   ♠ 8│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│9 ♠   ♠  │\r\n│♠        │\r\n│  ♠   ♠  │\r\n│    ♠    │\r\n│  ♠   ♠  │\r\n│        ♠│\r\n│  ♠   ♠ 9│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│10♠   ♠  │\r\n│♠   ♠    │\r\n│  ♠   ♠  │\r\n│         │\r\n│  ♠   ♠  │\r\n│    ♠   ♠│\r\n│  ♠   ♠10│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│J┌─────┐ │\r\n│♠│♠\\__/│ │\r\n│ │|(_/|│ │\r\n│ │} / {│ │\r\n│ │|/_)|│ │\r\n│ │/  \\♠│♠│\r\n│ └─────┘J│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│Q┌─────┐ │\r\n│♠│♠(_(/│ │\r\n│ │  )/❀│ │\r\n│ │{ / }│ │\r\n│ │❀/(  │ │\r\n│ │/) )♠│♠│\r\n│ └─────┘Q│\r\n╰─────────╯\r\n",
               "╭─────────╮\r\n│K┌─────┐ │\r\n│♠│♠\\__/│ │\r\n│ │ (_/|│ │\r\n│ │+ / +│ │\r\n│ │|/_) │ │\r\n│ │/  \\♠│♠│\r\n│ └─────┘K│\r\n╰─────────╯\r\n"
            };

            // replacing the suit icon in the string
            Console.WriteLine(cardNumbers[rank].Replace("♠", getStringACE(suit)));
        }
        
        private string getStringACE(Suits suit)
        {
            switch (suit)
            {
                case Suits.Hearts:
                    return "♥";
                case Suits.Clubs:
                    return "♣";
                case Suits.Spades:
                    return "♠";
                case Suits.Diamonds:
                    return "♦";
                default:
                    Console.WriteLine("ERROR! Dumbass typed wrong number");
                    return "ERROR!";
            }
        }

        public enum Suits
    {
        Hearts = 0,
        Clubs = 1,
        Spades = 2,
        Diamonds = 3,
    }
    }
}
