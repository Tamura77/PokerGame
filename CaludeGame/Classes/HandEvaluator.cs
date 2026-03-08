namespace CaludeGame.Classes;

public static class HandEvaluator
{
    public static HandRank Evaluate(List<Card> hand)
    {
        var values = hand.Select(c => c.Value).OrderBy(v => v).ToList();
        var suits = hand.Select(c => c.Suit).ToList();

        bool isFlush = suits.Distinct().Count() == 1;
        bool isStraight = values.Last() - values.First() == 4 && values.Distinct().Count() == 5;

        var groups = values.GroupBy(v => v).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).ToList();
        int firstGroupCount = groups[0].Count();
        int secondGroupCount = groups.Count > 1 ? groups[1].Count() : 0;

        if (isFlush && isStraight) return HandRank.StraightFlush;
        if (firstGroupCount == 4) return HandRank.FourOfAKind;
        if (firstGroupCount == 3 && secondGroupCount == 2) return HandRank.FullHouse;
        if (isFlush) return HandRank.Flush;
        if (isStraight) return HandRank.Straight;
        if (firstGroupCount == 3) return HandRank.ThreeOfAKind;
        if (firstGroupCount == 2 && secondGroupCount == 2) return HandRank.TwoPair;
        if (firstGroupCount == 2) return HandRank.OnePair;

        return HandRank.HighCard;
    }
    // Add this to HandEvaluator.cs
    public static HandRank EvaluateBest(List<Card> cards)
    {
        // Get all combinations of 5 cards from 7
        var best = HandRank.HighCard;
        var combos = GetCombinations(cards, 5);
        foreach (var combo in combos)
        {
            var rank = Evaluate(combo);
            if (rank > best) best = rank;
        }
        return best;
    }

    private static IEnumerable<List<Card>> GetCombinations(List<Card> cards, int k)
    {
        if (k == 0) yield return new List<Card>();
        else
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var rest = cards.Skip(i + 1).ToList();
                foreach (var combo in GetCombinations(rest, k - 1))
                {
                    combo.Insert(0, cards[i]);
                    yield return combo;
                }
            }
        }
    }

    // Returns cards sorted by importance for tiebreaking
    // e.g. for OnePair: the pair value first, then kickers descending
    private static List<int> GetTiebreakerValues(List<Card> hand)
    {
        var groups = hand
            .GroupBy(c => c.Value)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .ToList();

        // Flatten: grouped values first (pairs, trips etc), then kickers
        return groups.SelectMany(g => Enumerable.Repeat(g.Key, g.Count())).ToList();
    }

    // Compares two hands of the same rank, returns 1 if hand1 wins, -1 if hand2 wins, 0 if tie
    private static int CompareHands(List<Card> hand1, List<Card> hand2)
    {
        var tb1 = GetTiebreakerValues(hand1);
        var tb2 = GetTiebreakerValues(hand2);

        for (int i = 0; i < tb1.Count; i++)
        {
            if (tb1[i] > tb2[i]) return 1;
            if (tb1[i] < tb2[i]) return -1;
        }
        return 0; // true tie
    }

    public static List<Player> DetermineWinners(List<Player> players, List<Card> communityCards)
    {
        HandRank bestRank = HandRank.HighCard;

        // Find best rank across all players using full 7 card hand
        foreach (var player in players)
        {
            var fullHand = player.Hand.Concat(communityCards).ToList();
            var rank = EvaluateBest(fullHand);
            if (rank > bestRank) bestRank = rank;
        }

        // Find all players with that rank
        var candidates = players.Where(p =>
            EvaluateBest(p.Hand.Concat(communityCards).ToList()) == bestRank).ToList();

        // Tiebreak among candidates
        var winners = new List<Player> { candidates[0] };
        foreach (var player in candidates.Skip(1))
        {
            var hand1 = player.Hand.Concat(communityCards).ToList();
            var hand2 = winners[0].Hand.Concat(communityCards).ToList();
            int result = CompareHands(hand1, hand2);
            if (result > 0)
                winners = new List<Player> { player };
            else if (result == 0)
                winners.Add(player);
        }

        return winners;
    }
}