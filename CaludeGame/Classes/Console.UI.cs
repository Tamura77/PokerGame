namespace CaludeGame.Classes;

public static class ConsoleUI
{
    public static void DisplayCard(Card card)
    {
        string rank = card.Value switch
        {
            1 => "A ",
            10 => "10",
            11 => "J ",
            12 => "Q ",
            13 => "K ",
            _ => card.Value + " "
        };

        string suit = card.Suit switch
        {
            Suit.Heart => "♥",
            Suit.Diamond => "♦",
            Suit.Club => "♣",
            Suit.Spade => "♠",
            _ => "?"
        };

        // Set suit colors
        ConsoleColor color = card.Suit switch
        {
            Suit.Heart => ConsoleColor.Red,
            Suit.Diamond => ConsoleColor.Red,
            Suit.Club => ConsoleColor.Cyan,
            Suit.Spade => ConsoleColor.White,
            _ => ConsoleColor.White
        };

        Console.WriteLine("┌─────┐");
        Console.Write("│ ");
        Console.ForegroundColor = color;
        Console.Write($"{rank}  ");
        Console.ResetColor();
        Console.WriteLine("│");
        Console.Write("│  ");
        Console.ForegroundColor = color;
        Console.Write($"{suit}  ");
        Console.ResetColor();
        Console.WriteLine("│");
        Console.Write("│  ");
        Console.ForegroundColor = color;
        Console.Write($"  {rank}");
        Console.ResetColor();
        Console.WriteLine("│");
        Console.WriteLine("└─────┘");
    }

    public static void DisplayHand(Player player)
    {
        Console.WriteLine($"\n{player.Name}'s Hand:");
        DisplayCardsInRow(player.Hand);
    }

    public static void DisplayCommunityCards(List<Card> cards)
    {
        Console.WriteLine("\n--- Community Cards ---");
        DisplayCardsInRow(cards);
    }

    public static void DisplayCardsInRow(List<Card> cards)
    {
        // Row 1 - top of cards
        foreach (var _ in cards)
            Console.Write("┌─────┐ ");
        Console.WriteLine();

        // Row 2 - rank top left
        foreach (var card in cards)
        {
            string rank = GetRankString(card);
            Console.Write("│");
            SetSuitColor(card);
            Console.Write($"{rank}   ");
            Console.ResetColor();
            Console.Write("│ ");
        }
        Console.WriteLine();

        // Row 3 - suit in middle
        foreach (var card in cards)
        {
            string suit = GetSuitString(card);
            Console.Write("│  ");
            SetSuitColor(card);
            Console.Write(suit);
            Console.ResetColor();
            Console.Write("  │ ");
        }
        Console.WriteLine();

        // Row 4 - rank bottom left (mirrored to match top)
        foreach (var card in cards)
        {
            string rank = GetRankString(card);
            Console.Write("│");
            SetSuitColor(card);
            Console.Write($"{rank}   ");
            Console.ResetColor();
            Console.Write("│ ");
        }
        Console.WriteLine();

        // Row 5 - bottom of cards
        foreach (var _ in cards)
            Console.Write("└─────┘ ");
        Console.WriteLine();
    }

    public static void DisplayGameStatus(List<Player> players, int pot)
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine($"║  {"POT: " + pot,-28}║");
        Console.WriteLine("╠══════════════════════════════╣");
        foreach (var player in players)
            Console.WriteLine($"║  {player.Name + " - Chips: " + player.Chips,-28}║");
        Console.WriteLine("╚══════════════════════════════╝");
    }

    public static void DisplayWinner(List<Player> winners, int pot)
    {
        Console.WriteLine("╔══════════════════════════════╗");
        if (winners.Count > 1)
        {
            Console.WriteLine($"║  {"🤝 TIE GAME!",-28}║");
            Console.WriteLine("╠══════════════════════════════╣");
            foreach (var w in winners)
                Console.WriteLine($"║  {w.Name,-28}║");
        }
        else
        {
            Console.WriteLine($"║  {"🏆 WINNER: " + winners[0].Name,-28}║");
        }
        Console.WriteLine($"║  {"Pot: " + pot,-28}║");
        Console.WriteLine("╚══════════════════════════════╝");
    }

    public static void DisplayRoundTitle(string title)
    {
        Console.WriteLine($"\n══ {title} ══\n");
    }

    private static string GetRankString(Card card) => card.Value switch
    {
        1 => "A ",
        10 => "10",
        11 => "J ",
        12 => "Q ",
        13 => "K ",
        _ => card.Value.ToString() + " "
    };

    private static string GetSuitString(Card card) => card.Suit switch
    {
        Suit.Heart => "♥",
        Suit.Diamond => "♦",
        Suit.Club => "♣",
        Suit.Spade => "♠",
        _ => "?"
    };

    private static void SetSuitColor(Card card)
    {
        Console.ForegroundColor = card.Suit switch
        {
            Suit.Heart => ConsoleColor.Red,
            Suit.Diamond => ConsoleColor.Red,
            Suit.Club => ConsoleColor.Cyan,
            Suit.Spade => ConsoleColor.White,
            _ => ConsoleColor.White
        };
    }
}