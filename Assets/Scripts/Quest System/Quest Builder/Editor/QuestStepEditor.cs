using Codice.CM.WorkspaceServer.DataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using static TreeEditor.TreeEditorHelper;

public class QuestStepEditor : EditorWindow
{
    private float _space = 5f;

    public static SO_Quests quests = null;
    private SO_NPCList so_NPCS = null;
    private static SO_QuestStartDetails questStartDetails = null;
    private static SO_QuestEndDetails questEndDetails = null;
    private static SO_ObjectiveQuestCollect collectDetails = null;
    private static SO_ObjectiveQuestCourier courierDetails = null;
    private static SO_ObjectiveQuestTask taskDetails = null;
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
        switch (currentNode)
        {
            case CurrentWorkingNode.QuestStart:
                if (questStartDetails != null)
                {
                    ViewEditStartDetails();
                }
                break;
            case CurrentWorkingNode.QuestEnd:
                if (questEndDetails != null)
                {
                    ViewEditEndDetails();
                }
                break;
            case CurrentWorkingNode.QuestCollect:
                if (collectDetails != null)
                {
                    ViewEditCollectDetails();
                }
                break;
            case CurrentWorkingNode.QuestCourier:
                if (courierDetails != null)
                {
                    ViewEditCourierDetails();
                }
                break;
            case CurrentWorkingNode.QuestTask:
                if (taskDetails != null)
                {
                    ViewEditTaskDetails();
                }
                break;
            case CurrentWorkingNode.QuestDialogResults:
                if (resultsDetail != null)
                {
                    ViewEditResultsDetails();
                }
                break;

        }
    }
    public void ViewEditStartDetails()
    {
        GUILayout.Label("Node ID");
        GUILayout.Label($"{questStartDetails.questStartID.ToString()}");
        GUILayout.Space(_space);
        GUILayout.Label("Children ID's and Step Types");
        if(questStartDetails.childQuestStepIDList.Count > 0)
        {
            foreach (string childID in questStartDetails.childQuestStepIDList)
            {
                GUILayout.Label($"{childID.ToString()}\t{quests.GetStepNodeType(childID).ToString()}");
            }
        }
        GUILayout.Space(_space);
        GUILayout.Label("Quest Title");
        questStartDetails.questTitle = GUILayout.TextField(questStartDetails.questTitle, 32);
        GUILayout.Space(_space);
        GUILayout.Label("Quest Text Body");
        questStartDetails.questText = GUILayout.TextArea(questStartDetails.questText, 640);
        GUILayout.Space(_space);
        questStartDetails.objectiveType=(QuestObjectiveType)EditorGUILayout.EnumPopup("Quest Objective Types: ",
            questStartDetails.objectiveType);
        GUILayout.Space(_space);
        GUILayout.Label("Quest Tracker Title");
        questStartDetails.trackerTitle = GUILayout.TextField(questStartDetails.trackerTitle, 32);
        GUILayout.Space(_space);
        GUILayout.Label("Quest Tracker Text");
        questStartDetails.trackerText = GUILayout.TextArea(questStartDetails.trackerText, 64);
        GUILayout.Space(_space);
        questStartDetails.initialDialogBeforeQuest = EditorGUILayout.ToggleLeft("Initial Dialog Before Quest",
            questStartDetails.initialDialogBeforeQuest);
        if (questStartDetails.initialDialogBeforeQuest)
        {
            GUILayout.Label("Initial Dialog Details");
            DialogActor oldActor = questStartDetails.initialQuestDialog.actorDialog;
            questStartDetails.initialQuestDialog.actorDialog = (DialogActor)EditorGUILayout.EnumPopup("Dialog Actor: ", questStartDetails.initialQuestDialog.actorDialog);
            if (oldActor != questStartDetails.initialQuestDialog.actorDialog)
            {
                questStartDetails.initialQuestDialog.actorID = "";
            }
            GUILayout.Space(_space);
            switch (questStartDetails.initialQuestDialog.actorDialog)
            {
                case DialogActor.NPC:
                    GUILayout.Label("Please select NPC to give dialog:");
                    questStartDetails.initialQuestDialog.actorID = BuildPopupElement(npcStarters, questStartDetails.initialQuestDialog.actorID);
                    break;
                case DialogActor.SceneItem:
                    GUILayout.Label("Please select scene item to start dialog:");
                    questStartDetails.initialQuestDialog.actorID = BuildPopupElement(sceneItemStarters, questStartDetails.initialQuestDialog.actorID);

                    break;
                case DialogActor.EventTrigger:
                    GUILayout.Label("Please enter Quest Event Trigger:");
                    questStartDetails.initialQuestDialog.actorID = EditorGUILayout.TextField("", questStartDetails.initialQuestDialog.actorID);
                    break;
                case DialogActor.Player:
                    questStartDetails.initialQuestDialog.actorID = "self";
                    break;
                default:
                    break;
            }
            GUILayout.Space(_space);
            GUILayout.Label("Actor Dialog Text");
            questStartDetails.initialQuestDialog.actorText = GUILayout.TextArea(questStartDetails.initialQuestDialog.actorText, 320);
            GUILayout.Label("Actor Portriat ID (Not Implimented Yet)");
            questStartDetails.initialQuestDialog.actorPortiatID = GUILayout.TextField(questStartDetails.initialQuestDialog.actorPortiatID, 64);
            GUILayout.Space(_space);
            questStartDetails.initialQuestDialog.waitTimeBeforeContinuing = EditorGUILayout.FloatField("Wait Time Before Continuing",
                questStartDetails.initialQuestDialog.waitTimeBeforeContinuing);

        }
    }

    public void ViewEditEndDetails()
    {
        GUILayout.Label("Node ID");
        GUILayout.Label($"{questEndDetails.questEndID.ToString()}");
        GUILayout.Space(_space);
        GUILayout.Label("Parent ID's and Step Types");
        if (questEndDetails.parentQuestStepIDList.Count > 0)
        {
            foreach (string parentID in questEndDetails.parentQuestStepIDList)
            {
                GUILayout.Label($"{parentID.ToString()}\t{quests.GetStepNodeType(parentID).ToString()}");
            }
        }
        GUILayout.Space(_space);
        GUILayout.Label("Children ID's and Step Types");
        if (questEndDetails.childQuestStepIDList.Count > 0)
        {
            foreach (string childID in questEndDetails.childQuestStepIDList)
            {
                GUILayout.Label($"{childID.ToString()}\t{quests.GetStepNodeType(childID).ToString()}");
            }
        }
        GUILayout.Space(_space);
        GUILayout.Label("Quest Finished Text");
        questEndDetails.npcCompleteText = GUILayout.TextArea(questEndDetails.npcCompleteText, 320);

    }

    public void ViewEditCollectDetails()
    {
        GUILayout.Label("Node ID");
        GUILayout.Label($"{collectDetails.collectQuestStepID.ToString()}");
        GUILayout.Space(_space);
        GUILayout.Label("Parent ID's and Step Types");
        if (collectDetails.parentQuestStepIDList.Count > 0)
        {
            foreach (string parentID in collectDetails.parentQuestStepIDList)
            {
                GUILayout.Label($"{parentID.ToString()}\t{quests.GetStepNodeType(parentID).ToString()}");
            }
        }
        GUILayout.Space(_space);
        GUILayout.Label("Children ID's and Step Types");
        if (collectDetails.childQuestStepIDList.Count > 0)
        {
            foreach (string childID in collectDetails.childQuestStepIDList)
            {
                GUILayout.Label($"{childID.ToString()}\t{quests.GetStepNodeType(childID).ToString()}");
            }
        }
        GUILayout.Space(_space);
        GUILayout.Label("Please select NPC to give dialog:");
        collectDetails.taskNPC = BuildPopupElement(npcStarters, collectDetails.taskNPC);
        GUILayout.Space(_space);
        GUILayout.Label("Quest Text Body");
        collectDetails.taskNPCDialog = GUILayout.TextArea(collectDetails.taskNPCDialog, 640);

    }

    public void ViewEditCourierDetails()
    {
        GUILayout.Label("Node ID");
        GUILayout.Label($"{courierDetails.courierQuestStepID.ToString()}");
        GUILayout.Space(_space);
        GUILayout.Label("Parent ID's and Step Types");
        if (courierDetails.parentQuestStepIDList.Count > 0)
        {
            foreach (string parentID in courierDetails.parentQuestStepIDList)
            {
                GUILayout.Label($"{parentID.ToString()}\t{quests.GetStepNodeType(parentID).ToString()}");
            }
        }
        GUILayout.Space(_space);
        GUILayout.Label("Children ID's and Step Types");
        if (courierDetails.childQuestStepIDList.Count > 0)
        {
            foreach (string childID in courierDetails.childQuestStepIDList)
            {
                GUILayout.Label($"{childID.ToString()}\t{quests.GetStepNodeType(childID).ToString()}");
            }
        }
        GUILayout.Space(_space);

    }

    public void ViewEditTaskDetails()
    {
        GUILayout.Label("Node ID");
        GUILayout.Label($"{taskDetails.taskQuestStepID.ToString()}");
        GUILayout.Space(_space);
        GUILayout.Label("Parent ID's and Step Types");
        if (taskDetails.parentQuestStepIDList.Count > 0)
        {
            foreach (string parentID in taskDetails.parentQuestStepIDList)
            {
                GUILayout.Label($"{parentID.ToString()}\t{quests.GetStepNodeType(parentID).ToString()}");
            }
        }
        GUILayout.Space(_space);
        GUILayout.Label("Children ID's and Step Types");
        if (taskDetails.childQuestStepIDList.Count > 0)
        {
            foreach (string childID in taskDetails.childQuestStepIDList)
            {
                GUILayout.Label($"{childID.ToString()}\t{quests.GetStepNodeType(childID).ToString()}");
            }
        }
        GUILayout.Space(_space);

    }

    public void ViewEditResultsDetails()
    {
        GUILayout.Label("Node ID");
        GUILayout.Label($"{resultsDetail.questDialogResultsStepID.ToString()}");
        GUILayout.Space(_space);
        GUILayout.Label("Parent ID's and Step Types");
        if (resultsDetail.parentQuestStepIDList.Count > 0)
        {
            foreach (string parentID in resultsDetail.parentQuestStepIDList)
            {
                GUILayout.Label($"{parentID.ToString()}\t{quests.GetStepNodeType(parentID).ToString()}");
            }
        }
        GUILayout.Space(_space);

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

