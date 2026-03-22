using UnityEngine;

[RequireComponent(typeof(AnimationControllerID))]
public class NPCMovementAnimationControl : MonoBehaviour
{
    private Animator animator;
    private string animationGUID;
    private AnimationControllerID animationControl = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animationControl = GetComponent<AnimationControllerID>();
        animationGUID = animationControl.AnimationGUID;
    }
    private void OnEnable()
    {
        AnimationEventHandler.NPCMovementEvent += SetNPCAnimationParameters;
    }

    private void OnDisable()
    {
        AnimationEventHandler.NPCMovementEvent -= SetNPCAnimationParameters;
    }

    private void SetNPCAnimationParameters(float xInput, float yInput, int direction, bool isWalking, bool isRunning, bool isIdle,
        bool isEating, bool isSleeping,bool eventAnimation,string controlID)
    {
        if (controlID==animationGUID)
        {
            animator.SetFloat(AnimationSettings.xInput, xInput);
            animator.SetFloat(AnimationSettings.yInput, yInput);
            animator.SetBool(AnimationSettings.isWalking, isWalking);
            animator.SetBool(AnimationSettings.isRunning, isRunning);
            animator.SetInteger(AnimationSettings.direction, direction);
            animator.SetBool(AnimationSettings.isEating, isEating);
            animator.SetBool(AnimationSettings.isSleeping, isSleeping);

            if (isIdle == true)
            {
                animator.SetTrigger(AnimationSettings.isIdle);
            }

            animator.SetBool(AnimationSettings.eventAnimation, eventAnimation);
        }
    }
}
