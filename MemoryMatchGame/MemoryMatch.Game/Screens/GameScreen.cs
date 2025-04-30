using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MemoryMatch.Core.Models;
using MemoryMatch.Core.Services;
using MemoryMatch.Game.Components;
using System.Collections.Generic;

namespace MemoryMatch.Game.Screens
{
    // Enum defining difficulty levels
    public enum DifficultyLevel
    {
        Easy = 0,   // 3x4 grid
        Medium = 1, // 4x4 grid
        Hard = 2,   // 4x6 grid
        Expert = 3  // 6x6 grid
    }
    
    // Interface example
    public interface IGameScreen
    {
        void Initialize();
        void LoadContent();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
        bool IsActive { get; }
    }
    
    public class GameScreen : IGameScreen, IFormattable
    {
        private readonly Microsoft.Xna.Framework.Game _game;
        private readonly GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        
        // Game components
        private InputManager _inputManager;
        private CardRenderer _cardRenderer;
        private BoardRenderer _boardRenderer;
        
        // Game objects
        private GameBoard _gameBoard;
        private Player _player;
        private GameLogic _gameLogic;
        private CardDeck _cardDeck;
        private ScoreCalculator _scoreCalculator;
        
        // Game state
        private List<Card> _flippedCards = new List<Card>();
        private float _flipBackTimer = 0f;
        private bool _waitingToFlipBack = false;
        private bool _gameOver = false;
        private bool _victory = false;
        private bool _showMenu = true;
        
        // Game limits and stats
        private int _maxAttempts;
        private float _elapsedTime = 0;
        
        // Screen properties
        public bool IsActive { get; private set; } = true;
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
        
        // UI Elements
        private Rectangle _scoreboardRect;
        private Rectangle[] _difficultyButtons = new Rectangle[4];
        private Rectangle _startButton;
        private bool _startHover = false;
        
        // Sample cards for difficulty visualization
        private Rectangle[,] _sampleGrids = new Rectangle[4, 2]; // 4 difficulties, rect + dimensions
        private Color[] _difficultyColors = new Color[] { 
            Color.LightGreen,   // Easy
            Color.Yellow,       // Medium
            Color.Orange,       // Hard
            Color.Red           // Expert
        };
        
        // Constants for limit calculations
        private readonly int[] _attemptsMultiplier = new int[] { 20, 15, 12, 10 }; // Higher for easier levels
        
        public GameScreen(Microsoft.Xna.Framework.Game game, GraphicsDevice graphicsDevice)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        }
        
        public void Initialize()
        {
            // Create input manager
            _inputManager = new InputManager();
            _inputManager.OnClick += HandleClick;
            _inputManager.OnKeyPressed += HandleKeyPress;
            
            // Create player
            _player = new Player("Player 1");
            
            // Calculate UI elements position
            CalculateUIElements();
            
            // Start in menu mode
            _showMenu = true;
        }
        
        private void CalculateUIElements()
        {
            var viewport = _graphicsDevice.Viewport;
            
            // Setup scoreboard area
            _scoreboardRect = new Rectangle(20, 20, 200, 160); // Made taller for better number display
            
            // Setup difficulty buttons - centered horizontally
            int buttonWidth = 150;
            int buttonHeight = 120; // Taller to show sample cards
            int buttonPadding = 20;
            int totalButtonsWidth = (buttonWidth * 4) + (buttonPadding * 3);
            int startX = (viewport.Width - totalButtonsWidth) / 2;
            int buttonY = viewport.Height / 2 - 150;
            
            for (int i = 0; i < 4; i++)
            {
                _difficultyButtons[i] = new Rectangle(
                    startX + (i * (buttonWidth + buttonPadding)),
                    buttonY,
                    buttonWidth,
                    buttonHeight
                );
                
                // Create sample card grid areas
                CalculateSampleGrid(i, _difficultyButtons[i]);
            }
            
            // Setup start button - centered and with a play triangle
            _startButton = new Rectangle(
                viewport.Width / 2 - 100,
                buttonY + buttonHeight + 40,
                200,
                70
            );
        }
        
        private void CalculateSampleGrid(int difficultyIndex, Rectangle buttonRect)
        {
            // Define sample grid sizes for each difficulty
            int rows = 0;
            int cols = 0;
            
            switch (difficultyIndex)
            {
                case 0: // Easy
                    rows = 2;
                    cols = 2;
                    break;
                case 1: // Medium
                    rows = 2;
                    cols = 3;
                    break;
                case 2: // Hard
                    rows = 3;
                    cols = 3;
                    break;
                case 3: // Expert
                    rows = 3;
                    cols = 4;
                    break;
                default:
                    rows = 2;
                    cols = 2;
                    break;
            }
            
            int totalCardWidth = buttonRect.Width - 20;
            int totalCardHeight = buttonRect.Height - 40;
            
            int cardWidth = totalCardWidth / Math.Max(1, cols);  // Prevent divide by zero
            int cardHeight = totalCardHeight / Math.Max(1, rows); // Prevent divide by zero
            
            // Start position for grid
            int startX = buttonRect.X + 10;
            int startY = buttonRect.Y + 30;
            
            // Store grid area and dimensions
            _sampleGrids[difficultyIndex, 0] = new Rectangle(startX, startY, totalCardWidth, totalCardHeight);
            _sampleGrids[difficultyIndex, 1] = new Rectangle(cols, rows, 0, 0); // Store dimensions in a Rectangle
        }
        
        public void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            
            // Create pixel texture for drawing
            _pixel = new Texture2D(_graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            
            // Create renderers
            _cardRenderer = new CardRenderer(_graphicsDevice, _spriteBatch);
            _boardRenderer = new BoardRenderer(_graphicsDevice, _spriteBatch, _cardRenderer);
        }
        
        // Start game with selected difficulty
        private void StartNewGame(DifficultyLevel difficulty)
        {
            _showMenu = false;
            Difficulty = difficulty;
            
            (int rows, int columns) = GetBoardDimensions(difficulty);
            
            // Create game board
            _gameBoard = new GameBoard("main_board", rows, columns);
            
            // Create card deck and generate cards
            _cardDeck = new CardDeck(
                CardType.Heart, CardType.Diamond, CardType.Club, CardType.Spade,
                CardType.Star, CardType.Moon, CardType.Sun, CardType.Cloud
            );
            
            int pairs = (rows * columns) / 2;
            var cards = _cardDeck.GenerateDeck(pairs);
            
            // Add cards to board
            int cardIndex = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (cardIndex < cards.Count)
                    {
                        var card = cards[cardIndex];
                        
                        string id = $"{row}_{col}";
                        // Replace the generated ID with our grid position ID
                        var newCard = new Card(id, card.Type, card.Value);
                        newCard.OnInteraction += CardInteraction;
                        
                        _gameBoard.AddCard(newCard);
                        cardIndex++;
                    }
                }
            }
            
            // Create score calculator with custom calculation using lambda
            _scoreCalculator = new ScoreCalculator((card, attempts) => 
                15 * (1 + (card.IsSpecialCard() ? 2 : 0)) / (attempts > 5 ? 2 : 1));
            
            // Create game logic
            _gameLogic = new GameLogic(_player, _gameBoard, _scoreCalculator);
            _gameLogic.OnScoreChanged += score => {
                Console.WriteLine($"Score changed: +{score}");
                UpdateBoardStats();
            };
            _gameLogic.OnMatchAttempt += (card1, card2, isMatch) => {
                Console.WriteLine($"Match attempt: {isMatch}");
                UpdateBoardStats();
                
                if (!isMatch)
                {
                    // Set timer to flip back
                    _waitingToFlipBack = true;
                    _flipBackTimer = 1.0f; // 1 second delay
                    _flippedCards.Clear();
                    _flippedCards.Add(card1);
                    _flippedCards.Add(card2);
                }
                
                // Check for game over by attempts
                if (_player.Attempts >= _maxAttempts)
                {
                    _gameOver = true;
                    _victory = false;
                    Console.WriteLine("Game Over - Out of attempts!");
                }
            };
            _gameLogic.OnGameStateChanged += state => {
                Console.WriteLine($"Game state changed: {state}");
                if ((state & GameState.Victory) != 0)
                {
                    _gameOver = true;
                    _victory = true;
                    Console.WriteLine("Game Over - Victory!");
                }
            };
            
            // Reset game state
            _gameLogic.StartGame();
            _gameOver = false;
            _victory = false;
            _waitingToFlipBack = false;
            _flippedCards.Clear();
            _elapsedTime = 0;
            
            // Calculate max attempts based on difficulty
            _maxAttempts = 30;
            System.Console.WriteLine(_maxAttempts);
            // Reset player
            _player.Reset();
            
            UpdateBoardStats();
            
            // Setup board renderer
            var viewport = _graphicsDevice.Viewport;
            var boardArea = new Rectangle(0, 0, viewport.Width, viewport.Height);
            
            _boardRenderer.SetBoard(_gameBoard, boardArea, _scoreboardRect);
        }
        
        private void UpdateBoardStats()
        {
            if (_boardRenderer != null)
            {
                _boardRenderer.UpdateStats(_player.Score, _player.Attempts, _player.Matches);
            }
        }
        
        // Tuple return example with deconstructor
        private (int rows, int columns) GetBoardDimensions(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => (3, 4),
                DifficultyLevel.Medium => (4, 4),
                DifficultyLevel.Hard => (4, 6),
                DifficultyLevel.Expert => (6, 6),
                _ => (4, 4) // Default to medium
            };
        }
        
        public void Update(GameTime gameTime)
        {
            // Update input manager
            _inputManager.Update(gameTime);
            
            if (_showMenu)
            {
                // Check if mouse is hovering over start button
                var mouseState = Mouse.GetState();
                var mousePoint = new Point(mouseState.X, mouseState.Y);
                _startHover = _startButton.Contains(mousePoint);
                
                return;
            }
            
            // Only update game logic if game is active
            if (!_gameOver)
            {
                // Update game timer
                _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Handle flip back timer
                if (_waitingToFlipBack)
                {
                    _flipBackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_flipBackTimer <= 0)
                    {
                        _waitingToFlipBack = false;
                        
                        // Manually flip back the cards
                        foreach (var card in _flippedCards)
                        {
                            card.Hide();
                        }
                        
                        // Clear flipped cards list
                        _flippedCards.Clear();
                        
                        // Tell the game logic we've cleared the cards
                        if (_gameLogic is GameLogic gameLogic)
                        {
                            gameLogic.ClearRevealedCards();
                        }
                        
                        Console.WriteLine("Cards have been flipped back - ready for new selections");
                    }
                }
                
                // Update game board
                _gameBoard?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }
        
        public void Draw(GameTime gameTime)
        {
            if (_showMenu)
            {
                DrawMenu();
                return;
            }
            
            // Draw game board
            _boardRenderer?.Draw(gameTime);
            
            // Draw additional UI elements
            _spriteBatch.Begin();
            
            // Draw scoreboard details
            DrawScoreboard();
            
            // Draw game over message if needed
            if (_gameOver)
            {
                DrawGameOverScreen();
            }
            
            _spriteBatch.End();
        }
        
        private void DrawMenu()
        {
            _spriteBatch.Begin();
            
            // Draw background
            var viewport = _graphicsDevice.Viewport;
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(100, 149, 237));
            
            // Draw title - card icons in a row
            int titleY = 50;
            int cardSize = 70;
            int spacing = 10;
            int totalWidth = 5 * cardSize + 4 * spacing;
            int startX = (viewport.Width - totalWidth) / 2;
            
            // Draw 5 cards with different colors as a title
            Color[] titleColors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple };
            for (int i = 0; i < 5; i++)
            {
                var cardRect = new Rectangle(startX + i * (cardSize + spacing), titleY, cardSize, cardSize);
                _spriteBatch.Draw(_pixel, cardRect, titleColors[i]);
                DrawBorder(cardRect, 3, Color.White);
                
                // Draw white square in middle (like cards in the game)
                int innerSize = 15;
                var innerRect = new Rectangle(
                    cardRect.X + (cardRect.Width - innerSize) / 2,
                    cardRect.Y + (cardRect.Height - innerSize) / 2,
                    innerSize, innerSize);
                _spriteBatch.Draw(_pixel, innerRect, Color.White);
            }
            
            // Draw difficulty options
            for (int i = 0; i < 4; i++)
            {
                // Draw button background
                Color buttonColor = (int)Difficulty == i ? _difficultyColors[i] : Color.LightGray;
                _spriteBatch.Draw(_pixel, _difficultyButtons[i], buttonColor);
                
                // Draw button border
                DrawBorder(_difficultyButtons[i], 2, Color.Black);
                
                // Draw number of stars to indicate difficulty
                int stars = i + 1;
                int starSize = 20;
                int starsWidth = stars * starSize + (stars - 1) * 5;
                int starStartX = _difficultyButtons[i].X + (_difficultyButtons[i].Width - starsWidth) / 2;
                
                for (int j = 0; j < stars; j++)
                {
                    var starRect = new Rectangle(
                        starStartX + j * (starSize + 5),
                        _difficultyButtons[i].Y + 5,
                        starSize, starSize);
                    DrawStar(starRect, Color.Gold);
                }
                
                // Draw sample card grid
                var gridArea = _sampleGrids[i, 0];
                var gridDims = _sampleGrids[i, 1];
                int cols = gridDims.X;
                int rows = gridDims.Y;
                
                // Ensure we don't divide by zero
                if (cols > 0 && rows > 0)
                {
                    int cardWidth = gridArea.Width / cols;
                    int cardHeight = gridArea.Height / rows;
                    
                    for (int row = 0; row < rows; row++)
                    {
                        for (int col = 0; col < cols; col++)
                        {
                            var cardRect = new Rectangle(
                                gridArea.X + col * cardWidth,
                                gridArea.Y + row * cardHeight,
                                cardWidth - 2,
                                cardHeight - 2
                            );
                            
                            _spriteBatch.Draw(_pixel, cardRect, Color.DarkBlue);
                            DrawBorder(cardRect, 1, Color.White);
                        }
                    }
                }
            }
            
            // Draw start button with play triangle
            Color startColor = _startHover ? Color.Green : Color.LightGreen;
            _spriteBatch.Draw(_pixel, _startButton, startColor);
            DrawBorder(_startButton, 3, Color.Black);
            
            // Draw triangle (play button) inside
            DrawPlayTriangle(_startButton, Color.White);
            
            _spriteBatch.End();
        }
        
        private void DrawStar(Rectangle rect, Color color)
        {
            // Simple representation of a star using rectangles
            // Horizontal bar
            _spriteBatch.Draw(_pixel, new Rectangle(
                rect.X, 
                rect.Y + rect.Height / 3, 
                rect.Width, 
                rect.Height / 3), color);
                
            // Vertical bar
            _spriteBatch.Draw(_pixel, new Rectangle(
                rect.X + rect.Width / 3, 
                rect.Y, 
                rect.Width / 3, 
                rect.Height), color);
        }
        
        private void DrawPlayTriangle(Rectangle rect, Color color)
        {
            // Draw a triangle pointing right (like a play button)
            int triangleWidth = rect.Width / 2;
            int triangleHeight = rect.Height - 20;
            int startX = rect.X + (rect.Width - triangleWidth) / 2;
            int startY = rect.Y + (rect.Height - triangleHeight) / 2;
            
            // Draw a triangle using slices
            for (int i = 0; i < triangleWidth; i++)
            {
                int height = (int)((float)i / triangleWidth * triangleHeight);
                int y = startY + (triangleHeight - height) / 2;
                
                _spriteBatch.Draw(_pixel, new Rectangle(
                    startX + i, 
                    y, 
                    1, 
                    height), color);
            }
        }
        
        private void DrawScoreboard()
        {
            // Background for scoreboard
            _spriteBatch.Draw(_pixel, _scoreboardRect, new Color(30, 30, 50, 220));
            DrawBorder(_scoreboardRect, 2, Color.White);
            
            // Calculate layout
            int padding = 10;
            int labelWidth = 80;
            int valueWidth = _scoreboardRect.Width - labelWidth - padding * 3;
            int itemHeight = 30;
            int spacing = 5;
            
            // Draw score item
            var scoreLabel = new Rectangle(_scoreboardRect.X + padding, _scoreboardRect.Y + padding, labelWidth, itemHeight);
            _spriteBatch.Draw(_pixel, scoreLabel, new Color(0, 100, 0));
            DrawBorder(scoreLabel, 1, Color.White);
            DrawTextIcon(scoreLabel, "SCORE", Color.White);
            
            var scoreValue = new Rectangle(scoreLabel.X + labelWidth + padding, scoreLabel.Y, valueWidth, itemHeight);
            _spriteBatch.Draw(_pixel, scoreValue, new Color(0, 50, 0));
            DrawDigitDisplay(scoreValue, _player.Score, Color.LightGreen);
            
            // Draw attempts item
            var attemptsLabel = new Rectangle(_scoreboardRect.X + padding, scoreLabel.Y + itemHeight + spacing, labelWidth, itemHeight);
            _spriteBatch.Draw(_pixel, attemptsLabel, new Color(100, 0, 0));
            DrawBorder(attemptsLabel, 1, Color.White);
            DrawTextIcon(attemptsLabel, "MOVES", Color.White);
            
            var attemptsValue = new Rectangle(attemptsLabel.X + labelWidth + padding, attemptsLabel.Y, valueWidth, itemHeight);
            _spriteBatch.Draw(_pixel, attemptsValue, new Color(50, 0, 0));
            DrawDigitDisplay(attemptsValue, _player.Attempts, Color.Red, _maxAttempts);
            
            // Draw matches item
            var matchesLabel = new Rectangle(_scoreboardRect.X + padding, attemptsLabel.Y + itemHeight + spacing, labelWidth, itemHeight);
            _spriteBatch.Draw(_pixel, matchesLabel, new Color(0, 0, 100));
            DrawBorder(matchesLabel, 1, Color.White);
            DrawTextIcon(matchesLabel, "MATCH", Color.White);
            
            int totalPairs = Math.Max(1, _gameBoard?.Cards.Count / 2 ?? 1); // Prevent division by zero
            var matchesValue = new Rectangle(matchesLabel.X + labelWidth + padding, matchesLabel.Y, valueWidth, itemHeight);
            _spriteBatch.Draw(_pixel, matchesValue, new Color(0, 0, 50));
            DrawDigitDisplay(matchesValue, _player.Matches, Color.LightBlue, totalPairs);
            
            // Draw time item
            var timeLabel = new Rectangle(_scoreboardRect.X + padding, matchesLabel.Y + itemHeight + spacing, labelWidth, itemHeight);
            _spriteBatch.Draw(_pixel, timeLabel, new Color(100, 50, 0));
            DrawBorder(timeLabel, 1, Color.White);
            DrawTextIcon(timeLabel, "TIME", Color.White);
            
            var timeValue = new Rectangle(timeLabel.X + labelWidth + padding, timeLabel.Y, valueWidth, itemHeight);
            _spriteBatch.Draw(_pixel, timeValue, new Color(50, 25, 0));
            DrawTimeDisplay(timeValue, _elapsedTime, Color.Orange);
        }
        
        private void DrawTextIcon(Rectangle area, string text, Color color)
        {
            // Draw text icon based on first letter
            if (string.IsNullOrEmpty(text) || area.Width <= 0 || area.Height <= 0)
                return;
                
            char firstChar = text[0];
            
            // Center position
            int x = area.X + area.Width / 2;
            int y = area.Y + area.Height / 2;
            int size = Math.Min(area.Width, area.Height) / 2;
            
            switch (firstChar)
            {
                case 'S': // Score - Dollar sign
                    // Vertical line
                    _spriteBatch.Draw(_pixel, new Rectangle(x - 1, y - size, 3, size * 2), color);
                    // Top horizontal
                    _spriteBatch.Draw(_pixel, new Rectangle(x - size / 2, y - size + 2, size, 3), color);
                    // Middle horizontal
                    _spriteBatch.Draw(_pixel, new Rectangle(x - size / 2, y - 1, size, 3), color);
                    // Bottom horizontal
                    _spriteBatch.Draw(_pixel, new Rectangle(x - size / 2, y + size - 4, size, 3), color);
                    break;
                    
                case 'M': // Moves or Match - Arrow or M
                    // Draw M
                    int lineWidth = 3;
                    int height = size;
                    // Left vertical
                    _spriteBatch.Draw(_pixel, new Rectangle(x - size + 2, y - height/2, lineWidth, height), color);
                    // Right vertical
                    _spriteBatch.Draw(_pixel, new Rectangle(x + size - 5, y - height/2, lineWidth, height), color);
                    // Middle diagonals
                    for (int i = 0; i < size/2; i++)
                    {
                        float ratio = (float)i / (size/2);
                        // Left diagonal
                        _spriteBatch.Draw(_pixel, new Rectangle(
                            x - size + 5 + i, 
                            y - height/2 + (int)(ratio * height/2), 
                            lineWidth, lineWidth), color);
                        // Right diagonal
                        _spriteBatch.Draw(_pixel, new Rectangle(
                            x + i, 
                            y - height/2 + (int)((1-ratio) * height/2), 
                            lineWidth, lineWidth), color);
                    }
                    break;
                    
                case 'T': // Time - Clock
                    // Draw circle
                    int radius = size - 2;
                    for (int angle = 0; angle < 360; angle += 15)
                    {
                        double radians = angle * Math.PI / 180;
                        int pointX = x + (int)(Math.Cos(radians) * radius);
                        int pointY = y + (int)(Math.Sin(radians) * radius);
                        
                        _spriteBatch.Draw(_pixel, new Rectangle(pointX - 1, pointY - 1, 3, 3), color);
                    }
                    
                    // Hour hand
                    _spriteBatch.Draw(_pixel, new Rectangle(x - 1, y - 1, 3, -radius/2), color);
                    
                    // Minute hand
                    _spriteBatch.Draw(_pixel, new Rectangle(x, y, 2, radius/2), color);
                    break;
            }
        }
        // Add these methods to the GameScreen class

private void DrawMenuIcon(Rectangle area, Color color)
{
    // Draw 3 horizontal lines to indicate menu
    int lineWidth = area.Width - 40;
    int lineHeight = 6;
    int spacing = 12;
    int startX = area.X + (area.Width - lineWidth) / 2;
    int startY = area.Y + (area.Height - (3 * lineHeight + 2 * spacing)) / 2;
    
    for (int i = 0; i < 3; i++)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(
            startX,
            startY + i * (lineHeight + spacing),
            lineWidth,
            lineHeight), color);
    }
}

private void DrawRestartIcon(Rectangle area, Color color)
{
    // Draw a circular arrow to indicate restart
    int size = Math.Min(area.Width, area.Height) - 20;
    int x = area.X + (area.Width - size) / 2;
    int y = area.Y + (area.Height - size) / 2;
    int thickness = 5;
    
    // Draw 3/4 of a circle
    for (int angle = 0; angle < 270; angle += 15)
    {
        double radians = angle * Math.PI / 180;
        int pointX = x + size / 2 + (int)(Math.Cos(radians) * size / 2);
        int pointY = y + size / 2 + (int)(Math.Sin(radians) * size / 2);
        
        _spriteBatch.Draw(_pixel, new Rectangle(pointX - thickness/2, pointY - thickness/2, thickness, thickness), color);
    }
    
    // Draw arrow at the end
    int arrowSize = 10;
    _spriteBatch.Draw(_pixel, new Rectangle(
        x + size - arrowSize, 
        y + size / 2 - arrowSize, 
        arrowSize * 2, 
        arrowSize * 2), color);
}
        private void DrawDigitDisplay(Rectangle area, int value, Color color, int maxValue = 0)
        {
            // Background
            _spriteBatch.Draw(_pixel, area, new Color(10, 10, 10));
            DrawBorder(area, 1, color);
            
            // Calculate digit size and spacing
            int height = area.Height - 6;
            int width = height * 2 / 3;
            int maxDigits = 3; // Maximum number of digits to display
            int totalWidth = Math.Min(maxDigits * (width + 2), area.Width - 10);
            int x = area.X + area.Width - totalWidth - 5;
            int y = area.Y + 3;
            
            // If maxValue is provided, draw progress bar
            if (maxValue > 0)
            {
                float progress = (float)value / maxValue;
                int progressWidth = (int)(area.Width * 0.7f * progress);
                var progressRect = new Rectangle(area.X + 4, area.Y + area.Height - 5, progressWidth, 2);
                _spriteBatch.Draw(_pixel, progressRect, color);
            }
            
            // Limit to reasonable value
            value = Math.Min(value, 999);
            
            // Extract digits
            int[] digits = new int[3];
            digits[0] = value / 100;
            digits[1] = (value / 10) % 10;
            digits[2] = value % 10;
            
            // Draw each digit, right aligned
            bool leadingZero = true;
            for (int i = 0; i < 3; i++)
            {
                // Skip leading zeros unless it's the last digit
                if (leadingZero && digits[i] == 0 && i < 2)
                    continue;
                    
                leadingZero = false;
                
                Rectangle digitRect = new Rectangle(x, y, width, height);
                DrawSevenSegmentDigit(digitRect, digits[i], color);
                x += width + 2;
            }
        }
        
        private void DrawTimeDisplay(Rectangle area, float seconds, Color color)
        {
            // Background
            _spriteBatch.Draw(_pixel, area, new Color(10, 10, 10));
            DrawBorder(area, 1, color);
            
            // Calculate minutes and seconds
            int totalSeconds = (int)seconds;
            int minutes = totalSeconds / 60;
            int displaySeconds = totalSeconds % 60;
            
            // Calculate digit size and spacing
            int height = area.Height - 6;
            int width = height * 2 / 3;
            int x = area.X + 5;
            int y = area.Y + 3;
            
            // Draw minutes (up to 2 digits)
            int[] minDigits = new int[2];
            minDigits[0] = minutes / 10;
            minDigits[1] = minutes % 10;
            
            // Draw minute digits
            bool leadingZero = true;
            for (int i = 0; i < 2; i++)
            {
                // Skip leading zero unless it's the last digit
                if (leadingZero && minDigits[i] == 0 && i < 1)
                    continue;
                    
                leadingZero = false;
                
                Rectangle digitRect = new Rectangle(x, y, width, height);
                DrawSevenSegmentDigit(digitRect, minDigits[i], color);
                x += width + 2;
            }
            
            // Draw colon
            _spriteBatch.Draw(_pixel, new Rectangle(x, y + height/3 - 1, 2, 2), color);
            _spriteBatch.Draw(_pixel, new Rectangle(x, y + height*2/3 - 1, 2, 2), color);
            x += 4;
            
            // Draw seconds (always 2 digits)
            int[] secDigits = new int[2];
            secDigits[0] = displaySeconds / 10;
            secDigits[1] = displaySeconds % 10;
            
            for (int i = 0; i < 2; i++)
            {
                Rectangle digitRect = new Rectangle(x, y, width, height);
                DrawSevenSegmentDigit(digitRect, secDigits[i], color);
                x += width + 2;
            }
        }
        
        private void DrawSevenSegmentDigit(Rectangle area, int digit, Color color)
        {
            // Ensure digit is 0-9
            digit = Math.Max(0, Math.Min(9, digit));
            
            int thickness = 2;
            int width = area.Width;
            int height = area.Height;
            
            // Segment definitions - which segments are lit for each digit
            bool[][] segments = new bool[][]
            {
                new bool[] { true, true, true, true, true, true, false },       // 0
                new bool[] { false, true, true, false, false, false, false },   // 1
                new bool[] { true, true, false, true, true, false, true },      // 2
                new bool[] { true, true, true, true, false, false, true },      // 3
                new bool[] { false, true, true, false, false, true, true },     // 4
                new bool[] { true, false, true, true, false, true, true },      // 5
                new bool[] { true, false, true, true, true, true, true },       // 6
                new bool[] { true, true, true, false, false, false, false },    // 7
                new bool[] { true, true, true, true, true, true, true },        // 8
                new bool[] { true, true, true, true, false, true, true }        // 9
            };
            
            // Draw each segment if it should be lit
            if (segments[digit][0]) // Top
                _spriteBatch.Draw(_pixel, new Rectangle(area.X + thickness, area.Y, width - thickness * 2, thickness), color);
                
            if (segments[digit][1]) // Top-right
                _spriteBatch.Draw(_pixel, new Rectangle(area.X + width - thickness, area.Y, thickness, height / 2), color);
                
            if (segments[digit][2]) // Bottom-right
                _spriteBatch.Draw(_pixel, new Rectangle(area.X + width - thickness, area.Y + height / 2, thickness, height / 2), color);
                
            if (segments[digit][3]) // Bottom
                _spriteBatch.Draw(_pixel, new Rectangle(area.X + thickness, area.Y + height - thickness, width - thickness * 2, thickness), color);
                
            if (segments[digit][4]) // Bottom-left
                _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + height / 2, thickness, height / 2), color);
                
            if (segments[digit][5]) // Top-left
                _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, height / 2), color);
                
            if (segments[digit][6]) // Middle
                _spriteBatch.Draw(_pixel, new Rectangle(area.X + thickness, area.Y + height / 2 - thickness / 2, width - thickness * 2, thickness), color);
        }
        
        private void DrawGameOverScreen()
        {
            var viewport = _graphicsDevice.Viewport;
            
            // Semi-transparent overlay
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(0, 0, 0, 180));
            
            // Draw game over panel
            int panelWidth = 500;
            int panelHeight = 350;
            var gameOverPanel = new Rectangle(viewport.Width / 2 - panelWidth / 2, viewport.Height / 2 - panelHeight / 2, panelWidth, panelHeight);
            
            // Panel background with gradient effect
            Color panelColor = _victory ? new Color(0, 70, 0, 230) : new Color(70, 0, 0, 230);
            _spriteBatch.Draw(_pixel, gameOverPanel, panelColor);
            
            // Panel border
            Color borderColor = _victory ? Color.Gold : Color.Red;
            DrawBorder(gameOverPanel, 4, borderColor);
            
            // Draw header text
            int headerHeight = 60;
            var headerRect = new Rectangle(gameOverPanel.X + 20, gameOverPanel.Y + 20, panelWidth - 40, headerHeight);
            _spriteBatch.Draw(_pixel, headerRect, _victory ? new Color(0, 100, 0) : new Color(100, 0, 0));
            DrawBorder(headerRect, 2, Color.White);
            
            // Draw header text
            string headerText = _victory ? "VICTORY!" : "GAME OVER";
            DrawLargeText(headerRect, headerText, Color.White);
            
            // Draw stats section title
            int sectionTitleY = headerRect.Y + headerRect.Height + 20;
            var statsTitleRect = new Rectangle(gameOverPanel.X + 40, sectionTitleY, panelWidth - 80, 30);
            _spriteBatch.Draw(_pixel, statsTitleRect, new Color(40, 40, 40));
            DrawBorder(statsTitleRect, 1, Color.White);
            DrawSimpleText(statsTitleRect, "YOUR STATS", Color.White);
            
            // Draw stats boxes
            int boxWidth = 100;
            int boxHeight = 80;
            int boxSpacing = 20;
            int boxY = statsTitleRect.Y + statsTitleRect.Height + 20;
            
            // Calculate positions for 3 boxes
            int totalBoxesWidth = 3 * boxWidth + 2 * boxSpacing;
            int boxStartX = gameOverPanel.X + (panelWidth - totalBoxesWidth) / 2;
            
            // Score box
            var scoreBox = new Rectangle(boxStartX, boxY, boxWidth, boxHeight);
            _spriteBatch.Draw(_pixel, scoreBox, new Color(0, 0, 40));
            DrawBorder(scoreBox, 2, Color.LightBlue);
            
            // Score label
            var scoreLabelRect = new Rectangle(scoreBox.X, scoreBox.Y, boxWidth, 20);
            DrawSimpleText(scoreLabelRect, "SCORE", Color.White);
            
            // Score value
            var scoreValueRect = new Rectangle(scoreBox.X + 5, scoreBox.Y + 25, boxWidth - 10, boxHeight - 30);
            DrawLargeDigit(scoreValueRect, _player.Score, Color.LightBlue);
            
            // Matches box
            var matchesBox = new Rectangle(boxStartX + boxWidth + boxSpacing, boxY, boxWidth, boxHeight);
            _spriteBatch.Draw(_pixel, matchesBox, new Color(0, 40, 0));
            DrawBorder(matchesBox, 2, Color.LightGreen);
            
            // Matches label
            var matchesLabelRect = new Rectangle(matchesBox.X, matchesBox.Y, boxWidth, 20);
            DrawSimpleText(matchesLabelRect, "MATCHES", Color.White);
            
            // Matches value
            var matchesValueRect = new Rectangle(matchesBox.X + 5, matchesBox.Y + 25, boxWidth - 10, boxHeight - 30);
            DrawLargeDigit(matchesValueRect, _player.Matches, Color.LightGreen);
            
            // Time box
            var timeBox = new Rectangle(boxStartX + 2 * (boxWidth + boxSpacing), boxY, boxWidth, boxHeight);
            _spriteBatch.Draw(_pixel, timeBox, new Color(40, 20, 0));
            DrawBorder(timeBox, 2, Color.Orange);
            
            // Time label
            var timeLabelRect = new Rectangle(timeBox.X, timeBox.Y, boxWidth, 20);
            DrawSimpleText(timeLabelRect, "TIME", Color.White);
            
            // Time value (minutes)
            var timeValueRect = new Rectangle(timeBox.X + 5, timeBox.Y + 25, boxWidth - 10, boxHeight - 30);
            DrawLargeDigit(timeValueRect, (int)_elapsedTime / 60, Color.Orange);
            
            // Draw buttons section
            int buttonsY = boxY + boxHeight + 30;
            
            // Draw menu button
            var menuButton = new Rectangle(gameOverPanel.X + (panelWidth - 320) / 2, buttonsY, 150, 50);
            _spriteBatch.Draw(_pixel, menuButton, new Color(60, 60, 60));
            DrawBorder(menuButton, 2, Color.White);
            DrawMenuIcon(menuButton, Color.White);
            
            // Draw restart button
            var restartButton = new Rectangle(menuButton.X + menuButton.Width + 20, buttonsY, 150, 50);
            _spriteBatch.Draw(_pixel, restartButton, new Color(60, 60, 60));
            DrawBorder(restartButton, 2, Color.White);
            DrawRestartIcon(restartButton, Color.White);
        }
        
        private void DrawLargeText(Rectangle area, string text, Color color)
        {
            // Simple block style letters
            int letterHeight = area.Height - 10;
            int letterWidth = letterHeight * 2 / 3;
            int spacing = 5;
            
            int totalWidth = text.Length * (letterWidth + spacing) - spacing;
            int startX = area.X + (area.Width - totalWidth) / 2;
            int startY = area.Y + (area.Height - letterHeight) / 2;
            
            for (int i = 0; i < text.Length; i++)
            {
                int x = startX + i * (letterWidth + spacing);
                DrawBlockLetter(new Rectangle(x, startY, letterWidth, letterHeight), text[i], color);
            }
        }
        
        private void DrawSimpleText(Rectangle area, string text, Color color)
        {
            // Even simpler block style for smaller text
            int letterHeight = area.Height - 6;
            int letterWidth = letterHeight * 2 / 3;
            int spacing = 2;
            
            int totalWidth = text.Length * (letterWidth + spacing) - spacing;
            int startX = area.X + (area.Width - totalWidth) / 2;
            int startY = area.Y + (area.Height - letterHeight) / 2;
            
            for (int i = 0; i < text.Length; i++)
            {
                int x = startX + i * (letterWidth + spacing);
                DrawBlockLetter(new Rectangle(x, startY, letterWidth, letterHeight), text[i], color);
            }
        }
        
        private void DrawBlockLetter(Rectangle area, char letter, Color color)
        {
            int thickness = Math.Max(1, area.Width / 8);
            letter = char.ToUpper(letter);
            
            switch (letter)
            {
                case 'A':
                    // Left leg
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height / 2, thickness, area.Height / 2), color);
                    // Right leg
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width - thickness, area.Y + area.Height / 2, thickness, area.Height / 2), color);
                    // Top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, thickness), color);
                    // Middle
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height / 2, area.Width, thickness), color);
                    break;
                    
                case 'C':
                    // Left side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, area.Height), color);
                    // Top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, thickness), color);
                    // Bottom
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height - thickness, area.Width, thickness), color);
                    break;
                    
                case 'E':
                    // Left side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, area.Height), color);
                    // Top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, thickness), color);
                    // Middle
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height / 2 - thickness / 2, area.Width, thickness), color);
                    // Bottom
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height - thickness, area.Width, thickness), color);
                    break;
                    
                case 'G':
                    // Left side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, area.Height), color);
                    // Top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, thickness), color);
                    // Bottom
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height - thickness, area.Width, thickness), color);
                    // Right bottom
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width - thickness, area.Y + area.Height / 2, thickness, area.Height / 2), color);
                    // Middle (shorter)
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width / 2, area.Y + area.Height / 2, area.Width / 2, thickness), color);
                    break;
                    
                case 'I':
                    // Vertical line
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width / 2 - thickness / 2, area.Y, thickness, area.Height), color);
                    break;
                    
                case 'M':
                    // Left side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, area.Height), color);
                    // Right side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width - thickness, area.Y, thickness, area.Height), color);
                    // Middle diagonals (simplified)
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width / 4, area.Y, thickness, area.Height / 2), color);
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width * 3 / 4 - thickness, area.Y, thickness, area.Height / 2), color);
                    break;
                    
                case 'O':
                    // Left side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, area.Height), color);
                    // Right side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width - thickness, area.Y, thickness, area.Height), color);
                    // Top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, thickness), color);
                    // Bottom
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height - thickness, area.Width, thickness), color);
                    break;
                    
                case 'R':
                    // Left side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, area.Height), color);
                    // Top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, thickness), color);
                    // Middle
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height / 2 - thickness / 2, area.Width, thickness), color);
                    // Right top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width - thickness, area.Y, thickness, area.Height / 2), color);
                    // Diagonal bottom
                    for (int i = 0; i < area.Width / 2; i++)
                    {
                        float ratio = (float)i / (area.Width / 2);
                        _spriteBatch.Draw(_pixel, new Rectangle(
                            area.X + area.Width / 2 + i, 
                            area.Y + area.Height / 2 + (int)(ratio * area.Height / 2), 
                            thickness, thickness), color);
                    }
                    break;
                    
                case 'S':
                    // Top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, thickness), color);
                    // Middle
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height / 2 - thickness / 2, area.Width, thickness), color);
                    // Bottom
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height - thickness, area.Width, thickness), color);
                    // Top left
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, area.Height / 2), color);
                    // Bottom right
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width - thickness, area.Y + area.Height / 2, thickness, area.Height / 2), color);
                    break;
                    
                case 'T':
                    // Top
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, area.Width, thickness), color);
                    // Middle vertical
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width / 2 - thickness / 2, area.Y, thickness, area.Height), color);
                    break;
                    
                case 'U':
                    // Left side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y, thickness, area.Height), color);
                    // Right side
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X + area.Width - thickness, area.Y, thickness, area.Height), color);
                    // Bottom
                    _spriteBatch.Draw(_pixel, new Rectangle(area.X, area.Y + area.Height - thickness, area.Width, thickness), color);
                    break;
                    
                case 'V':
                    // Diagonal left
                    for (int i = 0; i < area.Height; i++)
                    {
                        float ratio = (float)i / area.Height;
                        _spriteBatch.Draw(_pixel, new Rectangle(
                            area.X + (int)(ratio * area.Width / 2),
                            area.Y + i,
                            thickness, thickness), color);
                    }
                    // Diagonal right
                    for (int i = 0; i < area.Height; i++)
                    {
                        float ratio = (float)i / area.Height;
                        _spriteBatch.Draw(_pixel, new Rectangle(
                            area.X + area.Width - (int)(ratio * area.Width / 2) - thickness,
                            area.Y + i,
                            thickness, thickness), color);
                    }
                    break;
                    
                case 'Y':
                    // Top left diagonal
                    for (int i = 0; i < area.Height / 2; i++)
                    {
                        float ratio = (float)i / (area.Height / 2);
                        _spriteBatch.Draw(_pixel, new Rectangle(
                            area.X + (int)(ratio * area.Width / 2),
                            area.Y + i,
                            thickness, thickness), color);
                    }
                    // Top right diagonal
                    for (int i = 0; i < area.Height / 2; i++)
                    {
                        float ratio = (float)i / (area.Height / 2);
                        _spriteBatch.Draw(_pixel, new Rectangle(
                            area.X + area.Width - (int)(ratio * area.Width / 2) - thickness,
                            area.Y + i,
                            thickness, thickness), color);
                    }
                    // Bottom vertical
                    _spriteBatch.Draw(_pixel, new Rectangle(
                        area.X + area.Width / 2 - thickness / 2,
                        area.Y + area.Height / 2,
                        thickness,
                        area.Height / 2), color);
                    break;
                    
                case '!':
                    // Exclamation point
                    _spriteBatch.Draw(_pixel, new Rectangle(
                        area.X + area.Width / 2 - thickness / 2,
                        area.Y,
                        thickness,
                        area.Height * 2 / 3), color);
                    
                    _spriteBatch.Draw(_pixel, new Rectangle(
                        area.X + area.Width / 2 - thickness,
                        area.Y + area.Height * 4 / 5,
                        thickness * 2,
                        thickness * 2), color);
                    break;
                    
                case ' ':
                    // Space - do nothing
                    break;
                    
                default:
                    // Unsupported character - draw a rectangle
                    _spriteBatch.Draw(_pixel, new Rectangle(
                        area.X + area.Width / 4,
                        area.Y + area.Height / 4,
                        area.Width / 2,
                        area.Height / 2), color);
                    break;
            }
        }
        
        private void DrawLargeDigit(Rectangle area, int value, Color color)
        {
            // Draw a large digit centered in the area
            // Convert to string to handle multi-digit numbers
            string valueStr = value.ToString();
            
            // Calculate size for digits
            int digitHeight = area.Height - 10;
            int digitWidth = (int)(digitHeight * 0.6);
            int spacing = 5;
            
            int totalWidth = valueStr.Length * (digitWidth + spacing) - spacing;
            int startX = area.X + (area.Width - totalWidth) / 2;
            int startY = area.Y + (area.Height - digitHeight) / 2;
            
            for (int i = 0; i < valueStr.Length; i++)
            {
                int digitValue = valueStr[i] - '0';
                Rectangle digitRect = new Rectangle(
                    startX + i * (digitWidth + spacing),
                    startY,
                    digitWidth,
                    digitHeight);
                    
                DrawSevenSegmentDigit(digitRect, digitValue, color);
            }
        }
        
        private void DrawBorder(Rectangle rectangle, int thickness, Color color)
        {
            // Top
            _spriteBatch.Draw(_pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            // Bottom
            _spriteBatch.Draw(_pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
            // Left
            _spriteBatch.Draw(_pixel, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            // Right
            _spriteBatch.Draw(_pixel, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
        }
        
        private void HandleClick(Point position)
        {
            if (_showMenu)
            {
                // Check for difficulty button clicks
                for (int i = 0; i < 4; i++)
                {
                    if (_difficultyButtons[i].Contains(position))
                    {
                        Difficulty = (DifficultyLevel)i;
                        Console.WriteLine($"Difficulty set to {Difficulty}");
                        return;
                    }
                }
                
                // Check for start button click
                if (_startButton.Contains(position))
                {
                    StartNewGame(Difficulty);
                    return;
                }
                
                return;
            }
            
            if (_gameOver)
            {
                // Game over screen - check for restart or menu button clicks
                var viewport = _graphicsDevice.Viewport;
                int panelWidth = 500;
                int panelHeight = 350;
                var gameOverPanel = new Rectangle(viewport.Width / 2 - panelWidth / 2, viewport.Height / 2 - panelHeight / 2, panelWidth, panelHeight);
                
                // Get button positions
                int boxY = gameOverPanel.Y + 170;
                int buttonsY = boxY + 80 + 30;
                
                // Check for menu button click
                var menuButton = new Rectangle(gameOverPanel.X + (panelWidth - 320) / 2, buttonsY, 150, 50);
                if (menuButton.Contains(position))
                {
                    _showMenu = true;
                    return;
                }
                
                // Check for restart button click
                var restartButton = new Rectangle(menuButton.X + menuButton.Width + 20, buttonsY, 150, 50);
                if (restartButton.Contains(position))
                {
                    StartNewGame(Difficulty);
                    return;
                }
                
                return;
            }
            
            // Don't process clicks if we're waiting to flip cards back
            if (_waitingToFlipBack)
            {
                Console.WriteLine("Click ignored - waiting to flip back");
                return;
            }
                
            // Check if click is on a card
            if (_cardRenderer.TryGetCardAtPosition(position, out string cardId) && cardId != null)
            {
                CardInteraction(cardId);
            }
        }
        
        private void HandleKeyPress(Keys key)
        {
            if (_showMenu)
            {
                // Menu controls
                if (key >= Keys.D1 && key <= Keys.D4)
                {
                    int difficultyValue = key - Keys.D1;
                    Difficulty = (DifficultyLevel)difficultyValue;
                }
                else if (key == Keys.Enter)
                {
                    StartNewGame(Difficulty);
                }
                return;
            }
            
            if (_gameOver)
            {
                // Game over screen controls
                if (key == Keys.R)
                {
                    StartNewGame(Difficulty);
                }
                else if (key == Keys.M)
                {
                    _showMenu = true;
                }
                return;
            }
            
            // In-game controls
            if (key == Keys.R)
            {
                StartNewGame(Difficulty);
            }
            else if (key == Keys.Escape)
            {
                _showMenu = true;
            }
        }
        
        private void CardInteraction(string cardId)
        {
            _gameLogic.RevealCard(cardId);
        }
        
        // IFormattable implementation
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return format?.ToLower() switch
            {
                "d" => $"Difficulty: {Difficulty}",
                "s" => $"Score: {_player?.Score ?? 0}",
                "m" => $"Matches: {_player?.Matches ?? 0}/{_gameBoard?.Cards.Count / 2 ?? 0}",
                "a" => $"Attempts: {_player?.Attempts ?? 0}/{_maxAttempts}",
                _ => $"Memory Game - {Difficulty} mode - Score: {_player?.Score ?? 0}"
            };
        }
        
        public override string ToString()
        {
            return ToString(null, null);
        }
    }
}
