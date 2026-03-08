
namespace CaludeGame.Classes;

public class Player
{
    public Player(string name, int chips)
    {
        Name = name;
        Chips = chips;
        Hand = [];
    }

    public string Name { get; }
    public int Chips { get; set; }
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
        {
            Console.WriteLine($"  {card}");
        }
    }
}
