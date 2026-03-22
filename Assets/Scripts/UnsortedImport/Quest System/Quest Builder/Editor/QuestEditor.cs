using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class QuestEditor : EditorWindow
{
    //Node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    //mouse values
    //private float lastClickTime = 0f;
    //private const float doubleClickThreshold = 0.5f;

    //grid spacing
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    //connecting line values
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;

    private GUIStyle questNodeStyle;
    private GUIStyle questNodeSelectedStyle;

    //Scriptable Objects
    private static SO_QuestNode questNode;
    private SO_Quests currentQuest;



    [MenuItem("Quest Node Editor", menuItem = "Tools/Quest Editor/Quest Node Editor")]
    private static void OpenWindow()
    {
        GetWindow<QuestEditor>("Quest Node Editor");
    }

    private void OnEnable()
    {
        //subscribe to the inspector selection changed event
        Selection.selectionChanged += InspectorSelectionChanged;

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

        
    }

    private void OnDisable()
    {
        //unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void OnGUI()
    {
        // if a scriptable object of type so_RoomNodeGraph has been selected then process
        if (questNode != null)
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
            DrawQuestNodes();
        }
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
        SO_QuestNode questNodeGraph = EditorUtility.EntityIdToObject(instanceID) as SO_QuestNode;

        if (questNodeGraph != null)
        {
            OpenWindow();
            questNode = questNodeGraph;
            return true;
        }
        return false;
    }

    private void InspectorSelectionChanged()
    {
        SO_QuestNode roomNodeGraph = Selection.activeObject as SO_QuestNode;
        if (roomNodeGraph != null)
        {
            questNode = roomNodeGraph;
            GUI.changed = true;
        }
    }

    /// <summary>
    /// draw connections in the graph window between room nodes
    /// </summary>
    private void DrawRoomConnections()
    {
        //loop through all room nodes
        foreach (SO_Quests roomNode in questNode.questList)
        {
            if (roomNode.requiredForList.Count > 0)
            {
                //loop through child room nodes
                foreach (string childRoomNodeID in roomNode.requiredForList)
                {
                    //get the child room node from dictionary
                    if (questNode.questNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, questNode.questNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }


    private void DrawConnectionLine(SO_Quests parentRoomNode, SO_Quests childRoomNode)
    {
        //get line start and end points
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

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
        if (questNode.linePosition != Vector2.zero)
        {
            //draw line from node to line position
            Handles.DrawBezier(questNode.roomNodeToDrawLineFrom.rect.center, questNode.linePosition,
                questNode.roomNodeToDrawLineFrom.rect.center, questNode.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        //reset graph drag
        graphDrag = Vector2.zero;

        //get room node that mouse is over if its null or not currently being dragged
        if (currentQuest == null || currentQuest.isLeftClickDragging == false)
        {
            currentQuest = IsMouseOverRoomNode(currentEvent);
        }
        //if mouse isn't over a room node or we are currently dragging a line from the room node then process graph events
        if (currentQuest == null || questNode.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else if (currentQuest != null && currentEvent.button == 0 && currentEvent.clickCount == 2)
        {
            OnValidate();
        }
        else
        {
            //process room node events
            currentQuest.ProcessEvents(currentEvent);
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
        //if releasing the right mouse button and currently dragging a line
        if (currentEvent.button == 1 && questNode.roomNodeToDrawLineFrom != null)
        {
            //check if over a node
            SO_Quests quest = IsMouseOverRoomNode(currentEvent);

            if (quest != null)
            {
                //if so set it as a child of the parent room node if it can be added
                if (questNode.roomNodeToDrawLineFrom.AddRequiredForToQuestNode(quest.questNodeID))
                {
                    //set parent id in child room node
                    quest.AddQuestNodeIDToPrerequisate(questNode.roomNodeToDrawLineFrom.questNodeID);
                }
            }

            ClearLineDrag();
        } 
    }
        private SO_Quests IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = questNode.questList.Count - 1; i >= 0; i--)
        {
            if (questNode.questList[i].rect.Contains(currentEvent.mousePosition))
            {
                return questNode.questList[i];
            }
        }
        return null;
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
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }


    /// <summary>
    /// process left mouse drag event - drag room node graph
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;
        for (int i = 0; i < questNode.questList.Count; i++)
        {
            questNode.questList[i].DragNode(dragDelta);
        }
        GUI.changed = true;
    }

    /// <summary>
    /// process right mouse drag event - draw line
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (questNode.roomNodeToDrawLineFrom != null)
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
        questNode.linePosition += mousePosition;
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
    /// clear all selected room nodes
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach (SO_Quests quest in questNode.questList)
        {
            if (quest.isSelected)
            {
                quest.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// clear line drag from a room node
    /// </summary>
    private void ClearLineDrag()
    {
        questNode.roomNodeToDrawLineFrom = null;
        questNode.linePosition = Vector2.zero;
        GUI.changed = true;
    }


    /// <summary>
    /// Show the context menu
    /// </summary>
    /// <param name="mousePosition"></param>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateQuestNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Bulk Create/Create 5 Room Nodes"), false, () => BulkCreateRoomNode(mousePosition, 5));
        menu.AddItem(new GUIContent("Bulk Create/Create 10 Room Nodes"), false, () => BulkCreateRoomNode(mousePosition, 10));
        menu.AddItem(new GUIContent("Bulk Create/Create 15 Room Nodes"), false, () => BulkCreateRoomNode(mousePosition, 15));
        menu.AddItem(new GUIContent("Select All Room Node"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);
        menu.ShowAsContext();
    }

    /// <summary>
    /// delete the links between the selected room nodes
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        //iterate through the room nodes
        foreach (SO_Quests quests in questNode.questList)
        {
            if (quests.isSelected && quests.requiredForList.Count > 0)
            {
                for (int i = quests.requiredForList.Count - 1; i >= 0; i--)
                {
                    //get the child room node
                    SO_Quests childRoomNode = questNode.GetRoomNode(quests.requiredForList[i]);
                    //if the child room node is selected
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        //remove childid from parent room node
                        quests.RemoveRequiredForFromQuestNode(childRoomNode.questNodeID);
                        //remove parentid from child room node
                        childRoomNode.RemoveQuestNodeIDFromPrerequisate(quests.questNodeID);
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
        foreach (SO_Quests roomNode in questNode.questList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// delete all selected room nodes
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<SO_Quests> roomNodeDeletionQueue = new Queue<SO_Quests>();

        foreach (SO_Quests quest in questNode.questList)
        {
            if (quest.isSelected)
            {
                roomNodeDeletionQueue.Enqueue(quest);

                //iterate through child room nodes ids
                foreach (string reguiredFor in quest.requiredForList)
                {
                    //retrieve child room node
                    SO_Quests childRoomNode = questNode.GetRoomNode(reguiredFor);
                    if (childRoomNode != null)
                    {
                        //remove parent id from child room node
                        childRoomNode.RemoveQuestNodeIDFromPrerequisate(quest.questNodeID);
                    }
                }
                //iterate through parent room node ids
                foreach (string parentRoomNodeID in quest.prerequisateQuestsList)
                {
                    SO_Quests parentRoomNode = questNode.GetRoomNode(parentRoomNodeID);
                    if (parentRoomNode != null)
                    {
                        //remove childid from parent
                        parentRoomNode.RemoveRequiredForFromQuestNode(quest.questNodeID);
                    }

                }
            }
        }

        while (roomNodeDeletionQueue.Count > 0)
        {
            //get room node from queue
            SO_Quests roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            //delete all steps quest steps from the node
            roomNodeToDelete.DeleteStepNodes();

            // remove node from dictionary
            questNode.questNodeDictionary.Remove(roomNodeToDelete.questNodeID);

            //remove node from list
            questNode.questList.Remove(roomNodeToDelete);

            //remove node from assets database
            DestroyImmediate(roomNodeToDelete, true);

            //save the asset database
            AssetDatabase.SaveAssets();
        }
    }

    private void SelectAllRoomNodes()
    {
        foreach (SO_Quests roomNode in questNode.questList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    private void BulkCreateRoomNode(object mousePositionObject, int numNodes)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        for (int i = 0; i < numNodes; i++)
        {
            CreateQuestNode(mousePosition, QuestNodeType.none);
            mousePosition = new Vector2(mousePosition.x + 15f, mousePosition.y + 15f);
        }
    }

    private void CreateQuestNode(object mousePosition)
    {
        //if current room node graph is empty then add enterence room node first

        CreateQuestNode(mousePosition, QuestNodeType.none);
    }

    private void CreateQuestNode(object mousePositionObject, QuestNodeType questType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        //create room node scriptable object asset
        SO_Quests quests = ScriptableObject.CreateInstance<SO_Quests>();
        //add room node to current room node graph
        questNode.questList.Add(quests);
        //set room node values
        quests.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), questNode, questType);
        //add room node to room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(quests, questNode);
        AssetDatabase.SaveAssets();
        //refresh graph node dictionary
        questNode.OnValidate();
    }

    private void DrawQuestNodes()
    {
        foreach (SO_Quests quests in questNode.questList)
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
        GUI.changed = true;
    }

    private void OnValidate()
    {
        Event currentEvent;
        currentEvent = Event.current;
        SO_Quests overNode = IsMouseOverRoomNode(currentEvent);
        if (overNode != null && overNode.isConnected && overNode.callEditor && overNode.typeOfQuest != QuestNodeType.none &&
            currentEvent.button == 0 && currentEvent.type == EventType.MouseDown)
        {

            overNode.callEditor = false;
            switch (overNode.typeOfQuest)
            {
                case QuestNodeType.Dialog:
                    Debug.Log("Call Dialog Details Editor");
                    DialogDetailsEditor.CallEditor(overNode);
                    break;
                case QuestNodeType.Quest:
                    Debug.Log("Call Quest Details Editor");
                    QuestDetailsEditor.CallEditor(overNode);
                    break;
                default:
                    break;
            }
        }
    }
}
