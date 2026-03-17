using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerMovement player);
    bool CanInteract(PlayerMovement player);
    string GetInteractionPrompt();
}
