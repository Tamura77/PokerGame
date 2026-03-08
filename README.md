# ♠️ ClaudeGame - Console Poker

A simple Texas Hold'em poker game built as a C# .NET 8 console application.

This project was built entirely through prompt engineering with [Claude AI](https://claude.ai) as a personal exercise in learning how to effectively communicate with and direct AI to write code.

---

## 🎯 Purpose

This project is not production software — it's a **prompt engineering sandbox**. The goal is to:

- Practice writing clear, detailed prompts to guide AI code generation
- Learn how iterative back-and-forth with AI can build a real working project from scratch
- Explore how well Claude can handle architecture decisions, bug fixing, and feature additions

---

## 🃏 Features

- Full 52-card deck with shuffle and deal
- Texas Hold'em game loop (Pre-Flop, Flop, Turn, River)
- Support for 2+ players
- Betting system with check, call, raise and fold
- Small and big blinds
- Hand evaluator supporting all standard hand ranks:
  - High Card, One Pair, Two Pair, Three of a Kind
  - Straight, Flush, Full House, Four of a Kind, Straight Flush
- Tiebreaker logic with kicker comparison
- Split pot on true ties
- Player elimination on fold
- Coloured ASCII card display in the console

---

## 🚀 Getting Started

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Run the game

```bash
git clone https://github.com/YourUsername/ClaudeGame.git
cd ClaudeGame
dotnet run
```

---

## 🗂️ Project Structure

```
ClaudeGame/
├── Classes/
│   ├── Card.cs            # Card model with suit and rank
│   ├── Deck.cs            # 52 card deck with shuffle and deal
│   ├── Player.cs          # Player model with chips and hand
│   ├── HandRank.cs        # Enum of all hand rankings
│   ├── HandEvaluator.cs   # Hand ranking and tiebreaker logic
│   ├── BettingManager.cs  # Pot, blinds, call, raise, fold
│   ├── GameLoop.cs        # Round management and game flow
│   └── ConsoleUI.cs       # ASCII card rendering and display
└── Program.cs             # Entry point
```

---

## 🤖 Built With Claude AI

Every file in this project was written through conversation with Claude. The development process covered:

- Designing the class architecture from scratch
- Implementing Texas Hold'em rules and betting logic
- Debugging issues like pot calculation and hand evaluation
- Iterating on the console UI for card alignment and colour

If you're interested in prompt engineering, feel free to fork this and continue building — suggested next features are listed below.

---

## 🔜 Planned Features

- [ ] Multiple rounds with rotating blinds
- [ ] Player elimination when chips run out
- [ ] AI opponents with basic decision making
- [ ] Input validation and error handling improvements
- [ ] Side pot support for all-in scenarios

---

## 📄 License

This project is for personal learning purposes. Feel free to use it however you like.
