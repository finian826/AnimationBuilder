using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NPCGameManager : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private SO_NPCList masterNPCList = null;


    private void OnEnable()
    {
        //only populate in the editor
        if (!Application.IsPlaying(gameObject))
        {
            if (masterNPCList != null)
            {
                masterNPCList.list.Clear();
            }
        }
    }

    private void OnDisable()
    {
        if (!Application.IsPlaying(gameObject))
        {
            UpdateQuestStartList();

            if (masterNPCList != null)
            {
                // this is required to ensure the update gridproperties game object gets saved.
                EditorUtility.SetDirty(masterNPCList);
            }
        }
    }

    private void Update()
    {
        // only populate in editor
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("DISABLE NPC Game Manager");
        }
    }

    private void UpdateQuestStartList()
    {

    }

#endif
}
