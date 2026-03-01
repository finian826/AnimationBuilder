using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if(instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }
    [Space(10)]
    //[Header("Dungeon")]
    //[Tooltip("Populate with the dungeon so_RoomNodeTypeList")]
    //public SO_RoomNodeTypeList roomNodeTypeList;
    [Space(10)]
    [Header("Materials")]
    [Tooltip("Dimmed Material")]
    public Material dimmedMaterial;
    [Space(10)]
    [Header("Quest Related Items")]
    [Tooltip("Populate with the NPC List scriptable object")]
    public SO_NPCList npcList;
}
