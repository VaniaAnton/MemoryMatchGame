using System;
using MemoryMatch.Core.Interfaces;

namespace MemoryMatch.Core.Models
{
    // Bitwise flags for card state
    [Flags]
    public enum CardState
    {
        None = 0,
        Revealed = 1 << 0,
        Matched = 1 << 1,
        Special = 1 << 2,
        Locked = 1 << 3
    }

    // Card class implementing multiple interfaces including ICloneable
    public class Card : GameEntity, IComparable<Card>, IEquatable<Card>, IFormattable, IInteractable, ICloneable
    {
        // Static constructor example
        static Card()
        {
            Console.WriteLine("Card class initialized");
        }
        
        public CardType Type { get; }
        public int Value { get; }
        public CardState State { get; set; }
        public bool CanInteract => (State & (CardState.Matched | CardState.Locked)) == 0;
        
        public event Action<string> OnInteraction;
        
        public Card(string id, CardType type, int value) : base(id)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value;
            State = CardState.None;
        }
        
        // Deconstructor implementation
        public void Deconstruct(out CardType type, out int value)
        {
            type = Type;
            value = Value;
        }
        
        // IComparable implementation
        public int CompareTo(Card other)
        {
            if (other is null) return 1;
            
            var valueComparison = Value.CompareTo(other.Value);
            return valueComparison != 0 ? valueComparison : Type.Id.CompareTo(other.Type.Id);
        }
        
        // IEquatable implementation
        public bool Equals(Card other)
        {
            if (other is null) return false;
            return Type.Id == other.Type.Id && Value == other.Value;
        }
        
        public override bool Equals(object obj)
        {
            return obj is Card card && Equals(card);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Type.Id, Value);
        }
        
        // IFormattable implementation
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return format?.ToLower() switch
            {
                "s" => Type.Symbol,
                "f" => $"{Type.Name} {Value}",
                "v" => Value.ToString(),
                _ => $"{Type.Symbol} {Value}"
            };
        }
        
        public override string ToString()
        {
            return ToString(null, null);
        }
        
        // Operator overloading
        public static bool operator ==(Card left, Card right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        
        public static bool operator !=(Card left, Card right)
        {
            return !(left == right);
        }
        
        // Methods with out parameters
        public bool TryReveal(out CardState previousState)
        {
            previousState = State;
            
            if ((State & CardState.Revealed) != 0 || (State & CardState.Matched) != 0)
                return false;
                
            State |= CardState.Revealed;
            return true;
        }
        
        public void Match()
        {
            State |= CardState.Matched;
        }
        
        public void Hide()
        {
            State &= ~CardState.Revealed;
        }
        
        // Pattern matching example
        public bool IsSpecialCard() => Type switch
        {
            var t when t.Id == 5 || t.Id == 6 => true, // Star or Moon
            _ => false
        };
        
        public override void Update(float deltaTime)
        {
            // Update animation or state
        }
        
        // IInteractable implementation
        public void Interact()
        {
            Console.WriteLine($"Card {Id} interacted with");
            
            if (!CanInteract) 
            {
                Console.WriteLine($"Card {Id} cannot be interacted with");
                return;
            }
            
            // No need to call TryReveal - GameLogic will handle the state change
            OnInteraction?.Invoke(Id);
            Console.WriteLine($"Card {Id} interaction event fired");
        }
        
        public override void Reset()
        {
            base.Reset();
            State = CardState.None;
        }
        
        // ICloneable implementation
        public object Clone()
        {
            // Create a new card with the same properties
            var clone = new Card(Id, Type, Value);
            clone.State = State;
            return clone;
        }
        
        // Try-catch example method
        public static Card TryCreateCard(string id, CardType type, int value)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException(nameof(id), "Card ID cannot be null or empty");
                
                if (type == null)
                    throw new ArgumentNullException(nameof(type), "Card type cannot be null");
                
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Card value must be non-negative");
                
                return new Card(id, type, value);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"Failed to create card: {ex.Message}");
                return null;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"Failed to create card: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error creating card: {ex.Message}");
                return null;
            }
        }
    }
}