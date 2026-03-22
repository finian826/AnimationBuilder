using System;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AnimationControllerID : MonoBehaviour
{
    private string animationGUID;

    public string AnimationGUID { get { return animationGUID; } }

    public AnimationControllerID()
    {
        this.animationGUID= Guid.NewGuid().ToString();
    }

}
