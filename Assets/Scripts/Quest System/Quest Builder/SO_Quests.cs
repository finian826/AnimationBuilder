using System;
using System.Collections.Generic;
using UnityEditor;
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
     public List<SO_ObjectiveCollect> collectDetails = new List<SO_ObjectiveCollect>();
     public List<SO_ObjectiveCourier> couierDetails = new List<SO_ObjectiveCourier>();
     public List<SO_ObjectiveTask> taskDetails = new List<SO_ObjectiveTask>();
    [HideInInspector] public Dictionary<string, CurrentWorkingNode> questStepDictionary = new Dictionary<string, CurrentWorkingNode>();

    private void Awake()
    {
        BuildStepDictionary();
    }

    private void BuildStepDictionary()
    {
        questStepDictionary.Clear();
        if(questStartDetails != null)
        {
            questStepDictionary[questStartDetails.questStartID] = CurrentWorkingNode.QuestStart;
        }
        if (questEndDetails != null)
        {
            questStepDictionary[questEndDetails.questEndID] = CurrentWorkingNode.QuestEnd;
        }
        if(collectDetails.Count > 0)
        {
            foreach(SO_ObjectiveCollect nodeID in collectDetails)
            {
                questStepDictionary[nodeID.collectQuestStepID] = CurrentWorkingNode.QuestCollect;
            }
        }
        if (couierDetails.Count > 0)
        {
            foreach (SO_ObjectiveCourier nodeID in couierDetails)
            {
                questStepDictionary[nodeID.courierQuestStepID] = CurrentWorkingNode.QuestCourier;
            }
        }
        if (taskDetails.Count > 0)
        {
            foreach (SO_ObjectiveTask nodeID in taskDetails)
            {
                questStepDictionary[nodeID.taskQuestStepID] = CurrentWorkingNode.QuestTask;
            }
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
        if (taskDetails.Count > 0)
        {
            foreach (SO_ObjectiveTask item in taskDetails)
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
        if (collectDetails.Count > 0)
        {
            foreach (SO_ObjectiveCollect item in collectDetails)
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
        if (couierDetails.Count > 0)
        {
            foreach (SO_ObjectiveCourier item in couierDetails)
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
        if (questStartDetails != null)
        {
            nodeLocations[questStartDetails.questStartID] = questStartDetails.rect;
        }
        if(questEndDetails != null)
        {
            nodeLocations[questEndDetails.questEndID] = questEndDetails.rect;
        }
        if(taskDetails.Count > 0)
        {
            foreach(SO_ObjectiveTask task in taskDetails)
            {
                nodeLocations[task.taskQuestStepID]=task.rect;
            }
        }
        if(collectDetails.Count > 0)
        {
            foreach (SO_ObjectiveCollect collect in collectDetails)
            {
                nodeLocations[collect.collectQuestStepID] = collect.rect;
            }
        }
        if(couierDetails.Count > 0)
        {
            foreach(SO_ObjectiveCourier couier in couierDetails)
            {
                nodeLocations[couier.courierQuestStepID] = couier.rect;
            }
        }
    }

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
        if(collectDetails.Count > 0)
        {
            foreach(SO_ObjectiveCollect item in collectDetails)
            {
                if (item == node)
                {
                    collectDetails.Remove(node);
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
        if (couierDetails.Count > 0)
        {
            foreach (SO_ObjectiveCourier item in couierDetails)
            {
                if (item == node)
                {
                    couierDetails.Remove(node);
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
        if (taskDetails.Count > 0)
        {
            foreach (SO_ObjectiveTask item in taskDetails)
            {
                if (item == node)
                {
                    taskDetails.Remove(node);
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
        if(collectDetails.Count > 0)
        {
            for(int i = 0; i < collectDetails.Count; i++)
            {
                //remove node from assets database
                DestroyImmediate(collectDetails[i], true);

                //save the asset database
                AssetDatabase.SaveAssets();
            }
        }
        if(couierDetails.Count > 0)
        {
            for (int i = 0; i < couierDetails.Count; i++)
            {
                //remove node from assets database
                DestroyImmediate(couierDetails[i], true);

                //save the asset database
                AssetDatabase.SaveAssets();
            }
        }
        if(taskDetails.Count > 0)
        {
            for (int i = 0; i < taskDetails.Count; i++)
            {
                //remove node from assets database
                DestroyImmediate(taskDetails[i], true);

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
