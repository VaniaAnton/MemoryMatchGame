namespace MemoryMatch.Core.Interfaces
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        void Interact();
        event System.Action<string> OnInteraction;
    }
}