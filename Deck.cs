namespace CaludeGame.Classes

public class Deck
{
    public Deck()
    {
        Cards = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            for (int value = 1; value <= 13; value++)
            {
                Cards.Add(new Card(suit, value));
            }
        }
    }
    public List<Card> Cards { get; set; }
}
