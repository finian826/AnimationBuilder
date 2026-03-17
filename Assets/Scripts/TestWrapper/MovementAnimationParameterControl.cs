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
        EventHandler.MovementEvent += SetAnimationParameters;
    }

    private void OnDisable()
    {
        EventHandler.MovementEvent -= SetAnimationParameters;
    }

    private void SetAnimationParameters(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle,
        bool isIdle2, int direction, int action, bool isHorseWalking, bool isHorseTrotting, bool isHorseGalloping,
        bool horseIdle)
    {
        animator.SetFloat(Settings.xInput, xInput);
        animator.SetFloat(Settings.yInput, yInput);
        animator.SetBool(Settings.isWalking, isWalking);
        animator.SetBool(Settings.isRunning, isRunning);

        if (isIdle)
            animator.SetTrigger(Settings.isIdle);
        if (isIdle2)
            animator.SetTrigger(Settings.isIdle2);
        animator.SetInteger(Settings.direction, direction);
        animator.SetInteger(Settings.action, action);

        animator.SetBool(Settings.isHorseWalking, isHorseWalking);
        animator.SetBool(Settings.isHorseTrotting, isHorseTrotting);
        animator.SetBool(Settings.isHorseGalloping, isHorseGalloping);
        if (horseIdle)
            animator.SetTrigger(Settings.horseIdle);
        
    }


}
