using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public static class EventHandler
{
    
    //Movement Event

    public static event Action<float,float,bool,bool,bool,bool,int,int,bool,bool,bool,bool> MovementEvent;

    //Movement Event Call for Publishers

    public static void CallMovementEvent(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle,
        bool isIdle2, int direction, int action, bool isHorseWalking, bool isHorseTrotting, bool isHorseGalloping,
        bool horseIdle)
    {
        if (MovementEvent != null)
            MovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isIdle2, direction, action,
                isHorseWalking, isHorseTrotting, isHorseGalloping, horseIdle);
    }

    // NPC Movement Event
    public static Action<float, float, int, bool, bool, bool,bool> NPCMovementEvent;

    //NPC Movement Event Call for Publishers
    public static void CallNPCMovementEvent(float xInput, float yInput, int direction, bool isWalking,
        bool isRunning, bool isIdle, bool eventAnimation)
    {
        if (NPCMovementEvent != null)
            NPCMovementEvent(xInput, yInput, direction, isWalking, isRunning, isIdle, eventAnimation);
    }

    //Scene load events - in order that they happen

    //before scene unload fade out
    public static event Action BeforeSceneUnloadFadeOutEvent;

    public static void CallBeforeSceneUnloadFadeOutEvent()
    {
        if (BeforeSceneUnloadFadeOutEvent != null)
        {
            BeforeSceneUnloadFadeOutEvent();
        }
    }

    //before scene unload event
    public static event Action BeforeSceneUnloadEvent;

    public static void CallBeforeSceneUnloadEvent()
    {
        if (BeforeSceneUnloadEvent != null)
        {
            BeforeSceneUnloadEvent();
        }
    }

    // after scene loaded event
    public static event Action AfterSceneLoadEvent;

    public static void CallAfterSceneLoadEvent()
    {
        if (AfterSceneLoadEvent != null)
        {
            AfterSceneLoadEvent();
        }
    }

    //after scene load fade in event
    public static event Action AfterSceneLoadFadeInEvent;

    public static void CallAfterSceneLoadFadeInEvent()
    {
        if (AfterSceneLoadFadeInEvent != null)
        {
            AfterSceneLoadFadeInEvent();
        }
    }

    public static event Action DropSelectedItemEvent;

    public static void CallDropSelectedItemEvent()
    {
        if (DropSelectedItemEvent != null)
        {
            DropSelectedItemEvent();
        }
    }

    public static event Action RemoveSelectedItemFromInventoryEvent;

    public static void CallRemoveSelectedItemFromInventoryEvent()
    {
        if (RemoveSelectedItemFromInventoryEvent != null)
        {
            RemoveSelectedItemFromInventoryEvent();
        }
    }

    public static event Action InstantiateCropPrefabsEvent;

    public static void CallInstantiateCropPrefabsEvent()
    {
        if (InstantiateCropPrefabsEvent != null)
        {
            InstantiateCropPrefabsEvent();
        }
    }

}