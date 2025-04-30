using System;
using System.Collections;
using System.Collections.Generic;
using MemoryMatch.Core.Models;

namespace MemoryMatch.Core.Collections
{
    // Generic collection class that implements IEnumerable<T>
    public class CardCollection<T> : IEnumerable<T> where T : Card
    {
        private List<T> _cards = new List<T>();
        
        // Add a card to the collection
        public void Add(T card)
        {
            if (card == null)
                throw new CardCollectionException("Cannot add null card to collection");
                
            _cards.Add(card);
        }
        
        // Remove a card from the collection
        public bool Remove(T card)
        {
            return _cards.Remove(card);
        }
        
        // Get a card by index
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _cards.Count)
                    throw new CardCollectionException($"Index {index} is out of range");
                return _cards[index];
            }
        }
        
        // Count property
        public int Count => _cards.Count;
        
        // Clear the collection
        public void Clear()
        {
            _cards.Clear();
        }
        
        // IEnumerable<T> implementation - returns custom enumerator
        public IEnumerator<T> GetEnumerator()
        {
            return new CardEnumerator<T>(_cards);
        }
        
        // IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        // Create an iterator for cards of a specific type
        public IEnumerable<T> GetCardsByType(CardType type)
        {
            // Iterator implementation using yield
            foreach (var card in _cards)
            {
                if (card.Type.Equals(type))
                {
                    yield return card;
                }
            }
        }
        
        // Another iterator example - get cards with specific state
        public IEnumerable<T> GetCardsByState(CardState state)
        {
            foreach (var card in _cards)
            {
                if ((card.State & state) != 0)
                {
                    yield return card;
                }
            }
        }
        
        // Clone the collection
        public CardCollection<T> Clone()
        {
            var cloned = new CardCollection<T>();
            foreach (var card in _cards)
            {
                // Assumes T implements ICloneable
                if (card is ICloneable cloneable)
                {
                    cloned.Add((T)cloneable.Clone());
                }
                else
                {
                    cloned.Add(card); // Just add reference if not cloneable
                }
            }
            return cloned;
        }
    }
    
    // Custom generic enumerator that implements IEnumerator<T>
    public class CardEnumerator<T> : IEnumerator<T> where T : Card
    {
        private readonly List<T> _cards;
        private int _position = -1;
        
        public CardEnumerator(List<T> cards)
        {
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
        }
        
        // Current property - returns the current card
        public T Current
        {
            get
            {
                try
                {
                    return _cards[_position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException("Enumerator is not positioned on a valid element");
                }
            }
        }
        
        // Non-generic version of Current
        object IEnumerator.Current => Current;
        
        // Move to next card
        public bool MoveNext()
        {
            _position++;
            return _position < _cards.Count;
        }
        
        // Reset to beginning
        public void Reset()
        {
            _position = -1;
        }
        
        // Dispose method
        public void Dispose()
        {
            // No unmanaged resources to dispose
        }
    }
    
    // Custom exception type for card collection
    public class CardCollectionException : Exception
    {
        public CardCollectionException() : base() { }
        public CardCollectionException(string message) : base(message) { }
        public CardCollectionException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}