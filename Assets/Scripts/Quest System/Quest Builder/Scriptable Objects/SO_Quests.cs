using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


[CreateAssetMenu(fileName = "so_Quests", menuName = "Scriptable Objects/Quests/Quest")]
public class SO_Quests : ScriptableObject
{
    public string questNodeID;
    public QuestType typeOfQuest = QuestType.Quest;
    public string questStartStepID;
    public List<string> prerequisateQuests=new List<string>();
    public List<string> requiredFor=new List<string>();
    public QuestGiver questStartCondition = QuestGiver.NPC;
    public string questStarter;
    public QuestStatus status = QuestStatus.Locked;
    [HideInInspector] public SO_QuestNode questNode;
     public SO_QuestStartDetails questStartDetails = null;
     public SO_QuestEndDetails questEndDetails = null;
     public List<SO_ObjectiveCollect> questCollectDetailsList = new List<SO_ObjectiveCollect>();
     public List<SO_ObjectiveCourier> questCouierDetailsList = new List<SO_ObjectiveCourier>();
     public List<SO_ObjectiveTask> questTaskDetailsList = new List<SO_ObjectiveTask>();
    public SO_ObjectiveDialogStart dialogStartDetails = null;
    public SO_ObjectiveDialogEnd dialogEndDetails = null;
    public List<SO_ObjectiveDialogBasic> dialogBasicDetailsList = new List<SO_ObjectiveDialogBasic>();
    public List<SO_ObjectiveDialogBranch> dialogBranchDetailsList = new List<SO_ObjectiveDialogBranch>();
    public SO_QuestDialogResults resultsDetails = null;
    [HideInInspector] public Dictionary<string, CurrentWorkingNode> questStepDictionary = new Dictionary<string, CurrentWorkingNode>();

    private void Awake()
    {
        BuildStepDictionary();
    }

    private void BuildStepDictionary()
    {
        questStepDictionary.Clear();
        //Add quest nodes
        if(questStartDetails != null)
        {
            questStepDictionary[questStartDetails.questStartID] = CurrentWorkingNode.QuestStart;
        }
        if (questEndDetails != null)
        {
            questStepDictionary[questEndDetails.questEndID] = CurrentWorkingNode.QuestEnd;
        }
        if(questCollectDetailsList.Count > 0)
        {
            foreach(SO_ObjectiveCollect nodeID in questCollectDetailsList)
            {
                questStepDictionary[nodeID.collectQuestStepID] = CurrentWorkingNode.QuestCollect;
            }
        }
        if (questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier nodeID in questCouierDetailsList)
            {
                questStepDictionary[nodeID.courierQuestStepID] = CurrentWorkingNode.QuestCourier;
            }
        }
        if (questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask nodeID in questTaskDetailsList)
            {
                questStepDictionary[nodeID.taskQuestStepID] = CurrentWorkingNode.QuestTask;
            }
        }
        //Add dialog nodes
        if (dialogStartDetails != null)
        {
            questStepDictionary[dialogStartDetails.dialogStartStepID] = CurrentWorkingNode.DialogStart;
        }
        if (dialogEndDetails != null)
        {
            questStepDictionary[dialogEndDetails.dialogEndStepID] = CurrentWorkingNode.DialogEnd;
        }
        if (dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic nodeID in dialogBasicDetailsList)
            {
                questStepDictionary[nodeID.dialogBasicStepID] = CurrentWorkingNode.DialogBasic;
            }
        }
        if (dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch nodeID in dialogBranchDetailsList)
            {
                questStepDictionary[nodeID.dialogBranchStepID] = CurrentWorkingNode.DialogBranch;
            }
        }
        //add common nodes
        if (resultsDetails != null)
        {
            questStepDictionary[resultsDetails.questDialogResultsStepID] = CurrentWorkingNode.QuestDialogResults;
        }
#if UNITY_EDITOR
        BuildNodeLocationDictionary();
#endif
    }

    public CurrentWorkingNode GetStepNodeType(string nodeID)
    {
        CurrentWorkingNode nodeType;
        if(questStepDictionary.TryGetValue(nodeID, out nodeType))
        {
            return nodeType;
        }
        else
        {
            return CurrentWorkingNode.none;
        }
    }

    //TODO: Add extra methods for added node types
    public SO_QuestDialogResults GetResultsNodeByID(string nodeID)
    {
        if (nodeID == resultsDetails.questDialogResultsStepID)
        {
            return resultsDetails;
        }
        return null;
    }

    public SO_QuestStartDetails GetStartNodeByID(string nodeID)
    {
        if (nodeID == questStartDetails.questStartID)
        {
            return questStartDetails;
        }
        return null;
    }

    public SO_QuestEndDetails GetEndNodeByID(string nodeID)
    {
        if (nodeID == questEndDetails.questEndID)
        {
            return questEndDetails;
        }
        return null;
    }

    public SO_ObjectiveTask GetTaskNodeByID(string nodeID)
    {
        if (questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask item in questTaskDetailsList)
            {
                if (nodeID == item.taskQuestStepID)
                {
                    return item;
                }
            }
        }
        return null;
    }

    public SO_ObjectiveCollect GetCollectNodeByID(string nodeID)
    {
        if (questCollectDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCollect item in questCollectDetailsList)
            {
                if (nodeID == item.collectQuestStepID)
                {
                    return item;
                }
            }
        }
        return null;
    }

    public SO_ObjectiveCourier GetCourierNodeByID(string nodeID)
    {
        if (questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier item in questCouierDetailsList)
            {
                if (nodeID == item.courierQuestStepID)
                {
                    return item;
                }
            }
        }
        return null;
    }


#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public bool callEditor = false;
    [HideInInspector] public SO_QuestStartDetails startNodeToDrawLineFrom = null;
    [HideInInspector] public SO_QuestEndDetails endNodeToDrawLineFrom = null;
    [HideInInspector] public SO_ObjectiveCollect collectNodeToDrawLineFrom = null;
    [HideInInspector] public SO_ObjectiveCourier couierNodeToDrawLineFrom = null;
    [HideInInspector] public SO_ObjectiveTask taskNodeToDrawLineFrom = null;
    [HideInInspector] public SO_ObjectiveDialogBranch branchNodeToDrawLineFrom = null;
    [HideInInspector] public SO_ObjectiveDialogEnd dialogEndNodeToDrawLineFrom = null;
    [HideInInspector] public SO_ObjectiveDialogStart dialogStartNodeToDrawLineFrom = null;
    [HideInInspector] public SO_ObjectiveDialogBasic dialogBasicNodeToDrawLineFrom = null;
    [HideInInspector] public SO_QuestDialogResults resultsNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 linePosition;
    [HideInInspector] public CurrentWorkingNode nodeTypeLineFrom;
    public Dictionary<string,Rect> nodeLocations=new Dictionary<string,Rect>();
    public void Initialise(Rect rect, SO_QuestNode nodeGraph, QuestType questType)
    {
        this.rect = rect;
        this.questNodeID = Guid.NewGuid().ToString();
        this.name = "Quest";
        this.questNode = nodeGraph;
        this.typeOfQuest = questType;
        //load room node type list
        //roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public string GetNodeIDFromLocationDictionary(Vector2 mousePosition)
    {
        foreach(var kvp in nodeLocations)
        {
            if (kvp.Value.Contains(mousePosition))
            {
                return kvp.Key;
            }
           
        }
        return null;
    }

    public Rect GetRectFromLocationDirectory(string nodeID)
    {
        foreach(var kvp in nodeLocations)
        {
            if(kvp.Key.Contains(nodeID))
            {
                return kvp.Value;
            }
        }
        return Rect.zero;
    }

    public void BuildNodeLocationDictionary()
    {
        nodeLocations.Clear();
        //Add quest node rect locations
        if (questStartDetails != null)
        {
            nodeLocations[questStartDetails.questStartID] = questStartDetails.rect;
        }
        if(questEndDetails != null)
        {
            nodeLocations[questEndDetails.questEndID] = questEndDetails.rect;
        }
        if(questTaskDetailsList.Count > 0)
        {
            foreach(SO_ObjectiveTask task in questTaskDetailsList)
            {
                nodeLocations[task.taskQuestStepID]=task.rect;
            }
        }
        if(questCollectDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCollect collect in questCollectDetailsList)
            {
                nodeLocations[collect.collectQuestStepID] = collect.rect;
            }
        }
        if(questCouierDetailsList.Count > 0)
        {
            foreach(SO_ObjectiveCourier couier in questCouierDetailsList)
            {
                nodeLocations[couier.courierQuestStepID] = couier.rect;
            }
        }
        //add dialog rect locations
        if (dialogStartDetails != null)
        {
            nodeLocations[dialogStartDetails.dialogStartStepID] = dialogStartDetails.rect;
        }
        if (dialogEndDetails != null)
        {
            nodeLocations[dialogEndDetails.dialogEndStepID] = dialogEndDetails.rect;
        }
        if (dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic dialogBasic in dialogBasicDetailsList)
            {
                nodeLocations[dialogBasic.dialogBasicStepID] = dialogBasic.rect;
            }
        }
        if (dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch dialogBranch in dialogBranchDetailsList)
            {
                nodeLocations[dialogBranch.dialogBranchStepID] = dialogBranch.rect;
            }
        }
        //add common nodes
        if (resultsDetails != null)
        {
            nodeLocations[resultsDetails.questDialogResultsStepID] = resultsDetails.rect;
        }
    }

    //TODO: Add removeNode methods for all added nodes
    public void RemoveNode(SO_QuestStartDetails node)
    {
        if (questStartDetails == node)
        {
            //remove node from assets database
            DestroyImmediate(node, true);

            //save the asset database
            AssetDatabase.SaveAssets();
            questStartDetails = null;           

            BuildStepDictionary();            
        }
    }

    public void RemoveNode(SO_QuestEndDetails node)
    {
        if (questEndDetails == node)
        {
            //remove node from assets database
            DestroyImmediate(node, true);

            //save the asset database
            AssetDatabase.SaveAssets();
            questEndDetails = null;

            BuildStepDictionary();
        }
    }

    public void RemoveNode(SO_ObjectiveCollect node)
    {
        if(questCollectDetailsList.Count > 0)
        {
            foreach(SO_ObjectiveCollect item in questCollectDetailsList)
            {
                if (item == node)
                {
                    questCollectDetailsList.Remove(node);
                    //remove node from assets database
                    DestroyImmediate(node, true);

                    //save the asset database
                    AssetDatabase.SaveAssets();

                    BuildStepDictionary();
                    break;
                }
            }
        }
    }

    public void RemoveNode(SO_ObjectiveCourier node)
    {
        if (questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier item in questCouierDetailsList)
            {
                if (item == node)
                {
                    questCouierDetailsList.Remove(node);
                    //remove node from assets database
                    DestroyImmediate(node, true);

                    //save the asset database
                    AssetDatabase.SaveAssets();

                    BuildStepDictionary();
                    break;
                }
            }
        }
    }

    public void RemoveNode(SO_ObjectiveTask node)
    {
        if (questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask item in questTaskDetailsList)
            {
                if (item == node)
                {
                    questTaskDetailsList.Remove(node);
                    //remove node from assets database
                    DestroyImmediate(node, true);

                    //save the asset database
                    AssetDatabase.SaveAssets();

                    BuildStepDictionary();
                    break;
                }
            }
        }
    }


    public void OnValidate()
    {
        BuildStepDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(SO_QuestDialogResults node, Vector2 position)
    {
        resultsNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestTask;
    }

    public void SetNodeToDrawConnectionLineFrom(SO_ObjectiveDialogBasic node, Vector2 position)
    {
        dialogBasicNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestTask;
    }

    public void SetNodeToDrawConnectionLineFrom(SO_ObjectiveDialogStart node, Vector2 position)
    {
        dialogStartNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestTask;
    }

    public void SetNodeToDrawConnectionLineFrom(SO_ObjectiveDialogEnd node, Vector2 position)
    {
        dialogEndNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestTask;
    }


    public void SetNodeToDrawConnectionLineFrom(SO_ObjectiveDialogBranch node, Vector2 position)
    {
        branchNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestTask;
    }


    public void SetNodeToDrawConnectionLineFrom(SO_ObjectiveTask node, Vector2 position)
    {
        taskNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestTask;
    }

    public void SetNodeToDrawConnectionLineFrom(SO_ObjectiveCourier node, Vector2 position)
    {
        couierNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestCourier;
    }

    public void SetNodeToDrawConnectionLineFrom(SO_ObjectiveCollect node, Vector2 position)
    {
        collectNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestCollect;
    }

    public void SetNodeToDrawConnectionLineFrom(SO_QuestStartDetails node, Vector2 position)
    {
        startNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestStart;
    }

    public void SetNodeToDrawConnectionLineFrom(SO_QuestEndDetails node, Vector2 position)
    {
        endNodeToDrawLineFrom = node;
        linePosition = position;
        nodeTypeLineFrom = CurrentWorkingNode.QuestEnd;
    }

    public void DeleteStepNodes()
    {
        if (questStartDetails != null)
        {
            //remove node from assets database
            DestroyImmediate(questStartDetails, true);

            //save the asset database
            AssetDatabase.SaveAssets();
        }
        if(questEndDetails != null)
        {
            //remove node from assets database
            DestroyImmediate(questEndDetails, true);

            //save the asset database
            AssetDatabase.SaveAssets();
        }
        if(questCollectDetailsList.Count > 0)
        {
            for(int i = 0; i < questCollectDetailsList.Count; i++)
            {
                //remove node from assets database
                DestroyImmediate(questCollectDetailsList[i], true);

                //save the asset database
                AssetDatabase.SaveAssets();
            }
        }
        if(questCouierDetailsList.Count > 0)
        {
            for (int i = 0; i < questCouierDetailsList.Count; i++)
            {
                //remove node from assets database
                DestroyImmediate(questCouierDetailsList[i], true);

                //save the asset database
                AssetDatabase.SaveAssets();
            }
        }
        if(questTaskDetailsList.Count > 0)
        {
            for (int i = 0; i < questTaskDetailsList.Count; i++)
            {
                //remove node from assets database
                DestroyImmediate(questTaskDetailsList[i], true);

                //save the asset database
                AssetDatabase.SaveAssets();
            }
        }
    }

    //TODO: Still to be expanded
    public void Draw(GUIStyle nodeStyle)
    {
        //draw node box using begin area
        GUILayout.BeginArea(rect, nodeStyle);
        //start region to detect popup selection changes
        EditorGUI.BeginChangeCheck();
        if (requiredFor.Count > 0 || prerequisateQuests.Count > 0)
        {
            //display a label that can't be changed
            EditorGUILayout.LabelField(typeOfQuest.ToString());
            if (GUILayout.Button("Edit Details"))
            {
                CallEditDetails();
            }
        }
        else
        {
            //display a popup using the RoomNodeType name values that can be selected from(default to the current selected roomNodeType)
            QuestType selected = typeOfQuest;
            QuestType selection = (QuestType)EditorGUILayout.EnumPopup("", selected);

            typeOfQuest = selection;
            //if the room type selection has changed making child connections possibly invalid
            if (prerequisateQuests.Count > 0)
            {
                for (int i = prerequisateQuests.Count - 1; i >= 0; i--)
                {
                    //get the child room node
                    SO_Quests childRoomNode = questNode.GetRoomNode(prerequisateQuests[i]);
                    //if the child room node is selected
                    if (childRoomNode != null)
                    {
                        //remove childid from parent room node
                        RemoveRequiredForFromQuestNode(childRoomNode.questNodeID);
                        //remove parentid from child room node
                        childRoomNode.RemoveQuestNodeIDFromPrerequisate(questNodeID);
                    }
                }
            }

        }
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();

    }

    private void CallEditDetails()
    {
        callEditor = true;
    }

    private void IsNodeConnected()
    {
        if(requiredFor.Count > 0 || prerequisateQuests.Count > 0)
        {
            isConnected = true;
        }
        if (requiredFor.Count == 0 && prerequisateQuests.Count == 0)
        {
            isConnected = false;
        }
    }

    /// <summary>
    /// remove childID from the node, returns true if node has been removed
    /// </summary>
    /// <param name="questID"></param>
    /// <returns></returns>
    public bool RemoveRequiredForFromQuestNode(string questID)
    {
        //if the node contains the child id, remove it
        if (requiredFor.Contains(questID))
        {
            requiredFor.Remove(questID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    /// <summary>
    /// add childid to the node returns true if node has been added, false otherwise
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddRequiredForToQuestNode(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            requiredFor.Add(childID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    /// <summary>
    /// add parentID to the node returns true if node has been added, false otherwise
    /// </summary>
    /// <param name="parentID"></param>
    /// <returns></returns>
    public bool AddQuestNodeIDToPrerequisate(string parentID)
    {
        if (parentID != questNodeID)
        {
            prerequisateQuests.Add(parentID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    /// <summary>
    /// remove parentID from the node, returns true if node has been removed
    /// </summary>
    /// <param name="questID"></param>
    /// <returns></returns>
    public bool RemoveQuestNodeIDFromPrerequisate(string questID)
    {
        //if the node contains the parentID remove it
        if (prerequisateQuests.Contains(questID))
        {
            prerequisateQuests.Remove(questID);
            IsNodeConnected();
            return true;
        }
        return false;
    }

    /// <summary>
    /// drag node
    /// </summary>
    /// <param name="delta"></param>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// check if the child node can be added to the parent node
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    private bool IsChildRoomValid(string childID)
    {
        //TODO: Have to comeup with some rules
        bool testValid = false;
        if (childID != questNodeID)
            testValid = true;
        if (typeOfQuest == QuestType.none)
            testValid = false;
        if (questNode.GetRoomNode(childID).typeOfQuest == QuestType.none)
            testValid = false;

        return testValid;
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

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
            questNode.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
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



#endif
}
