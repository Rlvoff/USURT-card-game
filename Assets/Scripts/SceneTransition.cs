using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : BaseInteractable
{
    [Header("Настройки перехода")]
    [SerializeField] private int sceneToLoadIndex;
    [SerializeField] private string sceneToLoadName;
    [SerializeField] private Vector3 newPlayerPosition;

    [Header("Визуальные эффекты")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject frame;
    [SerializeField] private GameObject[] otherFrames;

    private bool isActivated = false;

    public override void Interact(PlayerMovement player)
    {
        InteractionPrompt.Instance?.HidePrompt();

        // Сохраняем позицию игрока в SceneStateManager перед переходом
        SceneStateManager.Instance?.SaveCurrentPlayerPosition();

        // Запускаем анимацию перехода
        if (anim != null && !isActivated)
        {
            anim.SetTrigger("isTriggered");
            isActivated = true;
        }

        // Переход на другую сцену
        if (!string.IsNullOrEmpty(sceneToLoadName))
        {
            SceneManager.LoadScene(sceneToLoadName);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoadIndex);
        }
    }

    public override string GetInteractionPrompt()
    {
        if (!string.IsNullOrEmpty(sceneToLoadName))
        {
            return $"Перейти в {sceneToLoadName} [E]";
        }
        else
        {
            return $"Перейти [E]";
        }

    }

}