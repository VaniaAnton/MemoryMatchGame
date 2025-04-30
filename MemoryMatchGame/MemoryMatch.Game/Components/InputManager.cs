using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MemoryMatch.Game.Components
{
    // Delegate example for input events
    public delegate void InputEvent(Point position);
    
    public class InputManager
    {
        // Delegates/lambda usage
        public event InputEvent OnClick;
        public event Action<Keys> OnKeyPressed;
        
        // Example of static constructor
        static InputManager()
        {
            Console.WriteLine("InputManager class initialized");
        }
        
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        
        // Bitwise flags for input states
        [Flags]
        public enum InputState
        {
            None = 0,
            LeftMouseDown = 1 << 0,
            RightMouseDown = 1 << 1,
            MouseMoved = 1 << 2,
            KeyPressed = 1 << 3,
            KeyReleased = 1 << 4
        }
        
        public InputState CurrentState { get; private set; } = InputState.None;
        
        // Method with out parameter
        public bool TryGetMousePosition(out Point position)
        {
            position = new Point(_currentMouseState.X, _currentMouseState.Y);
            return true;
        }
        
        // Method with default arguments
        public void Update(GameTime gameTime, bool processKeyboard = true)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            
            if (processKeyboard)
            {
                _previousKeyboardState = _currentKeyboardState;
                _currentKeyboardState = Keyboard.GetState();
                
                // Using pattern matching
                foreach (Keys key in _currentKeyboardState.GetPressedKeys())
                {
                    if (_previousKeyboardState.IsKeyUp(key))
                    {
                        // Key was just pressed
                        OnKeyPressed?.Invoke(key);
                        
                        // Example of using switch with when
                        switch (key)
                        {
                            case Keys.Escape when (CurrentState & InputState.MouseMoved) != 0:
                                // Special case for Escape when mouse was also moved
                                Console.WriteLine("Escape pressed with mouse movement");
                                break;
                            case Keys.Space when gameTime.TotalGameTime.TotalSeconds > 10:
                                // Only after 10 seconds of gameplay
                                Console.WriteLine("Space pressed after 10 seconds");
                                break;
                            default:
                                Console.WriteLine($"Key pressed: {key}");
                                break;
                        }
                    }
                }
            }
            
            // Reset state
            CurrentState = InputState.None;
            
            // Check for mouse movement
            if (_currentMouseState.X != _previousMouseState.X || 
                _currentMouseState.Y != _previousMouseState.Y)
            {
                CurrentState |= InputState.MouseMoved;
            }
            
            // Check for left mouse button click
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                CurrentState |= InputState.LeftMouseDown;
            }
            
            // Check for left mouse button released (click completed)
            if (_previousMouseState.LeftButton == ButtonState.Pressed && 
                _currentMouseState.LeftButton == ButtonState.Released)
            {
                OnClick?.Invoke(new Point(_currentMouseState.X, _currentMouseState.Y));
            }
        }
        
        // Using the is operator and params
        public bool IsAnyKeyPressed(params Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (_currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        // Using range type
        public Keys[] GetPressedKeysInRange(Range range)
        {
            var allKeys = _currentKeyboardState.GetPressedKeys();
            
            // Check if the range is valid for the array
            if (range.Start.Value >= 0 && range.End.Value <= allKeys.Length)
            {
                var result = new Keys[range.End.Value - range.Start.Value];
                Array.Copy(allKeys, range.Start.Value, result, 0, result.Length);
                return result;
            }
            
            return Array.Empty<Keys>();
        }
    }
}