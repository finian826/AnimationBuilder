using UnityEngine;

public class MovementAnimationParameterControl : MonoBehaviour
{

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        AnimationEventHandler.MovementEvent += SetAnimationParameters;
    }

    private void OnDisable()
    {
        AnimationEventHandler.MovementEvent -= SetAnimationParameters;
    }

    private void SetAnimationParameters(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle,
        bool isIdle2, int direction, int action, bool isHorseWalking, bool isHorseTrotting, bool isHorseGalloping,
        bool horseIdle)
    {
        animator.SetFloat(AnimationSettings.xInput, xInput);
        animator.SetFloat(AnimationSettings.yInput, yInput);
        animator.SetBool(AnimationSettings.isWalking, isWalking);
        animator.SetBool(AnimationSettings.isRunning, isRunning);

        if (isIdle)
            animator.SetTrigger(AnimationSettings.isIdle);
        if (isIdle2)
            animator.SetTrigger(AnimationSettings.isIdle2);
        animator.SetInteger(AnimationSettings.direction, direction);
        animator.SetInteger(AnimationSettings.action, action);

        animator.SetBool(AnimationSettings.isHorseWalking, isHorseWalking);
        animator.SetBool(AnimationSettings.isHorseTrotting, isHorseTrotting);
        animator.SetBool(AnimationSettings.isHorseGalloping, isHorseGalloping);
        if (horseIdle)
            animator.SetTrigger(AnimationSettings.horseIdle);
        
    }


}
