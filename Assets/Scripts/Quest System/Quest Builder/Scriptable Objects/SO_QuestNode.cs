using UnityEngine;
using System.Collections.Generic;
using System;


[CreateAssetMenu(fileName = "so_QuestNode", menuName = "Scriptable Objects/Quest System/Quest Node Graph")]
public class SO_QuestNode : ScriptableObject
{
    [HideInInspector] public List<SO_Quests> questList = new List<SO_Quests>();
    [HideInInspector] public Dictionary<string, SO_Quests> questNodeDictionary = new Dictionary<string, SO_Quests>();

    /// <summary>
    /// Get room node by room nodeID
    /// </summary>
    /// <param name="questNodeID"></param>
    /// <returns></returns>
    public SO_Quests GetRoomNode(string questNodeID)
    {
        if (questNodeDictionary.TryGetValue(questNodeID, out SO_Quests questNode))
        {
            return questNode;
        }
        return null;
    }

    private void Awake()
    {
        LoadQuestNodeDictionary();
    }

    private void LoadQuestNodeDictionary()
    {
        questNodeDictionary.Clear();

        //populate the dictionary
        foreach (SO_Quests node in questList)
        {
            questNodeDictionary[node.questNodeID] = node;
        }
    }

    /// <summary>
    /// get room node by roomNodeType
    /// </summary>
    /// <param name="questType"></param>
    /// <returns></returns>
    public SO_Quests GetRoomNode(QuestNodeType questType)
    {
        foreach (SO_Quests node in questList)
        {
            if (node.typeOfQuest == questType)
            {
                return node;
            }
        }
        return null;
    }

    public IEnumerable<SO_Quests> GetChildrenNodes(SO_Quests parentRoomNode)
    {
        foreach (string childrenID in parentRoomNode.requiredFor)
        {
            yield return GetRoomNode(childrenID);
        }
    }

#if UNITY_EDITOR

    [HideInInspector] public SO_Quests roomNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 linePosition;

    /// <summary>
    /// repopulate the node dictionary after a change is made
    /// </summary>
    public void OnValidate()
    {
        LoadQuestNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(SO_Quests node, Vector2 position)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }


#endif

}
