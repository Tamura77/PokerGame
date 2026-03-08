namespace CaludeGame.Classes;

public class Player
{
    public Player(string name, int chips, bool isHuman = true)
    {
        Name = name;
        Chips = chips;
        IsHuman = isHuman;
        Hand = new List<Card>();
    }

    public string Name { get; }
    public int Chips { get; set; }
    public bool IsHuman { get; }
    public List<Card> Hand { get; set; }

    public void AddCard(Card card)
    {
        Hand.Add(card);
    }

    public void ClearHand()
    {
        Hand.Clear();
    }

    public void DisplayHand()
    {
        Console.WriteLine($"\n{Name}'s hand:");
        foreach (var card in Hand)
            Console.WriteLine($"  {card}");
    }
}