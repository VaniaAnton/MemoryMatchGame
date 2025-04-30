using System;
using MemoryMatch.Core.Interfaces;

namespace MemoryMatch.Core.Models
{
    public abstract class GameEntity : IGameElement
    {
        public string Id { get; }
        public bool IsActive { get; protected set; }
        
        protected GameEntity(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            IsActive = true;
        }
        
        public abstract void Update(float deltaTime);
        
        public virtual void Reset()
        {
            IsActive = true;
        }
        
        // Method with default parameters
        public virtual void SetState(bool isActive = true, bool resetIfActive = false)
        {
            if (isActive && resetIfActive)
            {
                Reset();
            }
            IsActive = isActive;
        }
    }
}