using Codice.CM.WorkspaceServer.DataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestStepEditor : EditorWindow
{
    public static SO_Quests quests = null;
    private SO_NPCList so_NPCS = null;
    private static SO_QuestStartDetails questStartDetails = null;
    private static SO_QuestEndDetails questEndDetails = null;
    private static SO_ObjectiveCollect collectDetails = null;
    private static SO_ObjectiveCourier courierDetails = null;
    private static SO_ObjectiveTask taskDetails = null;
    private static SO_QuestDialogResults resultsDetail = null;
    private static CurrentWorkingNode currentNode = CurrentWorkingNode.none;

    private Dictionary<string, string> npcStarters = new Dictionary<string, string>();
    private Dictionary<string, string> sceneItemStarters = new Dictionary<string, string>();


    [MenuItem("Quest Step Editor", menuItem = "Tools/Quest Editor/Quest Step Editor")]
    public static void OpenWindow()
    {
        QuestStepEditor window = GetWindow<QuestStepEditor>($"Quest Step Editor: {currentNode.ToString()}");
        Vector2 maxSize = new Vector2(1600, 1080);
        Vector2 minsize = new Vector2(640, 480);

        window.minSize = minsize;
        window.maxSize = maxSize;
        window.Show();

    }

    private void OnEnable()
    {
        so_NPCS = GameResources.Instance.npcList;
        BuildDictionaries();

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

    private void OnGUI()
    {
        GUILayout.Label($"Passed Node Information:");
        if (questStartDetails != null)
        {
            GUILayout.Label($"{questStartDetails.questStartID.ToString()}");
        }
        else if (questEndDetails != null)
        {
            GUILayout.Label($"{questEndDetails.questEndID.ToString()}");
        }
    }

    public static bool CallEditor(SO_Quests quest, string nodeToEdit, CurrentWorkingNode nodeType)
    {
        questStartDetails = null;
        questEndDetails = null;
        collectDetails = null;
        courierDetails = null;
        taskDetails = null;
        resultsDetail = null;
        if (quest != null && nodeToEdit != "" && nodeType != CurrentWorkingNode.none)
        {
            quests = quest;
            switch (nodeType)
            {
                case CurrentWorkingNode.QuestStart:
                    questStartDetails = quests.GetStartNodeByID(nodeToEdit);
                    currentNode = nodeType;
                    break;
                case CurrentWorkingNode.QuestEnd:
                    questEndDetails = quests.GetEndNodeByID(nodeToEdit);
                    currentNode = nodeType;
                    break;
                case CurrentWorkingNode.QuestCollect:
                    collectDetails = quests.GetCollectNodeByID(nodeToEdit);
                    currentNode = nodeType;
                    break;
                case CurrentWorkingNode.QuestCourier:
                    courierDetails = quests.GetCourierNodeByID(nodeToEdit);
                    currentNode = nodeType;
                    break;
                case CurrentWorkingNode.QuestTask:
                    taskDetails = quests.GetTaskNodeByID(nodeToEdit);
                    currentNode = nodeType;
                    break;
                case CurrentWorkingNode.QuestDialogResults:
                    resultsDetail=quests.GetResultsNodeByID(nodeToEdit);
                    currentNode = nodeType;
                    break;
            }
            OpenWindow();
            return true;
        }
        return false;
    }
    

}

