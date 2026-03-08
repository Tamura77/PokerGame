using CaludeGame.Classes;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var players = new List<Player>
{
    new Player("Alice", 1000),
    new Player("Bob", 1000),
    new Player("Charlie", 1000)
};

var game = new GameLoop(players);
game.StartRound();
