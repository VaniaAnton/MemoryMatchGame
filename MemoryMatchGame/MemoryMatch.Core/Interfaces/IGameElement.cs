namespace MemoryMatch.Core.Interfaces
{
    public interface IGameElement
    {
        string Id { get; }
        bool IsActive { get; }
        void Update(float deltaTime);
        void Reset();
    }
}