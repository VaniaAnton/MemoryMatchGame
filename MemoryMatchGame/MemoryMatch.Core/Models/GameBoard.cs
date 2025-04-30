using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryMatch.Core.Models
{
    public class GameBoard : GameEntity
    {
        private readonly Dictionary<string, Card> _cards = new Dictionary<string, Card>();
        public IReadOnlyDictionary<string, Card> Cards => _cards;
        
        public int Rows { get; }
        public int Columns { get; }
        
        public GameBoard(string id, int rows, int columns) : base(id)
        {
            Rows = rows;
            Columns = columns;
        }
        
        public bool TryGetCard(string id, out Card card)
        {
            return _cards.TryGetValue(id, out card);
        }
        
        public bool TryGetCard(int row, int column, out Card card)
        {
            string id = $"{row}_{column}";
            return TryGetCard(id, out card);
        }
        
        public void AddCard(Card card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            _cards[card.Id] = card;
        }
        
        // Range type usage
        public IEnumerable<Card> GetCardsInRange(Range rowRange, Range columnRange)
        {
            var rowIndices = Enumerable.Range(rowRange.Start.Value, rowRange.End.Value - rowRange.Start.Value);
            var columnIndices = Enumerable.Range(columnRange.Start.Value, columnRange.End.Value - columnRange.Start.Value);
            
            foreach (var row in rowIndices)
            {
                foreach (var column in columnIndices)
                {
                    if (TryGetCard(row, column, out var card))
                    {
                        yield return card;
                    }
                }
            }
        }
        
        public override void Update(float deltaTime)
        {
            foreach (var card in _cards.Values)
            {
                card.Update(deltaTime);
            }
        }
        
        public override void Reset()
        {
            base.Reset();
            foreach (var card in _cards.Values)
            {
                card.Reset();
            }
        }
    }
}