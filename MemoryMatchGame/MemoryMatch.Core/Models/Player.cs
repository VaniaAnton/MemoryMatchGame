using System;

namespace MemoryMatch.Core.Models
{
    public class Player : IFormattable
    {
        public string Name { get; }
        public int Score { get; private set; }
        public int Matches { get; private set; }
        public int Attempts { get; private set; }
        
        public Player(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        public void AddScore(int points)
        {
            Score += points;
        }
        
        public void AddMatch()
        {
            Matches++;
        }
        
        public void AddAttempt()
        {
            Attempts++;
        }
        
        public void Reset()
        {
            Score = 0;
            Matches = 0;
            Attempts = 0;
        }
        
        // IFormattable implementation
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return format?.ToLower() switch
            {
                "n" => Name,
                "s" => $"Score: {Score}",
                "f" => $"{Name} - Score: {Score}, Matches: {Matches}, Attempts: {Attempts}",
                "r" => Attempts > 0 ? $"Success Rate: {(double)Matches / Attempts:P0}" : "No attempts",
                _ => $"{Name}: {Score} pts"
            };
        }
        
        public override string ToString()
        {
            return ToString(null, null);
        }
    }
}