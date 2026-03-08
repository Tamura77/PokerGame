namespace CaludeGame.Classes;
public enum Suit { Spade, Club, Diamond, Heart }

public class Card
{
    public Card(Suit suit, int value)
    {
        Value = value;
        Suit = suit;
    }

    public int Value { get; }
    public Suit Suit { get; }

    public override string ToString()
    {
        string rank = Value switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => Value.ToString()
        };
        return $"{rank} of {Suit}s";
    }
}
