using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "so_ObjectiveDialogEnd", menuName = "Scriptable Objects/Quests/Dialog/Objective Dialog End")]
public class SO_ObjectiveDialogEnd : ScriptableObject
{
    public string dialogEndStepID;
    public string questID;//id of quest this node belongs to
    public List<string> parentQuestStepID = new List<string>();
    [HideInInspector] public SO_Quests questNode;



#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    public bool isSelected = false;
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
        EditorGUILayout.LabelField("Objective Task");
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
