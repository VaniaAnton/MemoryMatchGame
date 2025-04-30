using System;

namespace MemoryMatch.Core.Exceptions
{
    // Base exception class for all Memory Match exceptions
    public class MemoryMatchException : Exception
    {
        public MemoryMatchException() : base() { }
        
        public MemoryMatchException(string message) : base(message) { }
        
        public MemoryMatchException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
    
    // Exception for card-related errors
    public class CardException : MemoryMatchException
    {
        public string CardId { get; }
        
        public CardException(string cardId) : base($"Error with card: {cardId}")
        {
            CardId = cardId;
        }
        
        public CardException(string cardId, string message) 
            : base($"Error with card {cardId}: {message}")
        {
            CardId = cardId;
        }
        
        public CardException(string cardId, string message, Exception innerException) 
            : base($"Error with card {cardId}: {message}", innerException)
        {
            CardId = cardId;
        }
    }
    
    // Exception for invalid game state
    public class GameStateException : MemoryMatchException
    {
        public GameState CurrentState { get; }
        public GameState ExpectedState { get; }
        
        public GameStateException(GameState currentState, GameState expectedState)
            : base($"Invalid game state. Current: {currentState}, Expected: {expectedState}")
        {
            CurrentState = currentState;
            ExpectedState = expectedState;
        }
        
        public GameStateException(GameState currentState, GameState expectedState, string message)
            : base($"Invalid game state: {message}. Current: {currentState}, Expected: {expectedState}")
        {
            CurrentState = currentState;
            ExpectedState = expectedState;
        }

        public GameStateException(Services.GameState state, Services.GameState playing, string v)
        {
        }
    }
    
    // Exception for invalid move attempts
    public class InvalidMoveException : MemoryMatchException
    {
        public string CardId { get; }
        
        public InvalidMoveException(string cardId)
            : base($"Invalid move with card: {cardId}")
        {
            CardId = cardId;
        }
        
        public InvalidMoveException(string cardId, string reason)
            : base($"Invalid move with card {cardId}: {reason}")
        {
            CardId = cardId;
        }
    }
    
    // Exception for when player is out of moves
    public class OutOfMovesException : MemoryMatchException
    {
        public int MaxMoves { get; }
        
        public OutOfMovesException(int maxMoves)
            : base($"Player has reached the maximum number of moves: {maxMoves}")
        {
            MaxMoves = maxMoves;
        }
    }
    
    // Exception for configuration errors
    public class ConfigurationException : MemoryMatchException
    {
        public string ConfigurationParameter { get; }
        
        public ConfigurationException(string parameter, string message)
            : base($"Configuration error for {parameter}: {message}")
        {
            ConfigurationParameter = parameter;
        }
    }
}

// Adding GameState enum for use with exceptions
namespace MemoryMatch.Core
{
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
}