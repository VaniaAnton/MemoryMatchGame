using System;
using System.Collections.Generic;
using System.Linq;
using MemoryMatch.Core.Models;

namespace MemoryMatch.Core.Extensions
{
    // Static class for card extensions
    public static class CardExtensions
    {
        // Generic extension method for filtering cards
        public static IEnumerable<T> WhereMatched<T>(this IEnumerable<T> cards) where T : Card
        {
            // Using LINQ within extension method
            return cards.Where(card => (card.State & CardState.Matched) != 0);
        }
        
        // Generic extension method with where constraint
        public static IEnumerable<TCard> OfCardType<TCard, TType>(
            this IEnumerable<TCard> cards, 
            TType cardType) 
            where TCard : Card 
            where TType : CardType
        {
            return cards.Where(card => card.Type.Equals(cardType));
        }
        
        // Extension method to get all revealed but not matched cards
        public static IEnumerable<Card> GetRevealedNotMatched(this IEnumerable<Card> cards)
        {
            return cards.Where(card => 
                (card.State & CardState.Revealed) != 0 && 
                (card.State & CardState.Matched) == 0);
        }
        
        // Extension method to calculate score for a collection of cards
        public static int CalculateTotalValue(this IEnumerable<Card> cards)
        {
            // Using LINQ Sum
            return cards.Sum(card => card.Value);
        }
        
        // Extension method to check if all cards are matched
        public static bool AllMatched(this IEnumerable<Card> cards)
        {
            return cards.All(card => (card.State & CardState.Matched) != 0);
        }
        
        // Extension method to count cards by type
        public static Dictionary<CardType, int> CountByType(this IEnumerable<Card> cards)
        {
            // Using LINQ GroupBy
            return cards.GroupBy(card => card.Type)
                       .ToDictionary(group => group.Key, group => group.Count());
        }
        
        // Extension deconstructor
        public static void Deconstruct(this Card card, out CardType type, out int value, out CardState state)
        {
            type = card.Type;
            value = card.Value;
            state = card.State;
        }
        
        // Extension method with try-catch
        public static bool TryGetCardOfType(this IEnumerable<Card> cards, CardType type, out Card result)
        {
            result = null;
            
            try
            {
                result = cards.FirstOrDefault(card => card.Type.Equals(type));
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding card of type {type.Name}: {ex.Message}");
                return false;
            }
        }
    }
}