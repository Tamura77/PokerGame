
namespace CaludeGame.Classes;
public class Deck
{
    public Deck(bool isShuffled)
    {
        Cards = [];
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            for (int value = 1; value <= 13; value++)
            {
                Cards.Add(new Card(suit, value));
            }
        }
        if (isShuffled) Shuffle();
    }
    public void Shuffle()
    {
        var rng = new Random();
        for (int i = Cards.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (Cards[i], Cards[j]) = (Cards[j], Cards[i]); 
        }
    }
    public Card Deal()
    {
        if (Cards.Count == 0)
            throw new InvalidOperationException("No cards left in deck!");

        var card = Cards[0];
        Cards.RemoveAt(0);
        return card;
    }
    public List<Card> Cards { get; set; }
}
