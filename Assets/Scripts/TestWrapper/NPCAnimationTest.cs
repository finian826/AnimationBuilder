using UnityEngine;

public class NPCAnimationTest : MonoBehaviour
{
    // Player Animation Parameters
    public float xInput;
    public float yInput;
    public int direction;
    public bool isWalking;
    public bool isRunning;
    public bool isIdle;
    public bool eventAnimation;


    private void Update()
    {
        EventHandler.CallNPCMovementEvent(xInput, yInput, direction, isWalking, isRunning, isIdle, eventAnimation);
    }
}
