using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class DialogDetailsEditor : EditorWindow
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

    public static SO_Quests questBody;

    private SO_NPCList so_NPCS = null;
    private SO_ObjectiveDialogStart dialogStartDetails;
    private SO_ObjectiveDialogEnd dialogEndDetails;
    private SO_ObjectiveDialogBasic dialogBasicDetails;
    private SO_ObjectiveDialogBranch dialogBranchDetails;
    private SO_QuestDialogResults dialogResultsDetails;
    private CurrentWorkingNode currentNode = CurrentWorkingNode.none;

    private Dictionary<string, string> npcStarters = new Dictionary<string, string>();
    private Dictionary<string, string> sceneItemStarters = new Dictionary<string, string>();


    [MenuItem("Dialog Details Editor", menuItem = "Tools/Quest Editor/Dialog Details Editor")]
    public static void OpenWindow()
    {
        DialogDetailsEditor window = GetWindow<DialogDetailsEditor>("Dialog Details Editor");
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

    private void OnDisable()
    {
        //unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void BuildDictionaries()
    {
        //build lists for drop down boxes based on quest start type
        foreach (NPCList npc in so_NPCS.list)
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

    private string BuildPopupElement(Dictionary<string, string> valuePairs, string selected)
    {
        int index = 0;
        for (int i = 0; i < valuePairs.Count; i++)
        {
            if (valuePairs.Keys.ElementAt(i) == selected)
            {
                index = i;
            }
        }
        int selectedItem = EditorGUILayout.Popup("Select:", index, valuePairs.Values.ToArray());
        return valuePairs.Keys.ElementAt(selectedItem);
    }

    private void InputQuestDetails()
    {
        GUILayout.Label($"Quest Node ID:\n {questBody.questNodeID}");
        GUILayout.Space(_space);
        GUILayout.Label($"Type: {questBody.typeOfQuest.ToString()}");
        GUILayout.Space(_space);
        GUILayout.Label($"Dialog Start Node ID:\n {questBody.questStartStepID}");
        GUILayout.Space(_space);
        GUILayout.Label("Pre-requisate ID's:");
        for (int i = 0; i < questBody.prerequisateQuestsList.Count; i++)
        {
            GUILayout.Label($"{questBody.prerequisateQuestsList[i]}");
        }
        GUILayout.Space(_space);
        GUILayout.Label("Required for:");
        for (int i = 0; i < questBody.requiredForList.Count; i++)
        {
            GUILayout.Label($"{questBody.requiredForList[i]}");
        }
        GUILayout.Space(_space);
        QuestGiver oldGiver = questBody.questStartCondition;
        questBody.questStartCondition = (QuestGiver)EditorGUILayout.EnumPopup("Dialog Intiater: ", questBody.questStartCondition);
        if (oldGiver != questBody.questStartCondition)
        {
            questBody.questStarter = "";
        }
        GUILayout.Space(_space);
        switch (questBody.questStartCondition)
        {
            case QuestGiver.NPC:
                GUILayout.Label("Please select NPC to give dialog:");
                questBody.questStarter = BuildPopupElement(npcStarters, questBody.questStarter);
                break;
            case QuestGiver.SceneItem:
                GUILayout.Label("Please select scene item to start dialog:");
                questBody.questStarter = BuildPopupElement(sceneItemStarters, questBody.questStarter);

                break;
            case QuestGiver.EventTrigger:
                GUILayout.Label("Please enter Dialog Event Trigger:");
                questBody.questStarter = EditorGUILayout.TextField("", questBody.questStarter);
                break;
            default:
                break;
        }
        GUILayout.Space(_space);
        questBody.status = (QuestStatus)EditorGUILayout.EnumPopup("Quest Status: ", questBody.status);
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

    private void ProcessEvents(Event currentEvent)
    {
        //reset graph drag
        graphDrag = Vector2.zero;

        //get any room node that mouse is over if its null or not currently being dragged
        if (dialogStartDetails == null || dialogStartDetails.isLeftClickDragging == false ||
            dialogEndDetails == null || dialogEndDetails.isLeftClickDragging == false ||
            dialogBranchDetails == null || dialogBranchDetails.isLeftClickDragging == false ||
            dialogBasicDetails == null || dialogBasicDetails.isLeftClickDragging == false ||
            dialogResultsDetails == null || dialogResultsDetails.isLeftClickDragging == false )
        {
            //get type of node mouse is over
            IsMouseOverRoomNode(currentEvent);

            //quests = IsMouseOverRoomNode(currentEvent);
        }
        //if mouse isn't over a room node or we are currently dragging a line from the room node then process graph events
        if (currentNode == CurrentWorkingNode.none || questBody.dialogStartNodeToDrawLineFrom != null ||
            questBody.dialogEndNodeToDrawLineFrom != null || questBody.dialogBasicNodeToDrawLineFrom != null ||
            questBody.dialogBranchNodeToDrawLineFrom != null || questBody.dialogResultsNodeToDrawLineFrom != null )
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
                case CurrentWorkingNode.DialogStart:
                    dialogStartDetails.ProcessEvents(currentEvent);
                    break;
                case CurrentWorkingNode.DialogEnd:
                    dialogEndDetails.ProcessEvents(currentEvent);
                    break;
                case CurrentWorkingNode.QuestDialogResults:
                    dialogResultsDetails.ProcessEvents(currentEvent);
                    break;
                case CurrentWorkingNode.DialogBasic:
                    dialogBasicDetails.ProcessEvents(currentEvent);
                    break;
                case CurrentWorkingNode.DialogBranch:
                    dialogBranchDetails.ProcessEvents(currentEvent);
                    break;
                default:
                    break;
            }
        }
    }

    private void IsMouseOverRoomNode(Event currentEvent)
    {
        dialogStartDetails = null;
        dialogEndDetails = null;
        dialogBasicDetails = null;
        dialogBranchDetails = null;
        dialogResultsDetails = null;
        currentNode = CurrentWorkingNode.none;
        bool rectFound = false;

        if (questBody.dialogStartDetails != null)
        {
            if (questBody.dialogStartDetails.rect.Contains(currentEvent.mousePosition))
            {
                currentNode = CurrentWorkingNode.DialogStart;
                dialogStartDetails = questBody.dialogStartDetails;
                rectFound = true;
            }
        }
        if (questBody.dialogEndDetails != null)
        {
            if (questBody.dialogEndDetails.rect.Contains(currentEvent.mousePosition))
            {
                currentNode = CurrentWorkingNode.DialogEnd;
                dialogEndDetails = questBody.dialogEndDetails;
                rectFound = true;
            }
        }
        if (questBody.questDialogResultsDetails != null)
        {
            if (questBody.questDialogResultsDetails.rect.Contains(currentEvent.mousePosition))
            {
                currentNode = CurrentWorkingNode.QuestDialogResults;
                dialogResultsDetails = questBody.questDialogResultsDetails;
                rectFound = true;
            }
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            for (int i = questBody.dialogBasicDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.dialogBasicDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    currentNode = CurrentWorkingNode.DialogBasic;
                    dialogBasicDetails = questBody.dialogBasicDetailsList[i];
                    rectFound = true;
                }
            }
        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            for (int i = questBody.dialogBranchDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.dialogBranchDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    currentNode = CurrentWorkingNode.DialogBranch;
                    dialogBranchDetails = questBody.dialogBranchDetailsList[i];
                    rectFound = true;
                }
            }
        }
        if (!rectFound)
        {
            currentNode = CurrentWorkingNode.none;
        }
    }

    private string NodeIDMouseIsOver(Event currentEvent)
    {
        string nodeIDToReturn = "";
        if (questBody.dialogStartDetails != null)
        {
            if (questBody.dialogStartDetails.rect.Contains(currentEvent.mousePosition))
            {
                nodeIDToReturn = questBody.dialogStartDetails.dialogStartStepID;
            }
        }
        if (questBody.dialogEndDetails != null)
        {
            if (questBody.dialogEndDetails.rect.Contains(currentEvent.mousePosition))
            {
                nodeIDToReturn = questBody.dialogEndDetails.dialogEndStepID;
            }
        }
        if (questBody.questDialogResultsDetails != null)
        {
            if (questBody.questDialogResultsDetails.rect.Contains(currentEvent.mousePosition))
            {
                nodeIDToReturn = questBody.questDialogResultsDetails.questDialogResultsStepID;
            }
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            for (int i = questBody.dialogBasicDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.dialogBasicDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    nodeIDToReturn = questBody.dialogBasicDetailsList[i].dialogBasicStepID;
                }
            }
        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            for (int i = questBody.dialogBranchDetailsList.Count - 1; i >= 0; i--)
            {
                if (questBody.dialogBranchDetailsList[i].rect.Contains(currentEvent.mousePosition))
                {
                    nodeIDToReturn = questBody.dialogBranchDetailsList[i].dialogBranchStepID;
                }
            }
        }
        return nodeIDToReturn;
    }

    private void DrawDraggedLine()
    {
        if (questBody.linePosition != Vector2.zero)
        {
            switch (questBody.nodeTypeLineFrom)
            {
                case CurrentWorkingNode.DialogBasic:
                    Handles.DrawBezier(questBody.dialogBasicNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.dialogBasicNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                case CurrentWorkingNode.DialogBranch:
                    Handles.DrawBezier(questBody.dialogBranchNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.dialogBranchNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                case CurrentWorkingNode.DialogEnd:
                    Handles.DrawBezier(questBody.dialogEndNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.dialogEndNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                case CurrentWorkingNode.DialogStart:
                    Handles.DrawBezier(questBody.dialogStartNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.dialogStartNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                case CurrentWorkingNode.QuestDialogResults:
                    Handles.DrawBezier(questBody.dialogResultsNodeToDrawLineFrom.rect.center, questBody.linePosition,
                        questBody.dialogResultsNodeToDrawLineFrom.rect.center, questBody.linePosition, Color.white, null, connectingLineWidth);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// draw connections in the graph window between room nodes
    /// </summary>
    private void DrawRoomConnections()
    {
        //loop through all room nodes
        //start room first
        if (questBody.dialogStartDetails != null)
        {
            if (questBody.dialogStartDetails.childQuestStepID.Count > 0)
            {
                foreach (string node in questBody.dialogStartDetails.childQuestStepID)
                {
                    DrawConnectionLine(questBody.dialogStartDetails.dialogStartStepID, node);
                }
            }
        }
        if (questBody.dialogEndDetails != null)
        {
            if (questBody.dialogEndDetails.childQuestStepID.Count > 0)
            {
                foreach (string node in questBody.dialogEndDetails.childQuestStepID)
                {
                    DrawConnectionLine(questBody.dialogEndDetails.dialogEndStepID, node);
                }
            }
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic so_node in questBody.dialogBasicDetailsList)
            {
                if (so_node != null)
                {
                    if (so_node.childQuestStepID.Count > 0)
                    {
                        foreach (string node in so_node.childQuestStepID)
                        {
                            DrawConnectionLine(so_node.dialogBasicStepID, node);
                        }
                    }
                }
            }
        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch so_node in questBody.dialogBranchDetailsList)
            {
                if (so_node != null)
                {
                    if (so_node.childQuestStepID.Count > 0)
                    {
                        foreach (string node in so_node.childQuestStepID)
                        {
                            DrawConnectionLine(so_node.dialogBranchStepID, node);
                        }
                    }
                }
            }
        }
        GUI.changed = true;
    }

    private void DrawStepNodes()
    {
        if (questBody.dialogStartDetails != null)
        {
            if (questBody.dialogStartDetails.isSelected)
            {
                questBody.dialogStartDetails.Draw(questNodeSelectedStyle);
            }
            else
            {
                questBody.dialogStartDetails.Draw(questNodeStyle);
            }
        }
        if (questBody.dialogEndDetails != null)
        {
            if (questBody.dialogEndDetails.isSelected)
            {
                questBody.dialogEndDetails.Draw(questNodeSelectedStyle);
            }
            else
            {
                questBody.dialogEndDetails.Draw(questNodeStyle);
            }
        }
        if (questBody.questDialogResultsDetails != null)
        {
            if (questBody.questDialogResultsDetails.isSelected)
            {
                questBody.questDialogResultsDetails.Draw(questNodeSelectedStyle);
            }
            else
            {
                questBody.questDialogResultsDetails.Draw(questNodeStyle);
            }

        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch quests in questBody.dialogBranchDetailsList)
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
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic quests in questBody.dialogBasicDetailsList)
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

        if (questNode != null&&questNode.typeOfQuest==QuestNodeType.Dialog)
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
    /// clear line drag from a room node
    /// </summary>
    private void ClearLineDrag()
    {
        questBody.dialogStartNodeToDrawLineFrom = null;
        questBody.dialogEndNodeToDrawLineFrom = null;
        questBody.dialogBasicNodeToDrawLineFrom = null;
        questBody.dialogBranchNodeToDrawLineFrom = null;
        questBody.dialogResultsNodeToDrawLineFrom = null;
        questBody.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// process mouse up events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // is starting from an end node, clear line and return
        if (currentEvent.button == 1 && questBody.dialogEndNodeToDrawLineFrom != null)
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
        if (currentEvent.button == 1 && questBody.dialogStartNodeToDrawLineFrom != null)
        {
            nodeIDParent = questBody.dialogStartNodeToDrawLineFrom.dialogStartStepID;
            Debug.Log("Start Switch");
            switch (endNode)
            {
                case CurrentWorkingNode.DialogEnd:
                    SO_ObjectiveDialogEnd so_EndNode = questBody.GetDialogEndNodeByID(nodeIDChild);
                    if (questBody.dialogStartNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_EndNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogBasic:
                    SO_ObjectiveDialogBasic so_BasicNode = questBody.GetDialogBasicNodeByID(nodeIDChild);
                    if (questBody.dialogStartNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_BasicNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogBranch:
                    SO_ObjectiveDialogBranch so_BranchNode = questBody.GetDialogBranchNodeByID(nodeIDChild);
                    if (questBody.dialogStartNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_BranchNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogStart:
                case CurrentWorkingNode.QuestDialogResults:
                    ClearLineDrag();
                    break;
            }
        }
        else if (currentEvent.button == 1 && questBody.dialogBasicNodeToDrawLineFrom != null)
        {
            nodeIDParent = questBody.dialogBasicNodeToDrawLineFrom.dialogBasicStepID;
            Debug.Log("Basic Switch");
            switch (endNode)
            {
                case CurrentWorkingNode.DialogEnd:
                    SO_ObjectiveDialogEnd so_EndNode = questBody.GetDialogEndNodeByID(nodeIDChild);
                    if (questBody.dialogBasicNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_EndNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogBasic:
                    SO_ObjectiveDialogBasic so_BasicNode = questBody.GetDialogBasicNodeByID(nodeIDChild);
                    if (questBody.dialogBasicNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_BasicNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogBranch:
                    SO_ObjectiveDialogBranch so_BranchNode = questBody.GetDialogBranchNodeByID(nodeIDChild);
                    if (questBody.dialogBasicNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_BranchNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogStart:
                case CurrentWorkingNode.QuestDialogResults:
                    ClearLineDrag();
                    break;
            }

        }
        else if (currentEvent.button == 1 && questBody.dialogBranchNodeToDrawLineFrom != null)
        {
            nodeIDParent = questBody.dialogBranchNodeToDrawLineFrom.dialogBranchStepID;
            Debug.Log("Collect Switch");
            switch (endNode)
            {
                case CurrentWorkingNode.DialogEnd:
                    SO_ObjectiveDialogEnd so_EndNode = questBody.GetDialogEndNodeByID(nodeIDChild);
                    if (questBody.dialogBranchNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_EndNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogBasic:
                    SO_ObjectiveDialogBasic so_BasicNode = questBody.GetDialogBasicNodeByID(nodeIDChild);
                    if (questBody.dialogBranchNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_BasicNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogBranch:
                    SO_ObjectiveDialogBranch so_BranchNode = questBody.GetDialogBranchNodeByID(nodeIDChild);
                    if (questBody.dialogBranchNodeToDrawLineFrom.AddChildStepToQuestStep(nodeIDChild))
                    {
                        so_BranchNode.AddQuestStepIDToParent(nodeIDParent);
                    }
                    break;
                case CurrentWorkingNode.DialogStart:
                case CurrentWorkingNode.QuestDialogResults:
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
            ProcessLeftMouseDragEvent(currentEvent.delta, currentEvent);
        }
    }

    /// <summary>
    /// process left mouse drag event - drag room node graph
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta, Event currentEvent)
    {
        //get node id mouse over
        //string overNode = NodeIDMouseIsOver(currentEvent);
        //CurrentWorkingNode nodeType=quests.GetStepNodeType(overNode);
        if (questBody.dialogStartDetails == null && questBody.dialogEndDetails == null && questBody.dialogBasicDetailsList.Count == 0
            && questBody.dialogBranchDetailsList.Count == 0 && questBody.questDialogResultsDetails == null)
        {
            return;
        }

        graphDrag = dragDelta;
        if (questBody.dialogStartDetails != null)
        {
            questBody.dialogStartDetails.DragNode(dragDelta);
        }
        if (questBody.dialogEndDetails != null)
        {
            questBody.dialogEndDetails.DragNode(dragDelta);
        }
        if (questBody.questDialogResultsDetails != null)
        {
            questBody.questDialogResultsDetails.DragNode(dragDelta);
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            for (int i = 0; i < questBody.dialogBasicDetailsList.Count; i++)
            {
                questBody.dialogBasicDetailsList[i].DragNode(dragDelta);
            }
        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            for (int i = 0; i < questBody.dialogBranchDetailsList.Count; i++)
            {
                questBody.dialogBranchDetailsList[i].DragNode(dragDelta);
            }
        }
        GUI.changed = true;
    }


    /// <summary>
    /// process right mouse drag event - draw line
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (questBody.dialogStartNodeToDrawLineFrom != null || questBody.dialogEndNodeToDrawLineFrom != null || 
            questBody.dialogBasicNodeToDrawLineFrom != null || questBody.dialogBranchNodeToDrawLineFrom != null || 
            questBody.dialogResultsNodeToDrawLineFrom != null)
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

    /// <summary>
    /// Show the context menu
    /// </summary>
    /// <param name="mousePosition"></param>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        if (questBody.dialogStartDetails == null)
        {
            menu.AddItem(new GUIContent("Create Start Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.DialogStart));
            menu.AddSeparator("");
        }
        menu.AddItem(new GUIContent("Create Basic Dialog Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.DialogBasic));
        menu.AddItem(new GUIContent("Create Dialog Branch Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.DialogBranch));
        menu.AddItem(new GUIContent("Create End Node"), false, () => CreateQuestNode(mousePosition, CurrentWorkingNode.DialogEnd));        
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Node"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);
        menu.ShowAsContext();
    }

    private void DeleteSelectedRoomNodes()
    {
        Queue<string> nodeIDToDelete = new Queue<string>();
        CurrentWorkingNode nodeTypeToRemove = CurrentWorkingNode.none;
        string childID;

        if (questBody.dialogStartDetails != null && questBody.dialogStartDetails.isSelected && questBody.dialogEndDetails == null &&
            questBody.dialogBasicDetailsList.Count == 0 && questBody.dialogBranchDetailsList.Count == 0 &&
            questBody.questDialogResultsDetails == null)
        {
            nodeIDToDelete.Enqueue(questBody.dialogStartDetails.dialogStartStepID);
            for (int i = questBody.dialogStartDetails.childQuestStepID.Count - 1; i >= 0; i--)
            {
                childID = questBody.dialogStartDetails.childQuestStepID[i];
                nodeTypeToRemove = questBody.GetStepNodeType(questBody.dialogStartDetails.childQuestStepID[i]);
                DeleteParent(nodeTypeToRemove, CurrentWorkingNode.DialogStart, questBody.dialogStartDetails.dialogStartStepID, childID, true);
            }
        }
        else
        {
            Debug.Log("Can't delete Dialog Start node with other nodes present.");
        }
        if (questBody.dialogEndDetails != null && questBody.dialogEndDetails.isSelected)
        {
            //TODO: add steps to remove results node with end node
            nodeIDToDelete.Enqueue(questBody.dialogEndDetails.dialogEndStepID);
            for (int i = questBody.dialogEndDetails.childQuestStepID.Count - 1; i >= 0; i--)
            {
                childID = questBody.dialogEndDetails.childQuestStepID[i];
                nodeTypeToRemove = questBody.GetStepNodeType(questBody.dialogEndDetails.childQuestStepID[i]);
                DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestCollect, questBody.dialogEndDetails.dialogEndStepID, childID, true);
            }
            for (int i = questBody.dialogEndDetails.parentQuestStepID.Count - 1; i >= 0; i--)
            {
                childID = questBody.dialogEndDetails.parentQuestStepID[i];
                nodeTypeToRemove = questBody.GetStepNodeType(childID);
                DeleteChildren(CurrentWorkingNode.DialogEnd, nodeTypeToRemove, childID, questBody.dialogEndDetails.dialogEndStepID, true);
            }
            if(questBody.questDialogResultsDetails!=null)
            {
                nodeIDToDelete.Enqueue(questBody.questDialogResultsDetails.questDialogResultsStepID);
                for (int i = questBody.questDialogResultsDetails.parentQuestStepIDList.Count - 1; i >= 0; i--)
                {
                    childID = questBody.questDialogResultsDetails.parentQuestStepIDList[i];
                    nodeTypeToRemove = questBody.GetStepNodeType(childID);
                    DeleteChildren(CurrentWorkingNode.QuestDialogResults, nodeTypeToRemove, childID, questBody.questDialogResultsDetails.questDialogResultsStepID, true);
                }
            }
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic node in questBody.dialogBasicDetailsList)
            {
                if (node.isSelected)
                {
                    nodeIDToDelete.Enqueue(node.dialogBasicStepID);
                    for (int i = node.childQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.childQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(node.childQuestStepID[i]);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.DialogBasic, node.dialogBasicStepID, childID, true);
                    }
                    for (int i = node.parentQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.parentQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(childID);
                        DeleteChildren(CurrentWorkingNode.DialogBasic, nodeTypeToRemove, childID, node.dialogBasicStepID, true);
                    }

                }
            }
        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch node in questBody.dialogBranchDetailsList)
            {
                if (node.isSelected)
                {
                    nodeIDToDelete.Enqueue(node.dialogBranchStepID);
                    for (int i = node.childQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.childQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(node.childQuestStepID[i]);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.DialogBranch, node.dialogBranchStepID, childID, true);
                    }
                    for (int i = node.parentQuestStepID.Count - 1; i >= 0; i--)
                    {
                        childID = node.parentQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(childID);
                        DeleteChildren(CurrentWorkingNode.DialogBranch, nodeTypeToRemove, childID, node.dialogBranchStepID, true);
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
            //TODO: Add results node to case list
            switch (nodeTypeToDelete)
            {
                case CurrentWorkingNode.DialogStart:
                    SO_ObjectiveDialogStart startNode = questBody.GetDialogStartNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(startNode);
                    break;
                case CurrentWorkingNode.DialogEnd:
                    SO_ObjectiveDialogEnd endNode = questBody.GetDialogEndNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(endNode);
                    break;
                case CurrentWorkingNode.DialogBasic:
                    SO_ObjectiveDialogBasic collectNode = questBody.GetDialogBasicNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(collectNode);
                    break;
                case CurrentWorkingNode.DialogBranch:
                    SO_ObjectiveDialogBranch courierNode = questBody.GetDialogBranchNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(courierNode);
                    break;
                case CurrentWorkingNode.QuestDialogResults:
                    SO_QuestDialogResults resultsNode = questBody.GetResultsNodeByID(roomNodeToDelete);
                    questBody.RemoveNode(resultsNode);
                    break;
            }
        }
    }

    private void ProcessNodeForDeletion(string parentNodeID, CurrentWorkingNode parentNodeType, List<string> children, List<string> parents, bool flag)
    {

        if (children != null)
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                string childID = children[i];
                CurrentWorkingNode targetNodeType = questBody.GetStepNodeType(childID);
                DeleteChildren(targetNodeType, parentNodeType, parentNodeID, childID, flag);
                DeleteParent(targetNodeType, parentNodeType, parentNodeID, childID, flag);
            }
        }

        if (parents != null)
        {
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                string parentID = parents[i];
                CurrentWorkingNode targetType = questBody.GetStepNodeType(parentID);
                DeleteChildren(targetType, parentNodeType, parentNodeID, parentID, flag);
                DeleteParent(targetType, parentNodeType, parentNodeID, parentID, flag);
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
    /// <param name="childNodeType"></param>
    /// <param name="parentNodeType"></param>
    /// <param name="parentID"></param>
    /// <param name="childID"></param>
    /// <returns></returns>
    private bool DeleteChildren(CurrentWorkingNode childNodeType, CurrentWorkingNode parentNodeType,
        string parentID, string childID, bool ignoreSelected)
    {
        bool childSelected = false;
        bool nodeRemoved = false;
        Debug.Log($"child node type: {childNodeType.ToString()}");
        switch (childNodeType)
        {
            case CurrentWorkingNode.DialogStart:
                if (questBody.GetDialogStartNodeByID(childID).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }
                break;
            case CurrentWorkingNode.DialogEnd:
                if (questBody.GetDialogEndNodeByID(childID).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }
                break;
            case CurrentWorkingNode.DialogBasic:
                if (questBody.GetDialogBasicNodeByID(childID).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }
                break;
            case CurrentWorkingNode.DialogBranch:
                if (questBody.GetDialogBranchNodeByID(childID).isSelected || ignoreSelected)
                {
                    childSelected = true;
                }
                break;
            case CurrentWorkingNode.QuestDialogResults:
                if (questBody.GetResultsNodeByID(childID).isSelected || ignoreSelected)
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
            case CurrentWorkingNode.DialogStart:
                SO_ObjectiveDialogStart so_startParent = questBody.GetDialogStartNodeByID(parentID);
                if ((childSelected && so_startParent != null))
                {
                    so_startParent.RemoveChild(childID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.DialogEnd:
                SO_ObjectiveDialogEnd so_endParent = questBody.GetDialogEndNodeByID(parentID);
                if ((childSelected && so_endParent != null))
                {
                    so_endParent.RemoveChild(childID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.DialogBasic:
                SO_ObjectiveDialogBasic so_basicParent = questBody.GetDialogBasicNodeByID(parentID);
                if ((childSelected && so_basicParent != null))
                {
                    so_basicParent.RemoveChild(childID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.DialogBranch:
                SO_ObjectiveDialogBranch so_branchParent = questBody.GetDialogBranchNodeByID(parentID);
                if ((childSelected && so_branchParent != null))
                {
                    so_branchParent.RemoveChild(childID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestDialogResults:
                nodeRemoved = true;
                break;
        }
        return nodeRemoved;
    }

    private bool DeleteParent(CurrentWorkingNode childNodeType, CurrentWorkingNode parentNodeType, string parentID, string childID)
    {
        return DeleteParent(childNodeType, parentNodeType, parentID, childID, false);
    }


    /// <summary>
    /// Remove parents from selected children
    /// </summary>
    /// <param name="childNodeType"></param>
    /// <param name="parentNodeType"></param>
    /// <param name="parentID"></param>
    /// <param name="childID"></param>
    /// <returns></returns>
    private bool DeleteParent(CurrentWorkingNode childNodeType, CurrentWorkingNode parentNodeType,
        string parentID, string childID, bool ignoreSelected)
    {
        bool nodeRemoved = false;
        //remove parent
        switch (childNodeType)
        {
            case CurrentWorkingNode.DialogBasic:
                SO_ObjectiveDialogBasic so_BasicChild = questBody.GetDialogBasicNodeByID(childID);
                if ((so_BasicChild != null && so_BasicChild.isSelected) || (so_BasicChild != null && ignoreSelected))
                {
                    //remove parent from child
                    so_BasicChild.RemoveParent(parentID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.DialogBranch:
                SO_ObjectiveDialogBranch so_Branch = questBody.GetDialogBranchNodeByID(childID);
                if ((so_Branch != null && so_Branch.isSelected) || (so_Branch != null && ignoreSelected))
                {
                    //remove parent from child
                    so_Branch.RemoveParent(parentID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.DialogEnd:
                SO_ObjectiveDialogEnd so_End = questBody.GetDialogEndNodeByID(childID);
                if ((so_End != null && so_End.isSelected) || (so_End != null && ignoreSelected))
                {
                    //remove parent from child
                    so_End.RemoveParent(parentID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.QuestDialogResults:
                SO_QuestDialogResults so_Results = questBody.GetResultsNodeByID(childID);
                if ((so_Results != null && so_Results.isSelected) || (so_Results != null && ignoreSelected))
                {
                    //remove parent from child
                    so_Results.RemoveParent(parentID);
                    nodeRemoved = true;
                }
                break;
            case CurrentWorkingNode.DialogStart:
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
        if (questBody.dialogStartDetails != null)
        {
            if (questBody.dialogStartDetails.isSelected && questBody.dialogStartDetails.childQuestStepID.Count > 0)
            {
                for (int i = questBody.dialogStartDetails.childQuestStepID.Count - 1; i >= 0; i--)
                {
                    nodeToRemove = questBody.dialogStartDetails.childQuestStepID[i];
                    nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                    DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.DialogStart, questBody.dialogStartDetails.dialogStartStepID, nodeToRemove);
                    DeleteParent(nodeTypeToRemove, CurrentWorkingNode.DialogStart, questBody.dialogStartDetails.dialogStartStepID, nodeToRemove);

                }
            }
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic basicNode in questBody.dialogBasicDetailsList)
            {
                if (basicNode.isSelected && basicNode.childQuestStepID.Count > 0)
                {
                    for (int i = basicNode.childQuestStepID.Count - 1; i >= 0; i--)
                    {
                        nodeToRemove = basicNode.childQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.DialogBasic, basicNode.dialogBasicStepID, nodeToRemove);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.DialogBasic, basicNode.dialogBasicStepID, nodeToRemove);
                    }
                }
            }
        }
        if (questBody.dialogEndDetails != null)
        {
            if (questBody.dialogEndDetails.isSelected && questBody.dialogEndDetails.childQuestStepID.Count > 0)
            {
                for (int i = questBody.dialogEndDetails.childQuestStepID.Count - 1; i >= 0; i--)
                {
                    nodeToRemove = questBody.dialogEndDetails.childQuestStepID[i];
                    nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                    DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.DialogEnd, questBody.dialogEndDetails.dialogEndStepID, nodeToRemove);
                    DeleteParent(nodeTypeToRemove, CurrentWorkingNode.DialogEnd, questBody.dialogEndDetails.dialogEndStepID, nodeToRemove);
                }
            }

        }
        if (questBody.questDialogResultsDetails != null)
        {
            if (questBody.questDialogResultsDetails.isSelected && questBody.questDialogResultsDetails.parentQuestStepIDList.Count > 0)
            {
                for (int i = questBody.questDialogResultsDetails.parentQuestStepIDList.Count - 1; i >= 0; i--)
                {
                    nodeToRemove = questBody.questDialogResultsDetails.parentQuestStepIDList[i];
                    nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                    DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.QuestDialogResults, questBody.questDialogResultsDetails.questDialogResultsStepID, nodeToRemove);
                    DeleteParent(nodeTypeToRemove, CurrentWorkingNode.QuestDialogResults, questBody.questDialogResultsDetails.questDialogResultsStepID, nodeToRemove);
                }
            }

        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch branchNode in questBody.dialogBranchDetailsList)
            {
                if (branchNode.isSelected && branchNode.childQuestStepID.Count > 0)
                {
                    for (int i = branchNode.childQuestStepID.Count - 1; i >= 0; i--)
                    {
                        nodeToRemove = branchNode.childQuestStepID[i];
                        nodeTypeToRemove = questBody.GetStepNodeType(nodeToRemove);
                        DeleteChildren(nodeTypeToRemove, CurrentWorkingNode.DialogBranch, branchNode.dialogBranchStepID, nodeToRemove);
                        DeleteParent(nodeTypeToRemove, CurrentWorkingNode.DialogBranch, branchNode.dialogBranchStepID, nodeToRemove);
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
        if (questBody.dialogStartDetails != null && questBody.dialogStartDetails.isSelected)
        {
            questBody.dialogStartDetails.isSelected = false;
        }
        if (questBody.dialogEndDetails != null && questBody.dialogEndDetails.isSelected)
        {
            questBody.dialogEndDetails.isSelected = false;
        }
        if (questBody.questDialogResultsDetails != null && questBody.questDialogResultsDetails.isSelected)
        {
            questBody.questDialogResultsDetails.isSelected = false;
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic basic in questBody.dialogBasicDetailsList)
            {
                if (basic.isSelected)
                {
                    basic.isSelected = false;
                }
            }
        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch brach in questBody.dialogBranchDetailsList)
            {
                if (brach.isSelected)
                {
                    brach.isSelected = false;
                }
            }
        }
        GUI.changed = true;
    }


    private void SelectAllRoomNodes()
    {
        if (questBody.dialogStartDetails != null)
        {
            questBody.dialogStartDetails.isSelected = true;
        }
        if (questBody.dialogEndDetails != null)
        {
            questBody.dialogEndDetails.isSelected = true;
        }
        if (questBody.questDialogResultsDetails != null)
        {
            questBody.questDialogResultsDetails.isSelected = true;
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic collect in questBody.dialogBasicDetailsList)
            {
                collect.isSelected = true;
            }
        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch courier in questBody.dialogBranchDetailsList)
            {
                courier.isSelected = true;
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
        if (questBody.dialogStartDetails == null && questBody.dialogEndDetails == null && 
            questBody.questDialogResultsDetails == null && questBody.dialogBasicDetailsList.Count == 0 &&
            questBody.dialogBranchDetailsList.Count == 0 && nodeToCreate != CurrentWorkingNode.QuestStart)
        {
            SO_ObjectiveDialogStart startNode = ScriptableObject.CreateInstance<SO_ObjectiveDialogStart>();
            startNode.Initialise(new Rect(new Vector2(375f, 25f), new Vector2(nodeWidth, nodeHeight)), questBody);
            questBody.dialogStartDetails = startNode;
            questBody.questStartStepID = startNode.dialogStartStepID;
            startNode.questID = questBody.questNodeID;
            AssetDatabase.AddObjectToAsset(startNode, questBody);
            AssetDatabase.SaveAssets();
        }
        switch (nodeToCreate)
        {
            case CurrentWorkingNode.DialogBasic:
                SO_ObjectiveDialogBasic basicNode = ScriptableObject.CreateInstance<SO_ObjectiveDialogBasic>();
                questBody.dialogBasicDetailsList.Add(basicNode);
                basicNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), questBody);
                basicNode.questID = questBody.questNodeID;
                AssetDatabase.AddObjectToAsset(basicNode, questBody);
                AssetDatabase.SaveAssets();
                break;
            case CurrentWorkingNode.DialogBranch:
                SO_ObjectiveDialogBranch branchNode = ScriptableObject.CreateInstance<SO_ObjectiveDialogBranch>();
                questBody.dialogBranchDetailsList.Add(branchNode);
                branchNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), questBody);
                branchNode.questID = questBody.questNodeID;
                AssetDatabase.AddObjectToAsset(branchNode, questBody);
                AssetDatabase.SaveAssets();
                break;
            case CurrentWorkingNode.DialogEnd:
                if (questBody.dialogStartDetails != null)
                {
                    SO_ObjectiveDialogEnd endNode = ScriptableObject.CreateInstance<SO_ObjectiveDialogEnd>();
                    questBody.dialogEndDetails = endNode;
                    endNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), questBody);
                    endNode.questID = questBody.questNodeID;
                    AssetDatabase.AddObjectToAsset(endNode, questBody);
                    AssetDatabase.SaveAssets();
                    //add call to automatically draw results node and link it to end node
                    BuildResultsAndLink(endNode.rect, endNode);
                }
                break;
            case CurrentWorkingNode.QuestStart:
                break;
        }
        AssetDatabase.Refresh();
        questBody.OnValidate();
    }

    private void BuildResultsAndLink(Rect nodeLocation, SO_ObjectiveDialogEnd endDetails)
    {
        //build node
        SO_QuestDialogResults results = ScriptableObject.CreateInstance<SO_QuestDialogResults>();
        questBody.questDialogResultsDetails = results;

        nodeLocation.y = nodeLocation.y + (1.5f * nodeHeight);
        results.Initialise(nodeLocation, questBody, QuestNodeType.Dialog);
        results.questID = questBody.questNodeID;
        AssetDatabase.AddObjectToAsset(results, questBody);
        AssetDatabase.SaveAssets();
        //link node
        endDetails.AddChildStepToQuestStep(results.questDialogResultsStepID);
        results.AddQuestStepIDToParent(endDetails.dialogEndStepID);
    }


    private void ClearAllSelectedRoomNodes()
    {
        if (questBody.dialogStartDetails != null)
        {
            if (questBody.dialogStartDetails.isSelected)
            {
                questBody.dialogStartDetails.isSelected = false;
            }
        }
        if (questBody.dialogEndDetails != null)
        {
            if (questBody.dialogEndDetails.isSelected)
            {
                questBody.dialogEndDetails.isSelected = false;
            }
        }
        if (questBody.questDialogResultsDetails != null)
        {
            if (questBody.questDialogResultsDetails.isSelected)
            {
                questBody.questDialogResultsDetails.isSelected = false;
            }
        }
        if (questBody.dialogBasicDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBasic collect in questBody.dialogBasicDetailsList)
            {
                if (collect.isSelected)
                {
                    collect.isSelected = false;
                }
            }
        }
        if (questBody.dialogBranchDetailsList.Count > 0)
        {
            foreach (SO_ObjectiveDialogBranch courier in questBody.dialogBranchDetailsList)
            {
                if (courier.isSelected)
                {
                    courier.isSelected = false;
                }
            }
        }
        GUI.changed = true;
    }
    private void OnValidateMousePos(Event currentEvent)
    {
        string overNode = questBody.GetNodeIDFromLocationDictionary(currentEvent.mousePosition);
        CurrentWorkingNode nodeType = questBody.GetStepNodeType(overNode);
        QuestStepEditor.CallEditor(questBody, overNode, nodeType);
    }

}
