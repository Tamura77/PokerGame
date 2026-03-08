namespace CaludeGame.Classes;

public class BlindStructure
{
    private readonly List<(int SmallBlind, int BigBlind, int HandsAtLevel)> _levels = new()
    {
        (10,  20,  5),
        (15,  30,  5),
        (25,  50,  5),
        (50,  100, 5),
        (75,  150, 5),
        (100, 200, 5),
        (150, 300, 5),
        (200, 400, 5),
    };

    private int _currentLevel = 0;
    private int _handsPlayedAtLevel = 0;
    public int HandsPlayed { get; private set; } = 0;

    public int SmallBlind => _levels[_currentLevel].SmallBlind;
    public int BigBlind => _levels[_currentLevel].BigBlind;
    public int MinRaise => BigBlind * 2;
    public int Level => _currentLevel + 1;

    public void RecordHand()
    {
        HandsPlayed++;
        _handsPlayedAtLevel++;

        int handsAtThisLevel = _levels[_currentLevel].HandsAtLevel;
        if (_handsPlayedAtLevel >= handsAtThisLevel && _currentLevel < _levels.Count - 1)
        {
            _currentLevel++;
            _handsPlayedAtLevel = 0;
            Console.WriteLine("\n╔══════════════════════════════╗");
            Console.WriteLine($"║  {"⬆ BLINDS INCREASING!",-28}║");
            Console.WriteLine($"║  {"Level " + Level + $": {SmallBlind}/{BigBlind}",-28}║");
            Console.WriteLine($"║  {"Min Raise: " + MinRaise,-28}║");
            Console.WriteLine("╚══════════════════════════════╝\n");
        }
    }

    public void DisplayCurrentLevel()
    {
        Console.WriteLine($"  Level {Level} | Blinds: {SmallBlind}/{BigBlind} | Min Raise: {MinRaise} | Hand: {HandsPlayed + 1}");
    }
}