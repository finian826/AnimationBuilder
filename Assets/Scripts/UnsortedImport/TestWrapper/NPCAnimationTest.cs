using UnityEngine;

public class NPCAnimationTest : MonoBehaviour
{
    // NPC Animation Parameters
    [Header("Common Attributes")]
    public float xInput;
    public float yInput;
    public int direction = 3;
    public bool isWalking = false;
    public bool isRunning = false;
    public bool isIdle = true;
    public bool isEating = false;
    public bool isSleeping = false;
    public bool eventAnimation;

    //npc Identification
    private string controllerID;
    private AnimationControllerID animationControl = null;

    private void Awake()
    {
        animationControl = GetComponentInChildren<AnimationControllerID>();
        controllerID = animationControl.AnimationGUID;
    }
    private void Update()
    {
        AnimationEventHandler.CallNPCMovementEvent(xInput, yInput, direction, isWalking, isRunning, isIdle, isEating,isSleeping, eventAnimation, controllerID);
    }
}
