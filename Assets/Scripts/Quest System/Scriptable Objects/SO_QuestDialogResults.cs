using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "so_QuestDialogResults", menuName = "Scriptable Objects/Quest System/Common Nodes/Quest Dialog Results")]
public class SO_QuestDialogResults : ScriptableObject
{
    public string questDialogResultsStepID;
    public string questID;//id of quest this node belongs to
    public List<string> parentQuestStepIDList = new List<string>();
    public QuestNodeType rewardType = QuestNodeType.none;
    [Header("Quest Rewards")]
    public bool guarenteedQuestReward;
    public List<QuestItems> guarentedQuestRewardItemsList;
    public bool choiceQuestReward;
    public List<QuestItems> choiceQuestRewardItemsList;
    public string questFinishedText;
    [Header("Rep Rewards (Not implimented yet)")]
    public QuestNPCRepReward[] npcRepReward;
    [Header("Money Rewards (Not Implimented Yet)")]
    public int fundsReward;
    [Header("Dialog Rewards")]
    public bool guarenteedDialogReward;
    public List<QuestItems> guarentedDialogRewardItemsList;
    public bool choiceDialogReward;
    public List<QuestItems> choiceDialogRewardItemsList;
    public string dialogFinishedText;
    public bool activateAllChildren = false;
    public List<string> onlyActivateTheseChildrenList;
    [Header("Rep Rewards (Not implimented yet)")]
    public QuestFactionRepReward[] factionRepReward;
    [HideInInspector] public SO_Quests questNode;


#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public bool callEditor = false;


    public void Initialise(Rect rect, SO_Quests nodeGraph)
    {
        this.rect = rect;
        this.questDialogResultsStepID = Guid.NewGuid().ToString();
        this.name = "QuestDialogResults";
        this.questNode = nodeGraph;
        this.questID = nodeGraph.questNodeID;
    }

    public void Initialise(Rect rect, SO_Quests nodeGraph,QuestNodeType rewardType)
    {
        this.rect = rect;
        this.questDialogResultsStepID = Guid.NewGuid().ToString();
        this.name = "QuestDialogResults";
        this.questNode = nodeGraph;
        this.questID = nodeGraph.questNodeID;
        this.rewardType = rewardType;
        this.guarentedQuestRewardItemsList = new List<QuestItems>();
        this.guarentedDialogRewardItemsList = new List<QuestItems>();
        this.choiceDialogRewardItemsList = new List<QuestItems>();
        this.choiceQuestRewardItemsList = new List<QuestItems>();
    }


    private void IsNodeConnected()
    {
        if (parentQuestStepIDList.Count > 0)
        {
            isConnected = true;
        }
        if (parentQuestStepIDList.Count == 0)
        {
            isConnected = false;
        }
    }


    private void CallEditDetails()
    {
        callEditor = true;
    }

    /// <summary>
    /// drag node
    /// </summary>
    /// <param name="delta"></param>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        questNode.BuildNodeLocationDictionary();
        EditorUtility.SetDirty(this);
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            //process mouse down events
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            //process mouse up event
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            //process mouse drag event
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    public void Draw(GUIStyle nodeStyle)
    {
        //draw node box using begin area
        GUILayout.BeginArea(rect, nodeStyle);
        //start region to detect popup selection changes
        EditorGUI.BeginChangeCheck();
        //display a label that can't be changed
        EditorGUILayout.LabelField("Results Node");
        if (GUILayout.Button("Edit Details"))
        {
            CallEditDetails();
        }
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();

    }

    /// <summary>
    /// Process mouse down events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //left click down
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        questNode.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }


    /// <summary>
    /// process mouse up event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        //if left click up
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// process mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        //process left click drag event
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }


    /// <summary>
    /// process left click events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        //toggle node selection
        if (isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
    }


    /// <summary>
    /// process left click up event
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
            questNode.BuildNodeLocationDictionary();
        }
    }

    /// <summary>
    /// process left mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// add parentID to the node returns true if node has been added, false otherwise
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool AddQuestStepIDToParent(string parentID)
    {
        if (parentID != questDialogResultsStepID)
        {
            parentQuestStepIDList.Add(parentID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    public bool RemoveParent(string parentID)
    {
        //if the node contains the parentID remove it
        if (parentQuestStepIDList.Contains(parentID))
        {
            parentQuestStepIDList.Remove(parentID);
            IsNodeConnected();
            return true;
        }
        return false;
    }


#endif
}
