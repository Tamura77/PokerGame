namespace CaludeGame.Classes;

public static class AIDecision
{
    private static readonly Random _rng = new Random();

    public static string MakeDecision(Player player, List<Card> communityCards, int amountOwed, int pot, int alreadyInvested, int minRaise)
    {
        double strength = EvaluateHandStrength(player, communityCards);
        double drawStrength = communityCards.Count > 0 ? EvaluateDrawStrength(player.Hand, communityCards) : 0;

        double variance = (_rng.NextDouble() * 0.1) - 0.05;
        strength = Math.Clamp(strength + variance, 0.0, 1.0);

        Console.WriteLine($"\n{player.Name} is thinking...");
        System.Threading.Thread.Sleep(800);

        bool isPreFlop = communityCards.Count == 0;
        bool hasDraw = drawStrength >= 0.40;
        double callRatio = player.Chips > 0 ? (double)amountOwed / player.Chips : 1.0;

        // ── Strong hand — raise aggressively ──────────────────────────────
        if (strength >= 0.60)
        {
            int raiseAmount = CalculateRaise(strength, pot, amountOwed, player.Chips, minRaise);
            if (raiseAmount > amountOwed && player.Chips >= raiseAmount)
            {
                Console.WriteLine($"{player.Name} raises to {raiseAmount}.");
                return $"raise:{raiseAmount}";
            }
        }

        // ── Pre-flop: defend blind investment ─────────────────────────────
        // If already invested (blind) and cost to call is small, protect it
        if (isPreFlop && alreadyInvested > 0 && amountOwed > 0)
        {
            double defenseRatio = (double)amountOwed / (alreadyInvested + amountOwed);
            if (defenseRatio <= 0.5 && strength >= 0.30)
            {
                Console.WriteLine($"{player.Name} calls {amountOwed} (defending blind).");
                return "call";
            }
        }

        // ── Pre-flop: see the flop cheaply ───────────────────────────────
        // Even with a mediocre hand, call small bets to see the flop
        if (isPreFlop && amountOwed > 0 && strength >= 0.35)
        {
            if (callRatio <= 0.08) // very cheap relative to stack
            {
                Console.WriteLine($"{player.Name} calls {amountOwed} (cheap flop).");
                return "call";
            }
        }

        // ── Draw hand — check or call cheaply to see next card ────────────
        if (hasDraw && !isPreFlop)
        {
            if (amountOwed == 0)
            {
                Console.WriteLine($"{player.Name} checks (on a draw).");
                return "check";
            }

            // Pot odds — worth calling if draw is strong enough
            double potOdds = (double)amountOwed / (pot + amountOwed);
            if (drawStrength > potOdds && callRatio <= 0.25)
            {
                Console.WriteLine($"{player.Name} calls {amountOwed} (chasing draw).");
                return "call";
            }
        }

        // ── Medium hand — call if affordable ─────────────────────────────
        if (strength >= 0.25)
        {
            if (amountOwed == 0)
            {
                Console.WriteLine($"{player.Name} checks.");
                return "check";
            }
            if (callRatio <= 0.5)
            {
                Console.WriteLine($"{player.Name} calls {amountOwed}.");
                return "call";
            }
        }

        // ── Weak hand — check or fold ─────────────────────────────────────
        if (amountOwed == 0)
        {
            Console.WriteLine($"{player.Name} checks.");
            return "check";
        }

        Console.WriteLine($"{player.Name} folds.");
        return "fold";
    }

    // Returns 0.0-1.0 score for draw potential (flush draw, straight draw etc)
    private static double EvaluateDrawStrength(List<Card> holeCards, List<Card> communityCards)
    {
        var allCards = holeCards.Concat(communityCards).ToList();
        var values = allCards.Select(c => c.Value == 1 ? 14 : c.Value).Distinct().OrderBy(v => v).ToList();
        var suits = allCards.Select(c => c.Suit).ToList();

        double drawScore = 0.0;

        // ── Flush draw — 4 cards of same suit ─────────────────────────────
        var suitGroups = suits.GroupBy(s => s).ToList();
        int maxSuited = suitGroups.Max(g => g.Count());
        bool holeContributesToFlush = suitGroups
            .Where(g => g.Count() >= 4)
            .Any(g => holeCards.Any(c => c.Suit == g.Key));

        if (maxSuited >= 4 && holeContributesToFlush)
            drawScore = Math.Max(drawScore, 0.70); // flush draw
        else if (maxSuited >= 3 && holeContributesToFlush)
            drawScore = Math.Max(drawScore, 0.30); // backdoor flush draw

        // ── Straight draw — open ended or gutshot ─────────────────────────
        // Add Ace-low
        if (values.Contains(14) && !values.Contains(1))
            values.Add(1);

        int maxConsecutive = 1;
        int consecutive = 1;
        int openEndedDraws = 0;
        int gutshots = 0;

        for (int i = 1; i < values.Count; i++)
        {
            int diff = values[i] - values[i - 1];
            if (diff == 1)
            {
                consecutive++;
                maxConsecutive = Math.Max(maxConsecutive, consecutive);
            }
            else if (diff == 2)
            {
                gutshots++; // one gap in sequence
                consecutive = 1;
            }
            else
            {
                consecutive = 1;
            }
        }

        // Check if hole cards are part of the draw sequence
        bool holeInSequence = holeCards.Any(c =>
        {
            int v = c.Value == 1 ? 14 : c.Value;
            return values.Contains(v - 1) || values.Contains(v + 1) ||
                   values.Contains(v - 2) || values.Contains(v + 2);
        });

        if (maxConsecutive >= 4 && holeInSequence)
            drawScore = Math.Max(drawScore, 0.65); // open ended straight draw
        else if (maxConsecutive >= 3 && gutshots > 0 && holeInSequence)
            drawScore = Math.Max(drawScore, 0.45); // gutshot straight draw
        else if (maxConsecutive >= 3 && holeInSequence)
            drawScore = Math.Max(drawScore, 0.35); // backdoor straight draw

        return drawScore;
    }

    private static double LookupPreflopStrength(int high, int low, bool suited)
    {
        if (high == 1) high = 14;
        if (low == 1) low = 14;
        if (high < low) (high, low) = (low, high);

        bool isPair = high == low;

        if (isPair)
        {
            return high switch
            {
                14 => 0.95,
                13 => 0.90,
                12 => 0.86,
                11 => 0.82,
                10 => 0.78,
                9 => 0.72,
                8 => 0.67,
                7 => 0.62,
                6 => 0.56,
                5 => 0.52,
                4 => 0.48,
                3 => 0.45,
                2 => 0.42,
                _ => 0.42
            };
        }

        if (high == 14)
        {
            return (low, suited) switch
            {
                (13, true) => 0.88,
                (13, false) => 0.83,
                (12, true) => 0.84,
                (12, false) => 0.78,
                (11, true) => 0.80,
                (11, false) => 0.74,
                (10, true) => 0.77,
                (10, false) => 0.71,
                (9, true) => 0.72,
                (9, false) => 0.65,
                (8, true) => 0.69,
                (8, false) => 0.62,
                (7, true) => 0.66,
                (7, false) => 0.59,
                (6, true) => 0.63,
                (6, false) => 0.56,
                (5, true) => 0.62,
                (5, false) => 0.55,
                (4, true) => 0.60,
                (4, false) => 0.53,
                (3, true) => 0.58,
                (3, false) => 0.51,
                (2, true) => 0.56,
                (2, false) => 0.49,
                _ => 0.49
            };
        }

        if (high == 13)
        {
            return (low, suited) switch
            {
                (12, true) => 0.78,
                (12, false) => 0.72,
                (11, true) => 0.74,
                (11, false) => 0.68,
                (10, true) => 0.71,
                (10, false) => 0.65,
                (9, true) => 0.65,
                (9, false) => 0.58,
                (8, true) => 0.61,
                (8, false) => 0.54,
                (7, true) => 0.58,
                (7, false) => 0.51,
                (6, true) => 0.55,
                (6, false) => 0.48,
                (5, true) => 0.53,
                (5, false) => 0.46,
                (4, true) => 0.51,
                (4, false) => 0.44,
                (3, true) => 0.49,
                (3, false) => 0.42,
                (2, true) => 0.47,
                (2, false) => 0.40,
                _ => 0.40
            };
        }

        if (high == 12)
        {
            return (low, suited) switch
            {
                (11, true) => 0.72,
                (11, false) => 0.66,
                (10, true) => 0.69,
                (10, false) => 0.63,
                (9, true) => 0.63,
                (9, false) => 0.56,
                (8, true) => 0.58,
                (8, false) => 0.51,
                (7, true) => 0.54,
                (7, false) => 0.47,
                (6, true) => 0.51,
                (6, false) => 0.44,
                (5, true) => 0.49,
                (5, false) => 0.42,
                (4, true) => 0.47,
                (4, false) => 0.40,
                (3, true) => 0.45,
                (3, false) => 0.38,
                (2, true) => 0.43,
                (2, false) => 0.36,
                _ => 0.36
            };
        }

        if (high == 11)
        {
            return (low, suited) switch
            {
                (10, true) => 0.67,
                (10, false) => 0.61,
                (9, true) => 0.61,
                (9, false) => 0.54,
                (8, true) => 0.56,
                (8, false) => 0.49,
                (7, true) => 0.51,
                (7, false) => 0.44,
                (6, true) => 0.47,
                (6, false) => 0.40,
                (5, true) => 0.45,
                (5, false) => 0.38,
                (4, true) => 0.43,
                (4, false) => 0.36,
                (3, true) => 0.41,
                (3, false) => 0.34,
                (2, true) => 0.39,
                (2, false) => 0.32,
                _ => 0.32
            };
        }

        if (high == 10)
        {
            return (low, suited) switch
            {
                (9, true) => 0.62,
                (9, false) => 0.55,
                (8, true) => 0.57,
                (8, false) => 0.50,
                (7, true) => 0.51,
                (7, false) => 0.44,
                (6, true) => 0.46,
                (6, false) => 0.39,
                (5, true) => 0.43,
                (5, false) => 0.36,
                (4, true) => 0.41,
                (4, false) => 0.34,
                (3, true) => 0.39,
                (3, false) => 0.32,
                (2, true) => 0.37,
                (2, false) => 0.30,
                _ => 0.30
            };
        }

        int gap = high - low - 1;
        if (suited && gap == 0) return high switch { 9 => 0.57, 8 => 0.53, 7 => 0.49, 6 => 0.45, 5 => 0.41, _ => 0.38 };
        if (suited && gap == 1) return high switch { 9 => 0.51, 8 => 0.47, 7 => 0.43, 6 => 0.39, 5 => 0.36, _ => 0.33 };
        if (!suited && gap == 0) return high switch { 9 => 0.49, 8 => 0.45, 7 => 0.41, 6 => 0.37, 5 => 0.33, _ => 0.30 };
        if (suited) return Math.Max(0.30, 0.20 + (high / 50.0));
        return Math.Max(0.15, 0.10 + (high / 60.0));
    }

    private static double EvaluateHandStrength(Player player, List<Card> communityCards)
    {
        if (communityCards.Count == 0)
        {
            var h1 = player.Hand[0].Value == 1 ? 14 : player.Hand[0].Value;
            var h2 = player.Hand[1].Value == 1 ? 14 : player.Hand[1].Value;
            bool suited = player.Hand[0].Suit == player.Hand[1].Suit;
            return LookupPreflopStrength(h1, h2, suited);
        }
        return EvaluateWithContext(player.Hand, communityCards);
    }

    private static double EvaluateWithContext(List<Card> holeCards, List<Card> communityCards)
    {
        var allCards = holeCards.Concat(communityCards).ToList();
        var rank = HandEvaluator.EvaluateBest(allCards);

        bool holeCardsContribute = DoHoleCardsContribute(holeCards, communityCards, rank);

        double baseScore = rank switch
        {
            HandRank.StraightFlush => 1.00,
            HandRank.FourOfAKind => 0.95,
            HandRank.FullHouse => 0.85,
            HandRank.Flush => 0.75,
            HandRank.Straight => 0.65,
            HandRank.ThreeOfAKind => 0.55,
            HandRank.TwoPair => 0.45,
            HandRank.OnePair => 0.35,
            HandRank.HighCard => 0.15,
            _ => 0.10
        };

        if (!holeCardsContribute)
            baseScore *= 0.40;

        bool bothContribute = BothHoleCardsContribute(holeCards, communityCards, rank);
        if (bothContribute)
            baseScore = Math.Min(baseScore * 1.20, 1.0);

        return baseScore;
    }

    private static bool DoHoleCardsContribute(List<Card> holeCards, List<Card> communityCards, HandRank rank)
    {
        var holeValues = holeCards.Select(c => c.Value == 1 ? 14 : c.Value).ToList();
        var holeSuits = holeCards.Select(c => c.Suit).ToList();

        switch (rank)
        {
            case HandRank.OnePair:
            case HandRank.TwoPair:
            case HandRank.ThreeOfAKind:
            case HandRank.FullHouse:
            case HandRank.FourOfAKind:
                var allVals = holeCards.Concat(communityCards).Select(c => c.Value == 1 ? 14 : c.Value);
                var pairedVals = allVals.GroupBy(v => v).Where(g => g.Count() > 1).Select(g => g.Key);
                return holeValues.Any(v => pairedVals.Contains(v));

            case HandRank.Flush:
                var flushSuit = holeCards.Concat(communityCards)
                    .GroupBy(c => c.Suit)
                    .FirstOrDefault(g => g.Count() >= 5)?.Key;
                return flushSuit.HasValue && holeSuits.Contains(flushSuit.Value);

            case HandRank.Straight:
                var straightVals = GetStraightValues(holeCards.Concat(communityCards).ToList());
                return holeValues.Any(v => straightVals.Contains(v));

            case HandRank.StraightFlush:
                return true;

            default:
                var boardVals = communityCards.Select(c => c.Value == 1 ? 14 : c.Value).ToList();
                int boardHigh = boardVals.Any() ? boardVals.Max() : 0;
                return holeValues.Max() > boardHigh;
        }
    }

    private static bool BothHoleCardsContribute(List<Card> holeCards, List<Card> communityCards, HandRank rank)
    {
        var holeValues = holeCards.Select(c => c.Value == 1 ? 14 : c.Value).ToList();

        switch (rank)
        {
            case HandRank.TwoPair:
            case HandRank.FullHouse:
            case HandRank.ThreeOfAKind:
                var allVals = holeCards.Concat(communityCards).Select(c => c.Value == 1 ? 14 : c.Value);
                var grouped = allVals.GroupBy(v => v).Where(g => g.Count() >= 2).Select(g => g.Key);
                return holeValues.All(v => grouped.Contains(v));

            case HandRank.Straight:
                var straightVals = GetStraightValues(holeCards.Concat(communityCards).ToList());
                return holeValues.All(v => straightVals.Contains(v));

            case HandRank.Flush:
                var flushSuit = holeCards.Concat(communityCards)
                    .GroupBy(c => c.Suit)
                    .FirstOrDefault(g => g.Count() >= 5)?.Key;
                return flushSuit.HasValue && holeCards.All(c => c.Suit == flushSuit.Value);

            default:
                return false;
        }
    }

    private static HashSet<int> GetStraightValues(List<Card> cards)
    {
        var values = cards.Select(c => c.Value == 1 ? 14 : c.Value).Distinct().OrderBy(v => v).ToList();
        if (values.Contains(14)) values.Add(1);

        for (int i = 0; i <= values.Count - 5; i++)
        {
            var window = values.Skip(i).Take(5).ToList();
            if (window.Last() - window.First() == 4 && window.Distinct().Count() == 5)
                return new HashSet<int>(window);
        }
        return new HashSet<int>();
    }

    private static int CalculateRaise(double strength, int pot, int amountOwed, int chips, int minRaise)
    {
        double multiplier = strength >= 0.90 ? 0.75 :
                            strength >= 0.70 ? 0.50 : 0.30;
        int raise = (int)(pot * multiplier);
        raise = Math.Max(raise, minRaise);  // ← respect min raise
        raise = Math.Max(raise, amountOwed * 2);
        raise = Math.Min(raise, chips);
        return raise;
    }
}