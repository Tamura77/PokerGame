namespace CaludeGame.Classes;

public static class HandEvaluator
{
    // Normalise Ace from 1 to 14 for all comparisons
    private static int NormaliseValue(int value) => value == 1 ? 14 : value;

    private static List<int> NormaliseValues(List<Card> cards)
        => cards.Select(c => NormaliseValue(c.Value)).OrderBy(v => v).ToList();

    public static HandRank Evaluate(List<Card> hand)
    {
        var values = NormaliseValues(hand);
        var suits = hand.Select(c => c.Suit).ToList();

        bool isFlush = suits.Distinct().Count() == 1;
        bool isStraight = CheckStraight(values);

        var groups = values
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .ToList();

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

    private static bool CheckStraight(List<int> values)
    {
        // Standard straight check
        if (values.Last() - values.First() == 4 && values.Distinct().Count() == 5)
            return true;

        // Ace-low straight: A 2 3 4 5 (Ace counted as 1)
        // values will be [2, 3, 4, 5, 14] after normalisation
        var distinctVals = values.Distinct().ToList();
        if (distinctVals.Contains(14))
        {
            var aceLow = distinctVals
                .Select(v => v == 14 ? 1 : v)
                .OrderBy(v => v)
                .ToList();
            if (aceLow.Last() - aceLow.First() == 4 && aceLow.Distinct().Count() == 5)
                return true;
        }

        return false;
    }

    private static List<int> GetTiebreakerValues(List<Card> hand)
    {
        var groups = hand
            .GroupBy(c => NormaliseValue(c.Value))
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .ToList();

        return groups.SelectMany(g => Enumerable.Repeat(g.Key, g.Count())).ToList();
    }

    private static int CompareHands(List<Card> hand1, List<Card> hand2)
    {
        var tb1 = GetTiebreakerValues(hand1);
        var tb2 = GetTiebreakerValues(hand2);

        for (int i = 0; i < tb1.Count; i++)
        {
            if (tb1[i] > tb2[i]) return 1;
            if (tb1[i] < tb2[i]) return -1;
        }
        return 0;
    }

    public static HandRank EvaluateBest(List<Card> cards)
    {
        var best = HandRank.HighCard;
        var combos = GetCombinations(cards, 5);
        foreach (var combo in combos)
        {
            var rank = Evaluate(combo);
            if (rank > best) best = rank;
        }
        return best;
    }

    public static List<Player> DetermineWinners(List<Player> players)
    {
        var bestRank = players.Max(p => Evaluate(p.Hand));
        var candidates = players.Where(p => Evaluate(p.Hand) == bestRank).ToList();

        var winners = new List<Player> { candidates[0] };
        foreach (var player in candidates.Skip(1))
        {
            int result = CompareHands(player.Hand, winners[0].Hand);
            if (result > 0)
                winners = new List<Player> { player };
            else if (result == 0)
                winners.Add(player);
        }
        return winners;
    }

    public static List<Player> DetermineWinners(List<Player> players, List<Card> communityCards)
    {
        HandRank bestRank = HandRank.HighCard;

        foreach (var player in players)
        {
            var fullHand = player.Hand.Concat(communityCards).ToList();
            var rank = EvaluateBest(fullHand);
            if (rank > bestRank) bestRank = rank;
        }

        var candidates = players.Where(p =>
            EvaluateBest(p.Hand.Concat(communityCards).ToList()) == bestRank).ToList();

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
}