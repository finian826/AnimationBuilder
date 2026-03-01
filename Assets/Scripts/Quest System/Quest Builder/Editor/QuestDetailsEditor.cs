using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class QuestDetailsEditor : EditorWindow
{
    //Node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    //UI Values
    private float _space = 5f;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    //grid spacing
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    //connecting line values
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;

    private GUIStyle questNodeStyle;
    private GUIStyle questNodeSelectedStyle;
    private GUIStyle guiScreenStyle;
    
    
    private static SO_Quests questBody = null;

    private SO_NPCList so_NPCS = null;
    private static SO_QuestStartDetails questStartDetails = null;
    private static SO_QuestEndDetails questEndDetails = null;
    private static SO_ObjectiveCollect collectDetails = null;
    private static SO_ObjectiveCourier courierDetails = null;
    private static SO_ObjectiveTask taskDetails = null;
    private CurrentWorkingNode currentNode = CurrentWorkingNode.none;

    private Dictionary<string,string> npcStarters = new Dictionary<string,string>();
    private Dictionary<string, string> sceneItemStarters = new Dictionary<string, string>();

    [MenuItem("Quest Details Editor", menuItem = "Tools/Quest Editor/Quest Details Editor")]
    public static void OpenWindow()
    {
        QuestDetailsEditor window = GetWindow<QuestDetailsEditor>("Quest Details Editor");
        Vector2 maxSize = new Vector2(1600, 1080);
        Vector2 minsize = new Vector2(640, 480);

        window.minSize = minsize;
        window.maxSize = maxSize;
        window.Show();
    }
    private void InspectorSelectionChanged()
    {
        SO_Quests roomNodeGraph = Selection.activeObject as SO_Quests;
        if (roomNodeGraph != null)
        {
            questBody = roomNodeGraph;
            GUI.changed = true;
        }
    }

    private void OnEnable()
    {
        //define node layout style
        questNodeStyle = new GUIStyle();
        questNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        questNodeStyle.normal.textColor = Color.white;
        questNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        questNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //define selected node style
        questNodeSelectedStyle = new GUIStyle();
        questNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        questNodeSelectedStyle.normal.textColor = Color.white;
        questNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        questNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //define selected node style
        guiScreenStyle = new GUIStyle();
        guiScreenStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        guiScreenStyle.normal.textColor = Color.white;
        guiScreenStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        guiScreenStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //subscribe to the inspector selection changed event
        Selection.selectionChanged += InspectorSelectionChanged;

        so_NPCS = GameResources.Instance.npcList;
        BuildDictionaries();
    }

    private void BuildDictionaries()
    {
        //build lists for drop down boxes based on quest start type
        foreach(NPCList npc in so_NPCS.list)
        {
            switch (npc.startingType)
            {
                case QuestGiver.SceneItem:
                    sceneItemStarters.Add(npc.NPCid, npc.NPCName);
                    break;
                case QuestGiver.NPC:
                    npcStarters.Add(npc.NPCid, npc.NPCName);
                    break;
                default:
                    break;
            }
        }
    }

    private void OnDisable()
    {
        //unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void InputQuestDetails()
    {
        GUILayout.Label($"Quest Node ID:\n {questBody.questNodeID}");
        GUILayout.Space(_space);
        GUILayout.Label($"Type: {questBody.typeOfQuest.ToString()}");
        GUILayout.Space(_space);
        GUILayout.Label($"Detail Node ID:\n {questBody.questStartStepID}");
        GUILayout.Space(_space);
        GUILayout.Label("Pre-requisate quests ID's:");
        for(int i=0;i<questBody.prerequisateQuests.Count;i++) 
        {
            GUILayout.Label($"{questBody.prerequisateQuests[i]}");
        }
        GUILayout.Space(_space);
        GUILayout.Label("Required for Quests:");
        for(int i = 0; i < questBody.requiredFor.Count; i++)
        {
            GUILayout.Label($"{questBody.requiredFor[i]}");
        }
        GUILayout.Space(_space);
        QuestGiver oldGiver = questBody.questStartCondition;
        questBody.questStartCondition = (QuestGiver)EditorGUILayout.EnumPopup("Quest Giver: ", questBody.questStartCondition);
        if(oldGiver != questBody.questStartCondition)
        {
            questBody.questStarter = "";
        }
        GUILayout.Space(_space);
        switch(questBody.questStartCondition)
        {
            case QuestGiver.NPC:
                GUILayout.Label("Please select NPC to give quest:");
                questBody.questStarter = BuildPopupElement(npcStarters, questBody.questStarter);
                break;
            case QuestGiver.SceneItem:
                GUILayout.Label("Please select scene item to start quest:");
                questBody.questStarter = BuildPopupElement(sceneItemStarters, questBody.questStarter);

                break;
            case QuestGiver.EventTrigger:
                GUILayout.Label("Please enter Quest Event Trigger:");
                questBody.questStarter = EditorGUILayout.TextField("",questBody.questStarter);
                break;
            default:
                break;                
        }        
        GUILayout.Space(_space);        
        questBody.status = (QuestStatus)EditorGUILayout.EnumPopup("Quest Status: ", questBody.status);
    }

    private string BuildPopupElement(Dictionary<string,string> valuePairs,string selected)
    {
        int index = 0;
        for(int i=0;i<valuePairs.Count;i++)
        {
            if (valuePairs.Keys.ElementAt(i) == selected)
            {
                index = i;
            }
        }
        int selectedItem = EditorGUILayout.Popup("Select:", index, valuePairs.Values.ToArray());
        return valuePairs.Keys.ElementAt(selectedItem);
    }

    private void OnGUI()
    {
        float sourceAreaWidth = 300;
        float sourceAreaHeight = 450;
        GUILayout.BeginArea(new Rect(5, 5, sourceAreaWidth, sourceAreaHeight), guiScreenStyle);
        InputQuestDetails();
        GUILayout.EndArea();

        // if a scriptable object of type so_RoomNodeGraph has been selected then process
        if (questBody != null)
        {
            //draw grid
            DrawBackground(gridSmall, 0.2f, Color.gray);
            DrawBackground(gridLarge, 0.3f, Color.gray);

            //draw line if being dragged
            DrawDraggedLine();

            //process events
            ProcessEvents(Event.current);

            //draw connections between room nodes
            DrawRoomConnections();

            //draw room nodes
            DrawStepNodes();

        }
    }

    /// <summary>
    /// draw connections in the graph window between room nodes
    /// </summary>
    private void DrawRoomConnections()
    {
        //loop through all room nodes
        //start room first
        if (questBody.questStartDetails != null)
        {
            if (questBody.questStartDetails.childQuestStepID.Count > 0)
            {
                foreach (string node in questBody.questStartDetails.childQuestStepID)
                {
                    DrawConnectionLine(questBody.questStartDetails.questStartID, node);
                }
            }
        }
        if (questBody.questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask so_node in questBody.questTaskDetailsList)
            {
                if (so_node != null)
                {
                    if (so_node.childQuestStepID.Count > 0)
                    {
                        foreach (string node in so_node.childQuestStepID)
                        {
                            DrawConnectionLine(so_node.taskQuestStepID, node);
                        }
                    }
                }
            }
        }
        if (questBody.questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier so_node in questBody.questCouierDetailsList)
            {
                if (so_node != null)
                {
                    if (so_node.childQuestStepID.Count > 0)
                    {
                        foreach (string node in so_node.childQuestStepID)
                        {
                            DrawConnectionLine(so_node.courierQuestStepID, node);
                        }
                    }
                }
            }
        }
        if (questBody.questCollectDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCollect so_node in questBody.questCollectDetailsList)
            {
                if (so_node != null)
                {
                    if (so_node.childQuestStepID.Count > 0)
                    {
                        foreach (string node in so_node.childQuestStepID)
                        {
                            DrawConnectionLine(so_node.collectQuestStepID, node);
                        }
                    }
                }
            }
        }
        GUI.changed = true;
    }

    private void DrawConnectionLine(string parentRoomNode, string childRoomNode)
    {

        //get line start and end points
        Vector2 startPosition = questBody.GetRectFromLocationDirectory(parentRoomNode).center;
        Vector2 endPosition = questBody.GetRectFromLocationDirectory(childRoomNode).center;

        //calculate mid point
        Vector2 midPosition = (startPosition + endPosition) / 2;
        //vector from start to end position of line
        Vector2 direction = endPosition - startPosition;
        //calculate normalised perpendicular positions from the mid point
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        //calculate mid point offset position for arrow head
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;
        //draw arrow
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        //draw the line
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    private void DrawDraggedLine()
    {
        if (questBody.linePosition != Vector2.zero)
        {
            switch (questBody.nodeTypeLineFrom)
            {
                case CurrentWorkingNode.QuestStart:
                    Handles.DrawBezier(questBody.startNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.startNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                case CurrentWorkingNode.QuestEnd:
                    Handles.DrawBezier(questBody.endNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.endNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                case CurrentWorkingNode.QuestCollect:
                    Handles.DrawBezier(questBody.collectNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.collectNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                case CurrentWorkingNode.QuestCourier:
                    Handles.DrawBezier(questBody.couierNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.couierNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                case CurrentWorkingNode.QuestTask:
                    Handles.DrawBezier(questBody.taskNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.taskNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                default:
                    break;
            }
        }
    }

    private void IsMouseOverRoomNode(Event currentEvent)
    {
        questStartDetails = null;
        questEndDetails = null;
        taskDetails = null;
        collectDetails = null;
        courierDetails = null;
        currentNode = CurrentWorkingNode.none;

        if (questBody.questStartDetails != null)
        {
            if (questBody.questStartDetails.rect.Contains(currentEvent.mousePosition))
            {
                currentNode = CurrentWorkingNode.QuestStart;
                questStartDetails = questBody.questStartDetails;
            }
        }
        if (questBody.questEndDetails != null)
        {
            if (questBody.questEndDetails.rect.Contains(currentEvent.mousePosition))
            {
                currentNode = CurrentWorkingNode.QuestEnd;
                questEndDetails = questBody.questEndDetails;
            }
        }
        if (questBody.questTaskDetailsList.Count > 0)
        {
            for (int i = questBody.questTaskDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.questTaskDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    currentNode = CurrentWorkingNode.QuestTask;
                    taskDetails = questBody.questTaskDetailsList[i];
                }
            }
        }
        if (questBody.questCollectDetailsList.Count > 0)
        {
            for (int i = questBody.questCollectDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.questCollectDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    currentNode = CurrentWorkingNode.QuestCollect;
                    collectDetails = questBody.questCollectDetailsList[i];
                }
            }
        }
        if (questBody.questCouierDetailsList.Count > 0)
        {
            for (int i = questBody.questCouierDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.questCouierDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    currentNode = CurrentWorkingNode.QuestCourier;
                    courierDetails = questBody.questCouierDetailsList[i];
                }
            }
        }
        else
        {
            currentNode = CurrentWorkingNode.none;
        }
    }

    private string NodeIDMouseIsOver(Event currentEvent)
    {
        string nodeIDToReturn="";
        if (questBody.questStartDetails != null)
        {
            if (questBody.questStartDetails.rect.Contains(currentEvent.mousePosition))
            {
                nodeIDToReturn = questBody.questStartDetails.questStartID;
            }
        }
        if (questBody.questEndDetails != null)
        {
            if (questBody.questEndDetails.rect.Contains(currentEvent.mousePosition))
            {
                nodeIDToReturn=questBody.questEndDetails.questEndID;
            }
        }
        if (questBody.questTaskDetailsList.Count > 0)
        {
            for (int i = questBody.questTaskDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.questTaskDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    nodeIDToReturn = questBody.questTaskDetailsList[i].taskQuestStepID;
                }
            }
        }
        if (questBody.questCollectDetailsList.Count > 0)
        {
            for (int i = questBody.questCollectDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.questCollectDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    nodeIDToReturn = questBody.questCollectDetailsList[i].collectQuestStepID;
                }
            }
        }
        if (questBody.questCouierDetailsList.Count > 0)
        {
            for (int i = questBody.questCouierDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.questCouierDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    nodeIDToReturn = questBody.questCouierDetailsList[i].courierQuestStepID;
                }
            }
        }
        return nodeIDToReturn;
    }

    private void ProcessEvents(Event currentEvent)
    {
        //reset graph drag
        graphDrag = Vector2.zero;

        //get any room node that mouse is over if its null or not currently being dragged
        if (questStartDetails == null || questStartDetails.isLeftClickDragging == false ||
            questEndDetails == null || questEndDetails.isLeftClickDragging == false ||
            collectDetails == null || collectDetails.isLeftClickDragging == false ||
            courierDetails == null || courierDetails.isLeftClickDragging == false ||
            taskDetails == null || taskDetails.isLeftClickDragging == false)
        {
            //get type of node mouse is over
            IsMouseOverRoomNode(currentEvent);
            
            //quests = IsMouseOverRoomNode(currentEvent);
        }
        //if mouse isn't over a room node or we are currently dragging a line from the room node then process graph events
        if (currentNode == CurrentWorkingNode.none || questBody.startNodeToDrawLineFrom != null ||
            questBody.endNodeToDrawLineFrom != null || questBody.collectNodeToDrawLineFrom != null ||
            questBody.couierNodeToDrawLineFrom != null || questBody.taskNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else if (currentNode != CurrentWorkingNode.none && currentEvent.button == 0 && currentEvent.clickCount == 2)
        {
            Debug.Log("double Click");
            OnValidateMousePos(currentEvent);
        }
        else
        {
            //add switch event to choose which node to run process events on.
            //Debug.Log("single Click");
            //process room node events
            switch (currentNode)
            {
                case CurrentWorkingNode.QuestStart:
                    questStartDetails.ProcessEvents(currentEvent);
                    break;
                case CurrentWorkingNode.QuestEnd:
                    questEndDetails.ProcessEvents(currentEvent);
                    break;
                case CurrentWorkingNode.QuestTask:
                    taskDetails.ProcessEvents(currentEvent);
                    break;
                case CurrentWorkingNode.QuestCourier:
                    courierDetails.ProcessEvents(currentEvent);
                    break;
                case CurrentWorkingNode.QuestCollect:
                    collectDetails.ProcessEvents(currentEvent);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Process Room Node Graph Events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            //process mouse down event
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// process mouse up events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // is starting from an end node, clear line and return
        if (currentEvent.button == 1 && questBody.endNodeToDrawLineFrom != null)
        {
            ClearLineDrag();
            return;
        }
        CurrentWorkingNode endNode = CurrentWorkingNode.none;
        string nodeIDChild;
        string nodeIDParent;
        nodeIDChild = NodeIDMouseIsOver(currentEvent);
        endNode = questBody.GetStepNodeType(nodeIDChild);
        Debug.Log($" Child Node: {nodeIDChild} Node Type: {endNode.ToString()}");
        //if releasing the right mouse button and currently dragging a line
        if (currentEvent.button == 1 && questBody.startNodeToDrawLineFrom != null)
        {
            nodeIDParent = questBody.startNodeToDrawLineFrom.questStartID;
            Debug.Log("Start Switch");
            switch (endNode)
            {
                case CurrentWorkingNode.QuestEnd:
                    SO_QuestEndDetails so_EndNode = questBody.GetEndNodeByID(nodeIDChild);
                    if (questBody.startNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_EndNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestCourier:
                    SO_ObjectiveCourier so_CourierNode = questBody.GetCourierNodeByID(nodeIDChild);
                    if (questBody.startNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_CourierNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestCollect:
                    SO_ObjectiveCollect so_CollectNode=questBody.GetCollectNodeByID(nodeIDChild);
                    if (questBody.startNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_CollectNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestTask:
                    SO_ObjectiveTask so_TaskNode=questBody.GetTaskNodeByID(nodeIDChild);
                    if (questBody.startNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_TaskNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestStart:
                    ClearLineDrag();
                    break;
            }
        }
        else if (currentEvent.button == 1 && questBody.taskNodeToDrawLineFrom != null)
        {
            nodeIDParent = questBody.taskNodeToDrawLineFrom.taskQuestStepID;
            Debug.Log("Task Switch");
            switch (endNode)
            {
                case CurrentWorkingNode.QuestEnd:
                    SO_QuestEndDetails so_EndNode = questBody.GetEndNodeByID(nodeIDChild);
                    if (questBody.taskNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_EndNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestCourier:
                    SO_ObjectiveCourier so_CourierNode = questBody.GetCourierNodeByID(nodeIDChild);
                    if (questBody.taskNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_CourierNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestCollect:
                    SO_ObjectiveCollect so_CollectNode = questBody.GetCollectNodeByID(nodeIDChild);
                    if (questBody.taskNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_CollectNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestTask:
                    SO_ObjectiveTask so_TaskNode = questBody.GetTaskNodeByID(nodeIDChild);
                    if (questBody.taskNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_TaskNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestStart:
                    ClearLineDrag();
                    break;
            }

        }
        else if (currentEvent.button == 1 && questBody.collectNodeToDrawLineFrom != null)
        {
            nodeIDParent = questBody.collectNodeToDrawLineFrom.collectQuestStepID;
            Debug.Log("Collect Switch");
            switch (endNode)
            {
                case CurrentWorkingNode.QuestEnd:
                    SO_QuestEndDetails so_EndNode = questBody.GetEndNodeByID(nodeIDChild);
                    if (questBody.collectNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_EndNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestCourier:
                    SO_ObjectiveCourier so_CourierNode = questBody.GetCourierNodeByID(nodeIDChild);
                    if (questBody.collectNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_CourierNode.AddQuestStepIDToParent(nodeIDParent);
                    }

                    break;
                case CurrentWorkingNode.QuestCollect:
                    SO_ObjectiveCollect so_CollectNode = questBody.GetCollectNodeByID(nodeIDChild);
                    if (questBody.collectNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_CollectNode.AddQuestStepIDToParent(nodeIDParent);
                    }

                    break;
                case CurrentWorkingNode.QuestTask:
                    SO_ObjectiveTask so_TaskNode = questBody.GetTaskNodeByID(nodeIDChild);
                    if (questBody.collectNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_TaskNode.AddQuestStepIDToParent(nodeIDParent);
                    }

                    break;
                case CurrentWorkingNode.QuestStart:
                    ClearLineDrag();
                    break;
            }

        }
        else if (currentEvent.button == 1 && questBody.couierNodeToDrawLineFrom != null)
        {
            nodeIDParent = questBody.couierNodeToDrawLineFrom.courierQuestStepID;
            Debug.Log("Courier Switch");
            switch (endNode)
            {
                case CurrentWorkingNode.QuestEnd:
                    SO_QuestEndDetails so_EndNode = questBody.GetEndNodeByID(nodeIDChild);
                    if (questBody.couierNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_EndNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.QuestCourier:
                    SO_ObjectiveCourier so_CourierNode = questBody.GetCourierNodeByID(nodeIDChild);
                    if (questBody.couierNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_CourierNode.AddQuestStepIDToParent(nodeIDParent);
                    }

                    break;
                case CurrentWorkingNode.QuestCollect:
                    SO_ObjectiveCollect so_CollectNode = questBody.GetCollectNodeByID(nodeIDChild);
                    if (questBody.couierNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_CollectNode.AddQuestStepIDToParent(nodeIDParent);
                    }

                    break;
                case CurrentWorkingNode.QuestTask:
                    SO_ObjectiveTask so_TaskNode = questBody.GetTaskNodeByID(nodeIDChild);
                    if (questBody.couierNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_TaskNode.AddQuestStepIDToParent(nodeIDParent);
                    }

                    break;
                case CurrentWorkingNode.QuestStart:
                    ClearLineDrag();
                    break;
            }

        }
            ClearLineDrag();        
    }


    /// <summary>
    /// process mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta,currentEvent);
        }
    }

    /// <summary>
    /// process right mouse drag event - draw line
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (questBody.startNodeToDrawLineFrom != null || questBody.endNodeToDrawLineFrom != null || questBody.taskNodeToDrawLineFrom != null ||
            questBody.couierNodeToDrawLineFrom != null || questBody.collectNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    /// <summary>
    /// draw connecting line from node to mouse cursor
    /// </summary>
    /// <param name="mousePosition"></param>
    private void DragConnectingLine(Vector2 mousePosition)
    {
        questBody.linePosition += mousePosition;
    }


    /// <summary>
    /// process left mouse drag event - drag room node graph
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta,Event currentEvent)
    {
        //get node id mouse over
        //string overNode = NodeIDMouseIsOver(currentEvent);
        //CurrentWorkingNode nodeType=quests.GetStepNodeType(overNode);
        graphDrag = dragDelta;
        if (questBody.questStartDetails.questStartID !=null)
        {
            questBody.questStartDetails.DragNode(dragDelta);
        }
        if(questBody.questEndDetails.questEndID !=null)
        {
            questBody.questEndDetails.DragNode(dragDelta);
        }
        if (questBody.questTaskDetailsList.Count>0)
        {
            for(int i=0;i<questBody.questTaskDetailsList.Count;i++)
            {
                    questBody.questTaskDetailsList[i].DragNode(dragDelta);
            }
        }
        if (questBody.questCollectDetailsList.Count>0)
        {
            for(int i=0;i<questBody.questCollectDetailsList.Count; i++)
            {
                    questBody.questCollectDetailsList[i].DragNode(dragDelta);
            }
        }
        if (questBody.questCouierDetailsList.Count>0)
        {
            for(int i = 0; i < questBody.questCouierDetailsList.Count; i++)
            {
                    questBody.questCouierDetailsList[i].DragNode(dragDelta);
            }
        }
            GUI.changed = true;
    }



    /// <summary>
    /// Process mouse down events on the room node graph (not over a node)
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //process right click mouse down on graph event (chow context menu)
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }

    }

    private void ClearAllSelectedRoomNodes()
    {
        if (questBody.questStartDetails != null)
        {
            if (questBody.questStartDetails.isSelected)
            {
                questBody.questStartDetails.isSelected = false;
            }
        }
        if (questBody.questEndDetails != null)
        {
            if (questBody.questEndDetails.isSelected)
            {
                questBody.questEndDetails.isSelected = false;
            }
        }
        if (questBody.questCollectDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCollect collect in questBody.questCollectDetailsList)
            {
                if (collect.isSelected)
                {
                    collect.isSelected = false;
                }
            }
        }
        if (questBody.questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier courier in questBody.questCouierDetailsList)
            {
                if (courier.isSelected)
                {
                    courier.isSelected = false;
                }
            }
        }
        if (questBody.questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask task in questBody.questTaskDetailsList)
            {
                if (task.isSelected)
                {
                    task.isSelected = false;
                }
            }
        }
        GUI.changed = true;
    }

    /// <summary>
    /// clear line drag from a room node
    /// </summary>
    private void ClearLineDrag()
    {
        questBody.startNodeToDrawLineFrom = null;
        questBody.endNodeToDrawLineFrom = null;
        questBody.collectNodeToDrawLineFrom = null;
        questBody.couierNodeToDrawLineFrom = null;
        questBody.taskNodeToDrawLineFrom = null;
        questBody.linePosition = Vector2.zero;
        GUI.changed = true;
    }


    private void DrawBackground(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) +
                gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) +
                gridOffset);
        }
        Handles.color = Color.white;
    }

    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        SO_Quests questNode = EditorUtility.EntityIdToObject(instanceID) as SO_Quests;

        if (questNode != null && questNode.typeOfQuest == QuestType.Quest)
        {
            OpenWindow();
            questBody = questNode;
            return true;
        }
        return false;
    }

    public static bool CallEditor(SO_Quests quest)
    {
        if (quest != null)
        {
            OpenWindow();
            questBody = quest;
            return true;
        }
        return false;
    }

    private void OnValidateMousePos(Event currentEvent)
    {
        string overNode = questBody.GetNodeIDFromLocationDictionary(currentEvent.mousePosition);
        CurrentWorkingNode nodeType= questBody.GetStepNodeType(overNode);
        QuestStepEditor.CallEditor(questBody, overNode, nodeType);
    }

    /// <summary>
    /// Show the context menu
    /// </summary>
    /// <param name="mousePosition"></param>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        if (questBody.questStartDetails == null)
        {
            menu.AddItem(new GUIContent("Create Start Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.QuestStart));
            menu.AddSeparator("");
        }
        menu.AddItem(new GUIContent("Create Task Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.QuestTask));
        menu.AddItem(new GUIContent("Create Courier Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.QuestCourier));
        menu.AddItem(new GUIContent("Create Collect Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.QuestCollect));
        if (questBody.questEndDetails == null)
        {
            menu.AddItem(new GUIContent("Create End Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.QuestEnd));
        }
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Node"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);
        menu.ShowAsContext();
    }

    private void DeleteSelectedRoomNodes()
    {
        Queue<string> nodeIDToDelete=new Queue<string>();
        CurrentWorkingNode nodeTypeToRemove = CurrentWorkingNode.none;
        string childID;

        if (questBody.questStartDetails != null && questBody.questStartDetails.isSelected && questBody.questEndDetails == null &&
            questBody.questTaskDetailsList.Count == 0 && questBody.questCollectDetailsList.Count == 0 && questBody.questCouierDetailsList.Count == 0)
        {
            nodeIDToDelete.Enqueue(questBody.questStartDetails.questStartID);
            for (int i = questBody.questStartDetails.childQuestStepID.Count - 1; i >= 0; i--)
            {
                childID = questBody.questStartDetails.childQuestStepID[i];
                nodeTypeToRemove = questBody.GetStepNodeType(questBody.questStartDetails.childQuestStepID[i]);
                DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestStart, questBody.questStartDetails.questStartID, childID, true);
                DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestStart, questBody.questStartDetails.questStartID, childID, true);
            }
        }
        else
        {
            Debug.Log("Can't delete Quest Start node with other nodes present.");
        }
        if (questBody.questEndDetails != null && questBody.questEndDetails.isSelected)
        {
            nodeIDToDelete.Enqueue(questBody.questEndDetails.questEndID);
            for (int i = questBody.questEndDetails.parentQuestStepID.Count - 1; i >= 0; i--)
            {
                childID = questBody.questEndDetails.parentQuestStepID[i];
                nodeTypeToRemove = questBody.GetStepNodeType(childID);
                DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestEnd, questBody.questEndDetails.questEndID, childID, true);
                DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestEnd, questBody.questEndDetails.questEndID, childID, true);
            }
        }
        if(questBody.questCollectDetailsList.Count>0)
        {
            foreach(SO_ObjectiveCollect node in questBody.questCollectDetailsList)
            {
                if (node.isSelected)
                {
                    nodeIDToDelete.Enqueue(node.collectQuestStepID);
                    for (int i = node.childQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.childQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(node.childQuestStepID[i]);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestCollect, node.collectQuestStepID, childID, true);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestCollect, node.collectQuestStepID, childID, true);
                    }
                    for (int i = node.parentQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.parentQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(childID);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestCollect, node.collectQuestStepID, childID, true);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestCollect, node.collectQuestStepID, childID, true);
                    }

                }
            }
        }
        if (questBody.questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier node in questBody.questCouierDetailsList)
            {
                if (node.isSelected)
                {
                    nodeIDToDelete.Enqueue(node.courierQuestStepID);
                    for (int i = node.childQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.childQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(node.childQuestStepID[i]);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestCourier, node.courierQuestStepID, childID, true);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestCourier, node.courierQuestStepID, childID, true);
                    }
                    for (int i = node.parentQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.parentQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(childID);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestCourier, node.courierQuestStepID, childID, true);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestCourier, node.courierQuestStepID, childID, true);
                    }

                }
            }
        }
        if (questBody.questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask node in questBody.questTaskDetailsList)
            {
                if (node.isSelected)
                {
                    nodeIDToDelete.Enqueue(node.taskQuestStepID);
                    for (int i = node.childQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.childQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(node.childQuestStepID[i]);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestTask, node.taskQuestStepID, childID, true);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestTask, node.taskQuestStepID, childID, true);
                    }
                    for (int i = node.parentQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.parentQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(childID);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestTask, node.taskQuestStepID, childID, true);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestTask, node.taskQuestStepID, childID, true);
                    }

                }
            }
        }
        while (nodeIDToDelete.Count > 0)
        {
            //get room node from queue
            string roomNodeToDelete = nodeIDToDelete.Dequeue();

            //determine step type and delete node
            CurrentWorkingNode nodeTypeToDelete = questBody.GetStepNodeType(roomNodeToDelete);
            switch(nodeTypeToDelete)
            {
                case CurrentWorkingNode.QuestStart:
                    SO_QuestStartDetails startNode = questBody.GetStartNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(startNode);
                    break;
                case CurrentWorkingNode.QuestEnd:
                    SO_QuestEndDetails endNode = questBody.GetEndNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(endNode);
                    break;
                case CurrentWorkingNode.QuestCollect:
                    SO_ObjectiveCollect collectNode = questBody.GetCollectNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(collectNode);
                    break;
                case CurrentWorkingNode.QuestCourier:
                    SO_ObjectiveCourier courierNode = questBody.GetCourierNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(courierNode);
                    break;
                case CurrentWorkingNode.QuestTask:
                    SO_ObjectiveTask taskNode = questBody.GetTaskNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(taskNode);
                    break;
            }
        }
    }

    private bool DeleteChildren(CurrentWorkingNode nodeTypeToRemove, CurrentWorkingNode parentNodeType, string parentID, string nodeToRemove)
    {
        return DeleteChildren(nodeTypeToRemove, parentNodeType, parentID, nodeToRemove, false);
    }


    /// <summary>
    /// Remove children from selected parent steps
    /// </summary>
    /// <param name="nodeTypeToRemove"></param>
    /// <param name="parentNodeType"></param>
    /// <param name="parentID"></param>
    /// <param name="nodeToRemove"></param>
    /// <returns></returns>
    private bool DeleteChildren(CurrentWorkingNode nodeTypeToRemove, CurrentWorkingNode parentNodeType, 
        string parentID, string nodeToRemove,bool ignoreSelected)
    {
        bool childSelected = false;
        bool nodeRemoved = false;
        Debug.Log($"child node type: {nodeTypeToRemove.ToString()}");
        switch (nodeTypeToRemove)
        {
            case CurrentWorkingNode.QuestStart:
                if (questBody.GetStartNodeByID(nodeToRemove).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }
                    break;
            case CurrentWorkingNode.QuestEnd:
                if (questBody.GetEndNodeByID(nodeToRemove).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }                
                break;
            case CurrentWorkingNode.QuestTask:
                if (questBody.GetTaskNodeByID(nodeToRemove).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }
                break;
            case CurrentWorkingNode.QuestCollect:
                if (questBody.GetCollectNodeByID(nodeToRemove).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }
                break;
            case CurrentWorkingNode.QuestCourier:
                if (questBody.GetCourierNodeByID(nodeToRemove).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }
                break;
            default:
                childSelected = false;
                break;
        }
        //remove child
        Debug.Log($"child selected: {childSelected.ToString()}");
        switch (parentNodeType)
        {
            case CurrentWorkingNode.QuestCollect:
                SO_ObjectiveCollect so_CollectParent = questBody.GetCollectNodeByID(parentID);
                if ((childSelected && so_CollectParent != null))
                {
                    so_CollectParent.RemoveChild(nodeToRemove);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestCourier:
                SO_ObjectiveCourier so_CourierParent = questBody.GetCourierNodeByID(parentID);
                if ((childSelected && so_CourierParent != null))
                {
                    so_CourierParent.RemoveChild(nodeToRemove);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestTask:
                SO_ObjectiveTask so_TaskParent = questBody.GetTaskNodeByID(parentID);
                if ((childSelected && so_TaskParent != null))
                {
                    so_TaskParent.RemoveChild(nodeToRemove);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestStart:
                SO_QuestStartDetails so_StartParent = questBody.GetStartNodeByID(parentID);
                if ((childSelected && so_StartParent != null))
                {
                    so_StartParent.RemoveChild(nodeToRemove);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestEnd:
                nodeRemoved = true;
                break;
        }
        return nodeRemoved;
    }

    private bool DeleteParent(CurrentWorkingNode nodeTypeToRemove, CurrentWorkingNode parentNodeType, string parentID, string nodeToRemove)
    {
        return DeleteParent(nodeTypeToRemove, parentNodeType, parentID, nodeToRemove, false);
    }


    /// <summary>
    /// Remove parents from selected children
    /// </summary>
    /// <param name="nodeTypeToRemove"></param>
    /// <param name="parentNodeType"></param>
    /// <param name="parentID"></param>
    /// <param name="nodeToRemove"></param>
    /// <returns></returns>
    private bool DeleteParent(CurrentWorkingNode nodeTypeToRemove, CurrentWorkingNode parentNodeType, 
        string parentID, string nodeToRemove,bool ignoreSelected)
    {
        bool nodeRemoved = false;
        //remove parent
        switch (nodeTypeToRemove)
        {
            case CurrentWorkingNode.QuestCollect:
                SO_ObjectiveCollect so_Collect = questBody.GetCollectNodeByID(nodeToRemove);
                if ((so_Collect != null && so_Collect.isSelected) || (so_Collect != null && ignoreSelected))
                {
                    //remove parent from child
                    so_Collect.RemoveParent(parentID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestCourier:
                SO_ObjectiveCourier so_Courier = questBody.GetCourierNodeByID(nodeToRemove);
                if ((so_Courier != null && so_Courier.isSelected) || (so_Courier != null && ignoreSelected))
                {
                    //remove parent from child
                    so_Courier.RemoveParent(parentID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestTask:
                SO_ObjectiveTask so_Task = questBody.GetTaskNodeByID(nodeToRemove);
                if ((so_Task != null && so_Task.isSelected) || (so_Task != null && ignoreSelected))
                {
                    //remove parent from child
                    so_Task.RemoveParent(parentID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestEnd:
                SO_QuestEndDetails so_End = questBody.GetEndNodeByID(nodeToRemove);
                if ((so_End != null && so_End.isSelected) || (so_End != null && ignoreSelected))
                {
                    //remove parent from child
                    so_End.RemoveParent(parentID);
                    nodeRemoved = true;
                }
                break;
                case CurrentWorkingNode.QuestStart:
                nodeRemoved = true;
                break;
            default:
                break;
        }
        return nodeRemoved;
    }

    /// <summary>
    /// delete the links between the selected room nodes
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        string nodeToRemove;
        CurrentWorkingNode nodeTypeToRemove = CurrentWorkingNode.none;
        if (questBody.questStartDetails != null)
        {
            if (questBody.questStartDetails.isSelected && questBody.questStartDetails.childQuestStepID.Count > 0)
            {
                for (int i = questBody.questStartDetails.childQuestStepID.Count - 1; i >= 0; i--)
                {
                    nodeToRemove = questBody.questStartDetails.childQuestStepID[i];
                    nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                    DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestStart, questBody.questStartDetails.questStartID, nodeToRemove);
                    DeleteParent(nodeTypeToRemove,CurrentWorkingNode.QuestStart ,questBody.questStartDetails.questStartID, nodeToRemove);

                }
            }
        }
        if (questBody.questCollectDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCollect collectNode in questBody.questCollectDetailsList)
            {
                if (collectNode.isSelected && collectNode.parentQuestStepID.Count > 0)
                {
                    for (int i = collectNode.parentQuestStepID.Count - 1; i >= 0; i--)
                    {
                        nodeToRemove = collectNode.parentQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestStart, collectNode.collectQuestStepID, nodeToRemove);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestStart, collectNode.collectQuestStepID, nodeToRemove);
                    }
                }
            }
        }
        if (questBody.questEndDetails != null)
        {
            if (questBody.questEndDetails.isSelected && questBody.questEndDetails.parentQuestStepID.Count > 0)
            {
                for (int i = questBody.questEndDetails.parentQuestStepID.Count - 1; i >= 0; i--)
                {
                    nodeToRemove = questBody.questEndDetails.parentQuestStepID[i];
                    nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                    DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestStart, questBody.questEndDetails.questEndID, nodeToRemove);
                    DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestStart, questBody.questEndDetails.questEndID, nodeToRemove);
                }
            }

        }
        if (questBody.questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier courierNode in questBody.questCouierDetailsList)
            {
                if (courierNode.isSelected && courierNode.parentQuestStepID.Count > 0)
                {
                    for (int i = courierNode.parentQuestStepID.Count - 1; i >= 0; i--)
                    {
                        nodeToRemove = courierNode.parentQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestStart, courierNode.courierQuestStepID, nodeToRemove);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestStart, courierNode.courierQuestStepID, nodeToRemove);
                    }
                }
            }
        }
        if (questBody.questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask taskNode in questBody.questTaskDetailsList)
            {
                if (taskNode.isSelected && taskNode.parentQuestStepID.Count > 0)
                {
                    for (int i = taskNode.parentQuestStepID.Count - 1; i >= 0; i--)
                    {
                        nodeToRemove = taskNode.parentQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestStart, taskNode.taskQuestStepID, nodeToRemove);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestStart, taskNode.taskQuestStepID, nodeToRemove);
                    }
                }
            }
        }
        //clear all selected room nodes
        ClearAllSelectedQuestNodes();
    }

    /// <summary>
    /// clear all selected room nodes
    /// </summary>
    private void ClearAllSelectedQuestNodes()
    {
        if (questBody.questStartDetails != null&& questBody.questStartDetails.isSelected)
        {
            questBody.questStartDetails.isSelected = false;
        }
        if (questBody.questEndDetails != null&& questBody.questEndDetails.isSelected)
        {
            questBody.questEndDetails.isSelected = false;
        }
        if (questBody.questCollectDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCollect collect in questBody.questCollectDetailsList)
            {
                if (collect.isSelected)
                {
                    collect.isSelected = false;
                }
            }
        }
        if (questBody.questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier courier in questBody.questCouierDetailsList)
            {
                if (courier.isSelected)
                {
                    courier.isSelected = false;
                }
            }
        }
        if (questBody.questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask task in questBody.questTaskDetailsList)
            {
                if (task.isSelected)
                {
                    task.isSelected = false;
                }
            }
        }
        GUI.changed = true;
    }


    private void SelectAllRoomNodes()
    {
        if (questBody.questStartDetails != null)
        {
            questBody.questStartDetails.isSelected = true;
        }
        if(questBody.questEndDetails != null)
        {
            questBody.questEndDetails.isSelected = true;
        }
        if (questBody.questCollectDetailsList.Count > 0)
        {
            foreach(SO_ObjectiveCollect collect in questBody.questCollectDetailsList)
            {
                collect.isSelected = true;
            }
        }
        if(questBody.questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier courier in questBody.questCouierDetailsList)
            {
                courier.isSelected = true;
            }
        }
        if(questBody.questTaskDetailsList.Count > 0)
        {
            foreach(SO_ObjectiveTask task in questBody.questTaskDetailsList)
            {
                task.isSelected = true;
            }
        }
        GUI.changed = true;
    }

    /// <summary>
    /// Create a room node at the mouse position - overloaded to also pass in RoomNodeType
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <param name="roomNodeType"></param>
    private void CreateQuestNode(object mousePositionObject, CurrentWorkingNode nodeToCreate)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;
        //if current quest nodes are empty then add quest start node first
        if (questBody.questStartDetails == null && questBody.questEndDetails == null && questBody.questCollectDetailsList.Count == 0 &&
            questBody.questTaskDetailsList.Count == 0 && questBody.questCouierDetailsList.Count == 0 && nodeToCreate != CurrentWorkingNode.QuestStart)
        {
            SO_QuestStartDetails startNode = ScriptableObject.CreateInstance<SO_QuestStartDetails>();
            startNode.Initialise(new Rect(new Vector2(375f, 25f), new Vector2(nodeWidth, nodeHeight)), questBody);
            questBody.questStartDetails = startNode;
            questBody.questStartStepID = startNode.questStartID;
            startNode.questID = questBody.questNodeID;
            AssetDatabase.AddObjectToAsset(startNode, questBody);
            AssetDatabase.SaveAssets();
        }
        switch (nodeToCreate)
        {
            case CurrentWorkingNode.QuestCollect:
                SO_ObjectiveCollect collectNode = ScriptableObject.CreateInstance<SO_ObjectiveCollect>();
                questBody.questCollectDetailsList.Add(collectNode);
                collectNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), questBody);
                collectNode.questID = questBody.questNodeID;               
                AssetDatabase.AddObjectToAsset(collectNode, questBody);
                AssetDatabase.SaveAssets();
                break;
            case CurrentWorkingNode.QuestCourier:
                SO_ObjectiveCourier courierNode = ScriptableObject.CreateInstance<SO_ObjectiveCourier>();
                questBody.questCouierDetailsList.Add(courierNode);
                courierNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), questBody);
                AssetDatabase.AddObjectToAsset(courierNode, questBody);
                AssetDatabase.SaveAssets();
                courierNode.questID = questBody.questNodeID;
                break;
            case CurrentWorkingNode.QuestTask:
                SO_ObjectiveTask taskNode = ScriptableObject.CreateInstance<SO_ObjectiveTask>();
                questBody.questTaskDetailsList.Add(taskNode);
                taskNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), questBody);
                AssetDatabase.AddObjectToAsset(taskNode, questBody);
                AssetDatabase.SaveAssets();
                taskNode.questID = questBody.questNodeID;
                break;
            case CurrentWorkingNode.QuestEnd:
                if (questBody.questStartDetails != null)
                {
                    SO_QuestEndDetails endNode = ScriptableObject.CreateInstance<SO_QuestEndDetails>();
                    questBody.questEndDetails = endNode;
                    endNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), questBody);
                    AssetDatabase.AddObjectToAsset(endNode, questBody);
                    AssetDatabase.SaveAssets();
                    endNode.questID = questBody.questNodeID;
                }
                break;
            case CurrentWorkingNode.QuestStart:
                break;
        }
        questBody.OnValidate();
    }

    private void DrawStepNodes()
    {
        if(questBody.questStartDetails != null)
        {
            if (questBody.questStartDetails.isSelected)
            {
                questBody.questStartDetails.Draw(questNodeSelectedStyle);
            }
            else
            {
                questBody.questStartDetails.Draw(questNodeStyle);
            }
        }
        if (questBody.questEndDetails != null)
        {
            if (questBody.questEndDetails.isSelected)
            {
                questBody.questEndDetails.Draw(questNodeSelectedStyle);
            }
            else
            {
                questBody.questEndDetails.Draw(questNodeStyle);
            }
        }
        if (questBody.questCollectDetailsList.Count > 0)
        {
            foreach(SO_ObjectiveCollect quests in questBody.questCollectDetailsList)
            {
                if (quests.isSelected)
                {
                    quests.Draw(questNodeSelectedStyle);
                }
                else
                {
                    quests.Draw(questNodeStyle);
                }
            }
        }
        if (questBody.questCouierDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveCourier quests in questBody.questCouierDetailsList)
            {
                if (quests.isSelected)
                {
                    quests.Draw(questNodeSelectedStyle);
                }
                else
                {
                    quests.Draw(questNodeStyle);
                }
            }
        }
        if (questBody.questTaskDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveTask quests in questBody.questTaskDetailsList)
            {
                if (quests.isSelected)
                {
                    quests.Draw(questNodeSelectedStyle);
                }
                else
                {
                    quests.Draw(questNodeStyle);
                }
            }
        }
        GUI.changed = true;
    }
}
