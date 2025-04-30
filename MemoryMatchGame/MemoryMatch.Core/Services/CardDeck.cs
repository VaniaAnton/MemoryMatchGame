using System;
using System.Collections.Generic;
using System.Linq;
using MemoryMatch.Core.Models;

namespace MemoryMatch.Core.Services
{
    public class CardDeck
    {
        private readonly List<CardType> _availableTypes;
        private readonly Random _random = new Random();
        
        // Using a collection
        private readonly Dictionary<CardType, int> _typeCount = new Dictionary<CardType, int>();
        
        public CardDeck(params CardType[] availableTypes)
        {
            _availableTypes = availableTypes?.ToList() ?? new List<CardType>
            {
                CardType.Heart,
                CardType.Diamond,
                CardType.Club,
                CardType.Spade
            };
            
            // Print available card types for debugging
            Console.WriteLine($"CardDeck initialized with {_availableTypes.Count} types:");
            foreach (var type in _availableTypes)
            {
                Console.WriteLine($"  - {type.Name} (ID: {type.Id})");
            }
        }
        
        public List<Card> GenerateDeck(int pairs, bool includeSpecials = true)
        {
            if (pairs <= 0) throw new ArgumentException("Must have at least one pair", nameof(pairs));
            Console.WriteLine($"Generating deck with {pairs} pairs");
            
            _typeCount.Clear();
            var cards = new List<Card>();
            var usedCombinations = new HashSet<(int typeId, int value)>();
            
            // First, create a set of unique card definitions (type, value)
            var cardDefinitions = new List<(CardType type, int value)>();
            
            for (int i = 0; i < pairs; i++)
            {
                CardType cardType;
                int typeIndex = i % _availableTypes.Count; // Ensure even distribution
                cardType = _availableTypes[typeIndex];
                
                // Use pair index as value to ensure unique pairs
                int value = i + 1;
                
                // Add this definition
                cardDefinitions.Add((cardType, value));
            }
            
            // Now create two cards for each definition (the pair)
            foreach (var def in cardDefinitions)
            {
                var (cardType, value) = def;
                
                // Create unique IDs for each card
                string idA = $"card_{cardType.Id}_{value}_a";
                string idB = $"card_{cardType.Id}_{value}_b";
                
                cards.Add(new Card(idA, cardType, value));
                cards.Add(new Card(idB, cardType, value));
                
                Console.WriteLine($"Created pair: {cardType.Name} with value {value}");
            }
            
            // Shuffle the cards
            cards = cards.OrderBy(c => _random.Next()).ToList();
            Console.WriteLine($"Deck generated with {cards.Count} cards");
            
            return cards;
        }
    }
}