using UnityEngine;

public class NPCMovementAnimationControl : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        EventHandler.NPCMovementEvent += SetNPCAnimationParameters;
    }

    private void OnDisable()
    {
        EventHandler.NPCMovementEvent -= SetNPCAnimationParameters;
    }

    private void SetNPCAnimationParameters(float xInput, float yInput, int direction, bool isWalking, bool isRunning, bool isIdle,
        bool eventAnimation)
    {
        animator.SetFloat(Settings.xInput, xInput);
        animator.SetFloat(Settings.yInput, yInput);
        animator.SetBool(Settings.isWalking, isWalking);
        animator.SetBool(Settings.isRunning, isRunning);
        animator.SetInteger(Settings.direction, direction);
        
        if (isIdle)
            animator.SetTrigger(Settings.isIdle);

        animator.SetBool(Settings.eventAnimation, eventAnimation);
    }
}
