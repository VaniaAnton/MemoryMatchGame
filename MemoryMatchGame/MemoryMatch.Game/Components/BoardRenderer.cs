using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MemoryMatch.Core.Models;
using System.Collections.Generic;

namespace MemoryMatch.Game.Components
{
    // Abstract class example
    public abstract class Renderer
    {
        protected SpriteBatch SpriteBatch { get; }
        public Texture2D Pixel { get; }
        
        protected Renderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            SpriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            
            // Create a pixel texture for drawing
            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }
        
        public abstract void Draw(GameTime gameTime);
        
        // Method with params keyword
        protected void DrawRectangles(Color color, params Rectangle[] rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                SpriteBatch.Draw(Pixel, rectangle, color);
            }
        }
    }
    
    public class BoardRenderer : Renderer
    {
        private readonly CardRenderer _cardRenderer;
        private GameBoard _board;
        private Rectangle _boardArea;
        private Rectangle _scoreArea;
        
        // Stats display
        private int _score;
        private int _attempts;
        private int _matches;
        
        public BoardRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, CardRenderer cardRenderer) 
            : base(graphicsDevice, spriteBatch)
        {
            _cardRenderer = cardRenderer ?? throw new ArgumentNullException(nameof(cardRenderer));
        }
        
        public void SetBoard(GameBoard board, Rectangle boardArea, Rectangle scoreArea)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _boardArea = boardArea;
            _scoreArea = scoreArea;
            
            // Calculate card positions
            _cardRenderer.CalculateCardPositions(board, boardArea.Width, boardArea.Height);
        }
        
        public void UpdateStats(int score, int attempts, int matches)
        {
            _score = score;
            _attempts = attempts;
            _matches = matches;
        }
        
        // Override from abstract class
        public override void Draw(GameTime gameTime)
        {
            if (_board == null) return;
            
            // Start SpriteBatch before drawing
            SpriteBatch.Begin();
            
            // Draw board background
            SpriteBatch.Draw(Pixel, _boardArea, new Color(100, 149, 237)); // Cornflower blue
            
            // Draw score area
            SpriteBatch.Draw(Pixel, _scoreArea, new Color(0, 0, 0, 128));
            
            // Draw score bars
            int scoreBarWidth = Math.Min(_score * 5, _scoreArea.Width - 20);
            var scoreBarRect = new Rectangle(_scoreArea.X + 10, _scoreArea.Y + 10, scoreBarWidth, 15);
            
            int attemptsBarWidth = Math.Min(_attempts * 10, _scoreArea.Width - 20);
            var attemptsBarRect = new Rectangle(_scoreArea.X + 10, _scoreArea.Y + 35, attemptsBarWidth, 15);
            
            int matchesBarWidth = Math.Min(_matches * 20, _scoreArea.Width - 20);
            var matchesBarRect = new Rectangle(_scoreArea.X + 10, _scoreArea.Y + 60, matchesBarWidth, 15);
            
            // Using params to draw multiple rectangles in one call
            DrawRectangles(Color.Green, scoreBarRect);
            DrawRectangles(Color.Yellow, attemptsBarRect);
            DrawRectangles(Color.Red, matchesBarRect);
            
            // Draw all cards
            if (_board is GameBoard board) // Using 'is' operator
            {
                foreach (var pair in board.Cards)
                {
                    _cardRenderer.DrawCard(pair.Value);
                }
            }
            
            // End SpriteBatch when done drawing
            SpriteBatch.End();
        }
        
        // Using Range type
        public void DrawCardsInRange(Range rowRange, Range columnRange)
        {
            if (_board == null) return;
            
            // Get cards in specified ranges
            var cards = _board.GetCardsInRange(rowRange, columnRange);
            
            // Start SpriteBatch
            SpriteBatch.Begin();
            
            foreach (var card in cards)
            {
                _cardRenderer.DrawCard(card);
            }
            
            // End SpriteBatch
            SpriteBatch.End();
        }
    }
}