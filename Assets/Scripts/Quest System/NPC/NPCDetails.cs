using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class NPCDetails : MonoBehaviour
{
    [Header("Script MUST be disabled after setting values!")]
    public NPCList npcDetails=new NPCList();
    public SO_NPCQuestList npcQuestList = null;

    public NPCDetails()
    {
        this.npcDetails.NPCid = Guid.NewGuid().ToString();
        this.npcDetails.startingType = QuestGiver.none;
    }

#if UNITY_EDITOR
    private SO_NPCList npcs = null;
    private bool npcAdded;

    private void Awake()
    {
        if (!Application.IsPlaying(gameObject))
        {
            if (npcs == null)
            {
                npcs = GameResources.Instance.npcList;
            }
        }
    }

    private void OnDisable()
    {
        if (!Application.IsPlaying(gameObject))
        {

            if (npcs != null)
            {
                AddToNPCList();
                // this is required to ensure the update gridproperties game object gets saved.
                EditorUtility.SetDirty(npcs);
            }
        }

    }

    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("Disable Script component");
        }
    }

    public void AddToNPCList()
    {
        if (this.name == "")
        {
            Debug.Log("Please re-enable and set a name to the NPC then disable script.");
            return;
        }

        if(!npcs.list.Exists(x => x.NPCid == npcDetails.NPCid)) 
        {
            NPCList npcToAdd = new NPCList();
            npcToAdd.NPCid = npcDetails.NPCid;
            npcToAdd.NPCName = npcDetails.NPCName;
            npcToAdd.startingType=npcDetails.startingType;
            npcs.list.Add(npcToAdd);

        }

    }


#endif
}
