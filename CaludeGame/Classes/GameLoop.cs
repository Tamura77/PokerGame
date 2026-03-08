namespace CaludeGame.Classes;

public class GameLoop
{
    private Deck _deck;
    private List<Player> _players;
    private BettingManager _betting;
    private List<Card> _communityCards;
    private List<Player> _activePlayers;

    public GameLoop(List<Player> players)
    {
        _players = players;
        _betting = new BettingManager();
        _communityCards = new List<Card>();
        _activePlayers = new List<Player>();
        _deck = new Deck(true);
    }

    public void StartRound()
    {
        Console.Clear();
        _deck = new Deck(true);
        _communityCards.Clear();
        _betting.Reset();
        _activePlayers = new List<Player>(_players);

        foreach (var player in _players)
            player.ClearHand();

        Console.WriteLine("=== NEW ROUND ===\n");

        ConsoleUI.DisplayRoundTitle("Posting Blinds");
        _betting.PostBlind(_players[0], 10);
        _betting.PostBlind(_players[1], 20);

        DealHoleCards();
        if (!BettingRound("Pre-Flop")) return;

        DealCommunityCards(3);
        if (!BettingRound("Flop")) return;

        DealCommunityCards(1);
        if (!BettingRound("Turn")) return;

        DealCommunityCards(1);
        if (!BettingRound("River")) return;

        Showdown();
    }

    private void DealHoleCards()
    {
        ConsoleUI.DisplayRoundTitle("Dealing Hole Cards");
        foreach (var player in _players)
        {
            player.AddCard(_deck.Deal());
            player.AddCard(_deck.Deal());
            ConsoleUI.DisplayHand(player);
        }
    }

    private void DealCommunityCards(int count)
    {
        for (int i = 0; i < count; i++)
            _communityCards.Add(_deck.Deal());
        ConsoleUI.DisplayCommunityCards(_communityCards);
    }

    private bool BettingRound(string roundName)
    {
        ConsoleUI.DisplayRoundTitle($"{roundName} Betting");
        ConsoleUI.DisplayGameStatus(_activePlayers, _betting.Pot);

        var roundBets = _activePlayers.ToDictionary(p => p, p => 0);
        var playersToAct = new Queue<Player>(_activePlayers);

        while (playersToAct.Count > 0)
        {
            var player = playersToAct.Dequeue();

            if (!_activePlayers.Contains(player))
                continue;

            int amountOwed = _betting.CurrentBet - roundBets[player];

            Console.WriteLine($"\n{player.Name}'s turn (Chips: {player.Chips}, Pot: {_betting.Pot})");

            if (amountOwed == 0)
                Console.WriteLine("1) Check  2) Raise  3) Fold");
            else
                Console.WriteLine($"1) Call {amountOwed}  2) Raise  3) Fold");

            Console.Write("Choose: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    if (amountOwed == 0)
                    {
                        Console.WriteLine($"{player.Name} checks.");
                    }
                    else
                    {
                        _betting.Call(player);
                        roundBets[player] = _betting.CurrentBet;
                    }
                    break;

                case "2":
                    Console.Write("Raise amount: ");
                    if (int.TryParse(Console.ReadLine(), out int amount))
                    {
                        if (_betting.Raise(player, amount))
                        {
                            roundBets[player] = amount;
                            foreach (var other in _activePlayers)
                            {
                                if (other != player && !playersToAct.Contains(other))
                                    playersToAct.Enqueue(other);
                            }
                        }
                    }
                    break;

                case "3":
                    _betting.Fold(player);
                    _activePlayers.Remove(player);
                    roundBets.Remove(player);

                    if (_activePlayers.Count == 1)
                    {
                        Console.WriteLine($"\n{_activePlayers[0].Name} wins the pot!");
                        _betting.AwardPot(_activePlayers);
                        ConsoleUI.DisplayGameStatus(_players, _betting.Pot);
                        return false;
                    }
                    break;

                default:
                    Console.WriteLine("Invalid input, checking.");
                    if (amountOwed == 0)
                    {
                        Console.WriteLine($"{player.Name} checks.");
                    }
                    else
                    {
                        _betting.Call(player);
                        roundBets[player] = _betting.CurrentBet;
                    }
                    break;
            }
        }
        return true;
    }

    private void Showdown()
    {
        if (_activePlayers.Count == 1)
            return;

        ConsoleUI.DisplayRoundTitle("SHOWDOWN");

        foreach (var player in _activePlayers)
        {
            var fullHand = player.Hand.Concat(_communityCards).ToList();
            var rank = HandEvaluator.EvaluateBest(fullHand);
            ConsoleUI.DisplayHand(player);
            Console.WriteLine($"  Hand Rank: {rank}");
        }

        ConsoleUI.DisplayCommunityCards(_communityCards);

        var winners = HandEvaluator.DetermineWinners(_activePlayers, _communityCards);
        ConsoleUI.DisplayWinner(winners, _betting.Pot);
        _betting.AwardPot(winners);

        Console.WriteLine("\n--- Final Chip Counts ---");
        ConsoleUI.DisplayGameStatus(_players, _betting.Pot);
    }
}