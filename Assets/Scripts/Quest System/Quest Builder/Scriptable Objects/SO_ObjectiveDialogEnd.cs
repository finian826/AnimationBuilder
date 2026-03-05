using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "so_ObjectiveDialogEnd", menuName = "Scriptable Objects/Quest System/Dialog/Dialog End")]
public class SO_ObjectiveDialogEnd : ScriptableObject
{
    public string dialogEndStepID;
    public string questID;//id of quest this node belongs to
    public List<string> parentQuestStepID = new List<string>();
    public List<string> childQuestStepID = new List<string>();
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
        this.dialogEndStepID = Guid.NewGuid().ToString();
        this.name = "DialogEnd";
        this.questNode = nodeGraph;
        this.questID = nodeGraph.questNodeID;
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
    /// drag node
    /// </summary>
    /// <param name="delta"></param>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        questNode.BuildNodeLocationDictionary();
        EditorUtility.SetDirty(this);
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
    /// add childid to the node returns true if node has been added, false otherwise
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddChildStepToQuestStep(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            childQuestStepID.Add(childID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    private bool IsChildRoomValid(string childID)
    {
        //TODO: Have to comeup with some rules
        bool testValid = false;
        if (childID != dialogEndStepID)
            testValid = true;
        if (questNode.GetStepNodeType(childID) == CurrentWorkingNode.QuestStart)
            testValid = false;

        return testValid;
    }

    /// <summary>
    /// add parentID to the node returns true if node has been added, false otherwise
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool AddQuestStepIDToParent(string parentID)
    {
        if (parentID != dialogEndStepID)
        {
            parentQuestStepID.Add(parentID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    public bool RemoveParent(string parentID)
    {
        //if the node contains the parentID remove it
        if (parentQuestStepID.Contains(parentID))
        {
            parentQuestStepID.Remove(parentID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    public bool RemoveChild(string childID)
    {
        //if the node contains the child id, remove it
        if (childQuestStepID.Contains(childID))
        {
            childQuestStepID.Remove(childID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        questNode.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    public void Draw(GUIStyle nodeStyle)
    {
        //draw node box using begin area
        GUILayout.BeginArea(rect, nodeStyle);
        //start region to detect popup selection changes
        EditorGUI.BeginChangeCheck();
        //display a label that can't be changed
        EditorGUILayout.LabelField("End Dialog");
        if (GUILayout.Button("Edit Details"))
        {
            CallEditDetails();
        }
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();

    }


    private void IsNodeConnected()
    {
        if (parentQuestStepID.Count > 0)
        {
            isConnected = true;
        }
        if (parentQuestStepID.Count == 0)
        {
            isConnected = false;
        }
    }


    private void CallEditDetails()
    {
        callEditor = true;
    }

#endif
}
