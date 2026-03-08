namespace CaludeGame.Classes;

public class BettingManager
{
    public int Pot { get; private set; }
    public int CurrentBet { get; private set; }
    private Dictionary<Player, int> _playerBets = new();

    public void PostBlind(Player player, int amount)
    {
        player.Chips -= amount;
        Pot += amount;
        CurrentBet = amount;
        _playerBets[player] = amount;
        Console.WriteLine($"{player.Name} posts blind of {amount}. Pot: {Pot}");
    }

    public void Fold(Player player)
    {
        Console.WriteLine($"{player.Name} folds.");
    }

    public void Call(Player player)
    {
        _playerBets.TryGetValue(player, out int alreadyBet);
        int amountOwed = CurrentBet - alreadyBet;

        if (amountOwed <= 0)
        {
            Console.WriteLine($"{player.Name} checks.");
            return;
        }
        if (player.Chips < amountOwed)
        {
            Console.WriteLine($"{player.Name} goes all in with {player.Chips}!");
            Pot += player.Chips;
            _playerBets[player] = alreadyBet + player.Chips;
            player.Chips = 0;
            return;
        }

        player.Chips -= amountOwed;
        Pot += amountOwed;
        _playerBets[player] = CurrentBet;
        Console.WriteLine($"{player.Name} calls {amountOwed}. Pot: {Pot}");
    }

    public bool Raise(Player player, int amount)
    {
        if (amount <= CurrentBet)
        {
            Console.WriteLine($"Raise must be higher than current bet of {CurrentBet}!");
            return false;
        }
        if (player.Chips < amount)
        {
            Console.WriteLine($"{player.Name} doesn't have enough chips!");
            return false;
        }
        player.Chips -= amount;
        Pot += amount;
        CurrentBet = amount;
        _playerBets[player] = amount;
        Console.WriteLine($"{player.Name} raises to {amount}. Pot: {Pot}");
        return true;
    }

    public void AwardPot(List<Player> winners)
    {
        if (Pot == 0)
        {
            Console.WriteLine("Pot is empty, nothing to award!");
            return;
        }
        int share = Pot / winners.Count;
        foreach (var winner in winners)
        {
            winner.Chips += share;
            Console.WriteLine($"{winner.Name} wins {share} chips! Total chips: {winner.Chips}");
        }
        Pot = 0;
        CurrentBet = 0;
        _playerBets.Clear();
    }
    public int GetPlayerBet(Player player)
    {
        _playerBets.TryGetValue(player, out int amount);
        return amount;
    }

    public void Reset()
    {
        Pot = 0;
        CurrentBet = 0;
        _playerBets.Clear();
    }
}