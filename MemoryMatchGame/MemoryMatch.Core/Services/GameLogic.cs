
using System;
using System.Collections.Generic;
using System.Linq;
using MemoryMatch.Core.Models;
using MemoryMatch.Core.Exceptions;
using MemoryMatch.Core.Extensions;
using MemoryMatch.Core.Collections;

namespace MemoryMatch.Core.Services
{
    // Flags for game state
    [Flags]
    public enum GameState
    {
        None = 0,
        Ready = 1 << 0,
        Playing = 1 << 1,
        Paused = 1 << 2,
        GameOver = 1 << 3,
        Victory = 1 << 4
    }
    public class GameLogic
    {
        public GameState State { get; private set; }
        public Player CurrentPlayer { get; }
        public GameBoard Board { get; }
        
        private readonly ScoreCalculator _scoreCalculator;
        private readonly List<Card> _revealedCards = new List<Card>();
        private readonly CardCollection<Card> _matchedCards = new CardCollection<Card>();
        private int _attempts;
        
        // Events with delegates
        public event Action<Card, Card, bool> OnMatchAttempt;
        public event Action<int> OnScoreChanged;
        public event Action<GameState> OnGameStateChanged;
        public event Action<Exception> OnError;
        
        public GameLogic(Player player, GameBoard board, ScoreCalculator scoreCalculator = null)
        {
            try
            {
                CurrentPlayer = player ?? throw new ArgumentNullException(nameof(player));
                Board = board ?? throw new ArgumentNullException(nameof(board));
                _scoreCalculator = scoreCalculator ?? new ScoreCalculator();
                State = GameState.Ready;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                throw new ConfigurationException("GameLogic", "Failed to initialize game logic: " + ex.Message);
            }
        }
        
        public void StartGame()
        {
            try
            {
                State = GameState.Playing;
                _revealedCards.Clear();
                _matchedCards.Clear();
                _attempts = 0;
                OnGameStateChanged?.Invoke(State);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                throw new GameStateException(State, GameState.Playing, "Failed to start game: " + ex.Message);
            }
        }
        
        // Check if we can reveal more cards - used by GameScreen
        public bool CanRevealMoreCards()
        {
            return _revealedCards.Count < 2;
        }
        
        // Clear revealed cards - called by GameScreen after showing non-matches
        public void ClearRevealedCards()
        {
            _revealedCards.Clear();
        }
        
        public void RevealCard(string cardId)
        {
            Console.WriteLine($"RevealCard called for {cardId}");
            
            try
            {
                // Using bitwise operations to check game state
                if ((State & GameState.Playing) == 0)
                {
                    Console.WriteLine("Game not in playing state");
                    throw new GameStateException(State, GameState.Playing, "Game is not in playing state");
                }
                
                if (!Board.TryGetCard(cardId, out var card))
                {
                    Console.WriteLine($"Card {cardId} not found");
                    throw new CardException(cardId, "Card not found on board");
                }
                
                // Don't allow revealing if we already have 2 cards revealed
                if (_revealedCards.Count >= 2)
                {
                    Console.WriteLine("Already have 2 cards revealed");
                    throw new InvalidMoveException(cardId, "Already have two cards revealed");
                }
                
                // Check if card is already revealed or matched
                if ((card.State & CardState.Revealed) != 0 || (card.State & CardState.Matched) != 0)
                {
                    Console.WriteLine("Card already revealed or matched");
                    throw new InvalidMoveException(cardId, "Card is already revealed or matched");
                }
                
                Console.WriteLine($"Revealing card {cardId}");
                
                // Force reveal the card
                card.State |= CardState.Revealed;
                _revealedCards.Add(card);
                
                Console.WriteLine($"Revealed cards count: {_revealedCards.Count}");
                
                if (_revealedCards.Count == 2)
                {
                    ProcessMatch();
                }
            }
            catch (MemoryMatchException ex)
            {
                // These are expected exceptions - log but don't propagate
                Console.WriteLine($"Game logic error: {ex.Message}");
                OnError?.Invoke(ex);
            }
            catch (Exception ex)
            {
                // Unexpected exceptions
                Console.WriteLine($"Unexpected error: {ex.Message}");
                OnError?.Invoke(ex);
                throw;
            }
        }
        
        private void ProcessMatch()
        {
            try
            {
                _attempts++;
                CurrentPlayer.AddAttempt();
                
                var card1 = _revealedCards[0];
                var card2 = _revealedCards[1];
                
                Console.WriteLine($"ProcessMatch: Card1={card1.Id}, Card2={card2.Id}");
                Console.WriteLine($"Card1 Type={card1.Type.Id}, Value={card1.Value}, Card2 Type={card2.Type.Id}, Value={card2.Value}");
                
                bool isMatch = card1.Type.Id == card2.Type.Id && card1.Value == card2.Value;
                Console.WriteLine($"Is Match: {isMatch}");
                
                if (isMatch)
                {
                    // Mark cards as matched
                    card1.State |= CardState.Matched;
                    card2.State |= CardState.Matched;
                    
                    // Add to matched cards collection
                    _matchedCards.Add(card1);
                    _matchedCards.Add(card2);
                    
                    int score = _scoreCalculator.CalculateMatchScore(card1, card2, _attempts);
                    CurrentPlayer.AddScore(score);
                    CurrentPlayer.AddMatch();
                    
                    OnScoreChanged?.Invoke(score);
                    Console.WriteLine($"Match found! Score: {score}, Total: {CurrentPlayer.Score}");
                    
                    // Clear revealed cards since they're now matched
                    _revealedCards.Clear();
                }
                
                OnMatchAttempt?.Invoke(card1, card2, isMatch);
                
                if (!isMatch)
                {
                    // We don't clear revealed cards here - GameScreen will handle it after showing the cards
                    Console.WriteLine("Not a match - waiting for animation");
                }
                
                CheckGameOver();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing match: {ex.Message}");
                OnError?.Invoke(ex);
            }
        }
        
        private void CheckGameOver()
        {
            // Using LINQ with All extension method to check if all cards are matched
            bool allMatched = Board.Cards.Values.All(card => (card.State & CardState.Matched) != 0);
            
            if (allMatched)
            {
                State = GameState.GameOver | GameState.Victory;
                OnGameStateChanged?.Invoke(State);
                Console.WriteLine("Game Over - Victory!");
            }
        }
        
        // Using LINQ to get various card statistics
        public Dictionary<string, int> GetGameStats()
        {
            try
            {
                var stats = new Dictionary<string, int>
                {
                    ["TotalCards"] = Board.Cards.Count,
                    ["MatchedCards"] = Board.Cards.Values.Count(c => (c.State & CardState.Matched) != 0),
                    ["RevealedCards"] = Board.Cards.Values.Count(c => (c.State & CardState.Revealed) != 0),
                    ["RemainingPairs"] = (Board.Cards.Count - 
                                        Board.Cards.Values.Count(c => (c.State & CardState.Matched) != 0)) / 2,
                    ["Score"] = CurrentPlayer.Score,
                    ["Attempts"] = CurrentPlayer.Attempts,
                    ["Matches"] = CurrentPlayer.Matches
                };
                
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting game stats: {ex.Message}");
                OnError?.Invoke(ex);
                return new Dictionary<string, int>();
            }
        }
        
        // Return all matched cards, using our IEnumerable implementation
        public CardCollection<Card> GetMatchedCards()
        {
            return _matchedCards;
        }
        
        // Get most valuable matched card using LINQ
        public Card GetMostValuableMatchedCard()
        {
            try
            {
                return _matchedCards.OrderByDescending(c => c.Value).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting most valuable card: {ex.Message}");
                OnError?.Invoke(ex);
                return null;
            }
        }
    }
}