using UnityEngine;

public static class Settings
{

    // Player Animation Parameters
    public static int xInput;
    public static int yInput;
    public static int isWalking;
    public static int isRunning;
    public static int isIdle;
    public static int isIdle2;
    public static int direction;
    public static int action;
    public static int isHorseWalking;
    public static int isHorseTrotting;
    public static int isHorseGalloping;
    public static int horseIdle;


    // static Constructor
    static Settings()
    {
        //Player Animation Parameters
        xInput = Animator.StringToHash("xInput");
        yInput = Animator.StringToHash("yInput");
        isWalking = Animator.StringToHash("isWalking");
        isRunning = Animator.StringToHash("isRunning");
        isIdle = Animator.StringToHash("idle");
        isIdle2 = Animator.StringToHash("idle2");
        direction = Animator.StringToHash("direction");
        action = Animator.StringToHash("action");
        isHorseWalking = Animator.StringToHash("isHorseWaling");
        isHorseGalloping = Animator.StringToHash("isHorseGalloping");
        isHorseTrotting = Animator.StringToHash("isHorseTrotting");
        horseIdle = Animator.StringToHash("horseIdle");
    }
}