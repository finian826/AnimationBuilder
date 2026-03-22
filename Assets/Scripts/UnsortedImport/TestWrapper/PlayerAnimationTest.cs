using UnityEngine;

public class PlayerAnimationTest : MonoBehaviour
{

    // Player Animation Parameters
    public float xInput;
    public float yInput;
    public bool isWalking;
    public bool isRunning;
    public bool isIdle = true;
    public bool isIdle2;
    public int direction = 3;
    public int action = 0;
    public bool isHorseWalking;
    public bool isHorseTrotting;
    public bool isHorseGalloping;
    public bool horseIdle;


    private void Update()
    {
        AnimationEventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isIdle2,
            direction, action, isHorseWalking, isHorseTrotting, isHorseGalloping, horseIdle);
    }

}
