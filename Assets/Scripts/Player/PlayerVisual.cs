using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;

    private const string IS_RUNNING = "IsRunning";
    private const string IS_UP = "Up";
    private const string IS_DOWN = "Down";
    private const string IS_LEFT = "Left";
    private const string IS_RIGHT = "Right";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetBool(IS_RUNNING, PlayerMovement.Instance.IsRunning());
        animator.SetBool(IS_UP, PlayerMovement.Instance.IsUp());
        animator.SetBool(IS_DOWN, PlayerMovement.Instance.IsDown());
        animator.SetBool(IS_LEFT, PlayerMovement.Instance.IsLeft());
        animator.SetBool(IS_RIGHT, PlayerMovement.Instance.IsRight());
    }
}