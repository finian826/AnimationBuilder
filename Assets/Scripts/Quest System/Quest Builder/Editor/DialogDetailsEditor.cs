using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class DialogDetailsEditor : EditorWindow
{
    //Node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

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

    public static SO_Quests quests;
    private SO_QuestStartDetails questDetails;

    [MenuItem("Dialog Details Editor", menuItem = "Tools/Quest Editor/Dialog Details Editor")]
    public static void OpenWindow()
    {
        GetWindow<DialogDetailsEditor>("Dialog Details Editor");
    }

    private void InspectorSelectionChanged()
    {
        SO_Quests roomNodeGraph = Selection.activeObject as SO_Quests;
        if (roomNodeGraph != null)
        {
            quests = roomNodeGraph;
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

        //subscribe to the inspector selection changed event
        Selection.selectionChanged += InspectorSelectionChanged;

    }

    private void OnDisable()
    {
        //unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void OnGUI()
    {
        // if a scriptable object of type so_RoomNodeGraph has been selected then process
        if (quests != null)
        {
            //draw grid
            DrawBackground(gridSmall, 0.2f, Color.gray);
            DrawBackground(gridLarge, 0.3f, Color.gray);

            //draw line if being dragged
            //DrawDraggedLine();

            //process events
            //ProcessEvents(Event.current);

            //draw connections between room nodes
            //DrawRoomConnections();

            //draw room nodes
            //DrawQuestNodes();
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
        SO_Quests questNode = EditorUtility.EntityIdToObject(instanceID) as SO_Quests;

        if (questNode != null&&questNode.typeOfQuest==QuestType.Dialog)
        {
            OpenWindow();
            quests = questNode;
            return true;
        }
        return false;
    }

    public static bool CallEditor(SO_Quests quest)
    {
        if (quest != null)
        {
            OpenWindow();
            quests = quest;
            return true;
        }
        return false;
    }


}
