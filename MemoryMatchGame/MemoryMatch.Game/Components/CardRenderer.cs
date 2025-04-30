using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MemoryMatch.Core.Models;
using System.Collections.Generic;

namespace MemoryMatch.Game.Components
{
    // Partial class example
    public partial class CardRenderer
    {
        private readonly Dictionary<string, Rectangle> _cardPositions = new Dictionary<string, Rectangle>();
        private readonly Color[] _cardTypeColors = new Color[9]; // Colors for different card types
        private Texture2D _cardTexture;
        private SpriteBatch _spriteBatch;
        
        // Constructor with default and named parameters
        public CardRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int cardWidth = 80, int cardHeight = 100)
        {
            CardWidth = cardWidth;
            CardHeight = cardHeight;
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            
            // Create a pixel texture for drawing cards
            _cardTexture = new Texture2D(graphicsDevice, 1, 1);
            _cardTexture.SetData(new[] { Color.White });
            
            // Initialize card type colors
            InitializeCardColors();
        }
        
        // Properties
        public int CardWidth { get; }
        public int CardHeight { get; }
        public int Padding { get; set; } = 10;
        
        // Method to calculate card positions
        public void CalculateCardPositions(GameBoard board, int screenWidth, int screenHeight)
        {
            _cardPositions.Clear();
            
            // Calculate layout
            int totalWidth = (board.Columns * CardWidth) + ((board.Columns - 1) * Padding);
            int totalHeight = (board.Rows * CardHeight) + ((board.Rows - 1) * Padding);
            
            int startX = (screenWidth - totalWidth) / 2;
            int startY = 100; // Top margin
            
            // Calculate position for each card
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    string id = $"{row}_{col}";
                    int x = startX + (col * (CardWidth + Padding));
                    int y = startY + (row * (CardHeight + Padding));
                    
                    _cardPositions[id] = new Rectangle(x, y, CardWidth, CardHeight);
                }
            }
        }
        
        // Method to draw a card
        public void DrawCard(Card card)
        {
            if (!_cardPositions.TryGetValue(card.Id, out var rectangle))
            {
                return; // Card position not found
            }
            
            // Using null conditional operator
            bool isRevealed = (card?.State & CardState.Revealed) != 0;
            bool isMatched = (card?.State & CardState.Matched) != 0;
            
            if (isRevealed || isMatched)
            {
                // Draw card front
                _spriteBatch.Draw(_cardTexture, rectangle, Color.White);
                
                // Get color for card type using null coalescing operator
                var cardTypeId = card?.Type?.Id ?? 0;
                var cardColor = cardTypeId < _cardTypeColors.Length ? _cardTypeColors[cardTypeId] : Color.Gray;
                
                // Draw inner rectangle with card color
                var innerRect = new Rectangle(
                    rectangle.X + 5, 
                    rectangle.Y + 5, 
                    rectangle.Width - 10, 
                    rectangle.Height - 10);
                
                _spriteBatch.Draw(_cardTexture, innerRect, cardColor);
                
                // Draw card value indicator
                var valueRect = new Rectangle(
                    rectangle.X + rectangle.Width / 2 - 10,
                    rectangle.Y + rectangle.Height / 2 - 10,
                    20,
                    20
                );
                
                _spriteBatch.Draw(_cardTexture, valueRect, Color.White);
            }
            else
            {
                // Draw card back
                _spriteBatch.Draw(_cardTexture, rectangle, Color.DarkBlue);
            }
            
            // If card is matched, draw a border
            if (isMatched)
            {
                DrawBorder(rectangle, 2, Color.Green);
            }
        }
        
        // Helper method to draw a border
        private void DrawBorder(Rectangle rectangle, int thickness, Color color)
        {
            // Top
            _spriteBatch.Draw(_cardTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            // Bottom
            _spriteBatch.Draw(_cardTexture, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
            // Left
            _spriteBatch.Draw(_cardTexture, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            // Right
            _spriteBatch.Draw(_cardTexture, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
        }
        
        // Check if a position is over a card - returns the card ID or null
        public bool TryGetCardAtPosition(Point position, out string cardId)
        {
            foreach (var pair in _cardPositions)
            {
                if (pair.Value.Contains(position))
                {
                    cardId = pair.Key;
                    return true;
                }
            }
            
            cardId = null;
            return false;
        }
    }
    
    // Partial class implementation - showing partial class feature
    public partial class CardRenderer
    {
        // Initialize colors for different card types
        private void InitializeCardColors()
        {
            _cardTypeColors[0] = Color.Gray; // Default
            _cardTypeColors[1] = Color.Red; // Heart
            _cardTypeColors[2] = Color.Pink; // Diamond
            _cardTypeColors[3] = Color.Black; // Club
            _cardTypeColors[4] = Color.DarkGray; // Spade
            _cardTypeColors[5] = Color.Yellow; // Star
            _cardTypeColors[6] = Color.DarkBlue; // Moon
            _cardTypeColors[7] = Color.Orange; // Sun
            _cardTypeColors[8] = Color.Purple; // Cloud
        }
        
        // Operator overloading example
        public static CardRenderer operator +(CardRenderer renderer, int paddingIncrease)
        {
            renderer.Padding += paddingIncrease;
            return renderer;
        }
    }
}