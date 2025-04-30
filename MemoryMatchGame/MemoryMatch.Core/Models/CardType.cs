namespace MemoryMatch.Core.Models
{
    // Sealed class example
    public class CardType
    {
        public static readonly CardType Heart = new CardType(1, "Heart", "♥️");
        public static readonly CardType Diamond = new CardType(2, "Diamond", "♦️");
        public static readonly CardType Club = new CardType(3, "Club", "♣️");
        public static readonly CardType Spade = new CardType(4, "Spade", "♠️");
        public static readonly CardType Star = new CardType(5, "Star", "⭐");
        public static readonly CardType Moon = new CardType(6, "Moon", "🌙");
        public static readonly CardType Sun = new CardType(7, "Sun", "☀️");
        public static readonly CardType Cloud = new CardType(8, "Cloud", "☁️");
        
        public int Id { get; }
        public string Name { get; }
        public string Symbol { get; }
        
        private CardType(int id, string name, string symbol)
        {
            Id = id;
            Name = name;
            Symbol = symbol;
        }
        
        // Override Equals to compare by Id
        public override bool Equals(object obj)
        {
            return obj is CardType type && Id == type.Id;
        }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
        // Implement == and != operators
        public static bool operator ==(CardType left, CardType right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);
            return left.Equals(right);
        }
        
        public static bool operator !=(CardType left, CardType right)
        {
            return !(left == right);
        }
    }
}