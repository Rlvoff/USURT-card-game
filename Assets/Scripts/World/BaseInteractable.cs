using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    protected string interactionPrompt = "Нажмите E для взаимодействия";

    public virtual bool CanInteract(PlayerMovement player)
    {
        return true; // По умолчанию можно взаимодействовать
    }

    public virtual string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public abstract void Interact(PlayerMovement player);
}