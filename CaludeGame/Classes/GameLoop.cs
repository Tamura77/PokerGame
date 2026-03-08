namespace CaludeGame.Classes;

public class GameLoop
{
    private Deck _deck;
    private List<Player> _players;
    private BettingManager _betting;
    private List<Card> _communityCards;
    private List<Player> _activePlayers;
    private int _dealerIndex;
    private BlindStructure _blinds;

    public GameLoop(List<Player> players)
    {
        _players = players;
        _betting = new BettingManager();
        _communityCards = new List<Card>();
        _activePlayers = new List<Player>();
        _deck = new Deck(true);
        _dealerIndex = 0;
        _blinds = new BlindStructure();
    }

    public void Run()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║  ♠ WELCOME TO CLAUDEGAME ♠   ║");
        Console.WriteLine("╚══════════════════════════════╝\n");

        while (_players.Count > 1)
        {
            StartRound();
            _blinds.RecordHand();

            var eliminated = _players.Where(p => p.Chips <= 0).ToList();
            foreach (var player in eliminated)
            {
                Console.WriteLine($"\n{player.Name} has been eliminated!");
                _players.Remove(player);
            }

            if (_players.Count == 1)
                break;

            Console.WriteLine("\nPress any key to start the next round...");
            Console.ReadKey();

            _dealerIndex = (_dealerIndex + 1) % _players.Count;
        }

        Console.WriteLine("\n╔══════════════════════════════╗");
        Console.WriteLine($"║  {"🏆 GAME OVER!",-28}║");
        Console.WriteLine($"║  {_players[0].Name + " wins the game!",-28}║");
        Console.WriteLine("╚══════════════════════════════╝");
    }

    private void StartRound()
    {
        Console.Clear();
        _deck = new Deck(true);
        _communityCards.Clear();
        _betting.Reset();
        _activePlayers = new List<Player>(_players);

        foreach (var player in _players)
            player.ClearHand();

        Console.WriteLine("=== NEW ROUND ===\n");
        _blinds.DisplayCurrentLevel();

        Console.WriteLine($"\nDealer: {_players[_dealerIndex].Name}");

        int smallBlindIndex = (_dealerIndex + 1) % _players.Count;
        int bigBlindIndex = (_dealerIndex + 2) % _players.Count;

        ConsoleUI.DisplayRoundTitle("Posting Blinds");
        _betting.PostBlind(_players[smallBlindIndex], _blinds.SmallBlind);
        _betting.PostBlind(_players[bigBlindIndex], _blinds.BigBlind);

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

        var roundBets = _activePlayers.ToDictionary(p => p, p => _betting.GetPlayerBet(p));
        var playersToAct = new Queue<Player>(_activePlayers);

        while (playersToAct.Count > 0)
        {
            var player = playersToAct.Dequeue();
            if (!_activePlayers.Contains(player)) continue;

            int amountOwed = _betting.CurrentBet - roundBets[player];
            string input;

            if (!player.IsHuman)
            {
                var decision = AIDecision.MakeDecision(
                    player,
                    _communityCards,
                    amountOwed,
                    _betting.Pot,
                    _betting.GetPlayerBet(player),
                    _blinds.MinRaise
                );
                if (decision.StartsWith("raise:"))
                    input = $"2:{decision.Split(':')[1]}";
                else if (decision == "call" || decision == "check")
                    input = "1";
                else
                    input = "3";
            }
            else
            {
                Console.WriteLine($"\n{player.Name}'s turn (Chips: {player.Chips}, Pot: {_betting.Pot})");
                if (amountOwed == 0)
                    Console.WriteLine($"1) Check  2) Raise (min {_blinds.MinRaise})  3) Fold");
                else
                    Console.WriteLine($"1) Call {amountOwed}  2) Raise (min {_blinds.MinRaise})  3) Fold");
                Console.Write("Choose: ");
                input = Console.ReadLine() ?? "1";
            }

            int raiseAmount = 0;
            if (input.StartsWith("2:") && int.TryParse(input.Split(':')[1], out int parsedRaise))
            {
                raiseAmount = parsedRaise;
                input = "2";
            }

            switch (input)
            {
                case "1":
                    if (amountOwed == 0)
                        Console.WriteLine($"{player.Name} checks.");
                    else
                    {
                        _betting.Call(player);
                        roundBets[player] = _betting.CurrentBet;
                    }
                    break;

                case "2":
                    if (raiseAmount == 0)
                    {
                        Console.Write($"Raise by (min {_blinds.MinRaise}): ");
                        int.TryParse(Console.ReadLine(), out raiseAmount);
                        raiseAmount = _betting.CurrentBet + raiseAmount; // ← convert to total
                    }

                    if (raiseAmount < _betting.CurrentBet + _blinds.MinRaise)
                    {
                        Console.WriteLine($"Minimum raise is {_blinds.MinRaise}. Setting to minimum.");
                        raiseAmount = _betting.CurrentBet + _blinds.MinRaise;
                    }

                    if (_betting.Raise(player, raiseAmount))
                    {
                        roundBets[player] = raiseAmount;
                        foreach (var other in _activePlayers)
                        {
                            if (other != player && !playersToAct.Contains(other))
                                playersToAct.Enqueue(other);
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
                    if (amountOwed == 0)
                        Console.WriteLine($"{player.Name} checks.");
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
        if (_activePlayers.Count == 1) return;

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