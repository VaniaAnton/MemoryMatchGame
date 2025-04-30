using System;
using System.Linq;
using MemoryMatch.Core.Models;

namespace MemoryMatch.Core.Services
{
    public class ScoreCalculator
    {
        // Delegate examples
        public delegate int ScoreCalculation(Card card, int attempts);
        public delegate int BonusCalculation(params int[] values);
        
        // Default calculation
        public static readonly ScoreCalculation DefaultCalculation = (card, attempts) => 
            10 * (1 + (card.IsSpecialCard() ? 1 : 0)) / Math.Max(1, attempts / 2);
        
        // Bonus calculation using params
        public static int CalculateBonus(params int[] bonusFactors)
        {
            return bonusFactors.Sum();
        }
        
        private readonly ScoreCalculation _calculation;
        
        public ScoreCalculator(ScoreCalculation calculation = null)
        {
            _calculation = calculation ?? DefaultCalculation;
        }
        
        public int CalculateMatchScore(Card card1, Card card2, int attempts)
        {
            if (card1 == null) throw new ArgumentNullException(nameof(card1));
            if (card2 == null) throw new ArgumentNullException(nameof(card2));
            
            if (card1 != card2) return 0;
            
            // Pattern matching with switch
            int bonus = (card1.Type, card2.Type) switch
            {
                var (t1, t2) when t1 == CardType.Star && t2 == CardType.Star => 5,
                var (t1, t2) when t1 == CardType.Moon && t2 == CardType.Moon => 3,
                var (t1, _) when t1 == CardType.Sun => 2,
                _ => 0
            };
            
            return _calculation(card1, attempts) + bonus;
        }
    }
}