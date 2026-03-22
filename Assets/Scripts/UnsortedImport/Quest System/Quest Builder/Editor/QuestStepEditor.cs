using Codice.CM.WorkspaceServer.DataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.VersionControl;
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

    //Node temp variables
    private bool collectFlagSource = false;
    private bool collectFlagDestination = false;
    //Collect Node Globals
    private QuestItemSource newItemSource = new QuestItemSource();
    private QuestItemSource[] collectTempItemSource;
    private bool[] collectItemSourceToggles;
    private QuestItems newItemDestination = new QuestItems();
    private QuestItems[] collectTempItemDestination;
    private bool[] collectItemDesinationToggles;


    private Dictionary<string, string> npcStartersDictionary = new Dictionary<string, string>();
    private Dictionary<string, string> sceneItemStartersDictionary = new Dictionary<string, string>();
    private Dictionary<string, string> masterNPCDictionary = new Dictionary<string, string>();

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
        BuildBools();
    }

    private void BuildBools()
    {
        if (collectDetails != null)
        {
            collectTempItemSource=new QuestItemSource[collectDetails.itemSourceList.Count];
            collectItemSourceToggles=new bool[collectDetails.itemSourceList.Count];
            for (int i = 0; i < collectItemSourceToggles.Length; i++)
            {
                collectItemSourceToggles[i] = false;
            }
            collectTempItemDestination = new QuestItems[collectDetails.itemDestinationList.Count];
            collectItemDesinationToggles=new bool[collectDetails.itemDestinationList.Count];
            for(int i = 0; i < collectDetails.itemDestinationList.Count; i++)
            {
                collectItemDesinationToggles[i] = false;
            }
            newItemSource = new QuestItemSource();
            newItemDestination=new QuestItems();
        }
    }

    private void BuildDictionaries()
    {
        //build lists for drop down boxes based on quest start type
        foreach (NPCList npc in so_NPCS.list)
        {
            switch (npc.startingType)
            {
                case QuestGiver.SceneItem:
                    sceneItemStartersDictionary.Add(npc.NPCid, npc.NPCName);
                    break;
                case QuestGiver.NPC:
                    npcStartersDictionary.Add(npc.NPCid, npc.NPCName);
                    break;
                default:
                    break;
            }
            masterNPCDictionary.Add(npc.NPCid, npc.NPCName);
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
        EditorGUI.BeginChangeCheck();
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
        if (questStartDetails.questTitle == null)
        {
            questStartDetails.questTitle = "";
        }
        questStartDetails.questTitle = GUILayout.TextField(questStartDetails.questTitle, 32);
        GUILayout.Space(_space);
        GUILayout.Label("Quest Text Body");
        if(questStartDetails.questText == null)
        {
            questStartDetails.questText = "";
        }
        questStartDetails.questText = GUILayout.TextArea(questStartDetails.questText, 640);
        GUILayout.Space(_space);
        questStartDetails.objectiveType=(QuestObjectiveType)EditorGUILayout.EnumPopup("Quest Objective Types: ",
            questStartDetails.objectiveType);
        GUILayout.Space(_space);
        GUILayout.Label("Quest Tracker Title");
        if(questStartDetails.trackerTitle == null)
        {
            questStartDetails.trackerTitle = "";
        }
        questStartDetails.trackerTitle = GUILayout.TextField(questStartDetails.trackerTitle, 32);
        GUILayout.Space(_space);
        GUILayout.Label("Quest Tracker Text");
        if (questStartDetails.trackerText == null)
        {
            questStartDetails.trackerText = "";
        }
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
                    questStartDetails.initialQuestDialog.actorID = BuildPopupElement(npcStartersDictionary, questStartDetails.initialQuestDialog.actorID);
                    break;
                case DialogActor.SceneItem:
                    GUILayout.Label("Please select scene item to start dialog:");
                    questStartDetails.initialQuestDialog.actorID = BuildPopupElement(sceneItemStartersDictionary, questStartDetails.initialQuestDialog.actorID);

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
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(questStartDetails);
    }

    public void ViewEditEndDetails()
    {
        EditorGUI.BeginChangeCheck();
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
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(questEndDetails);

    }

    public void ViewEditCollectDetails()
    {
        EditorGUI.BeginChangeCheck();
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
        collectDetails.taskNPC = BuildPopupElement(npcStartersDictionary, collectDetails.taskNPC);
        GUILayout.Space(_space);
        GUILayout.Label("Quest Text Body");
        collectDetails.taskNPCDialog = GUILayout.TextArea(collectDetails.taskNPCDialog, 640);
        GUILayout.Space(_space);
        GUILayout.Label("Required Quest Items from Source");
        if (collectDetails.itemSourceList.Count == 0)
        {
            GUILayout.Label("No Quest Items Defined");
        }
        else
        {
            int itemIndex = 0;
            foreach (QuestItemSource items in collectDetails.itemSourceList)
            {
                EditorGUILayout.BeginHorizontal();
                collectTempItemSource[itemIndex] = items;
                GUILayout.Label("Source NPC", GUILayout.Width(96));
                collectTempItemSource[itemIndex].sourceNPCID = BuildPopupElement(masterNPCDictionary, collectTempItemSource[itemIndex].sourceNPCID);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Item", GUILayout.Width(96));//need to create drop down for here based on item codes for quest items
                collectTempItemSource[itemIndex].itemCode = EditorGUILayout.IntField(collectTempItemSource[itemIndex].itemCode);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Minimum Drop", GUILayout.Width(96));
                collectTempItemSource[itemIndex].minDrop = EditorGUILayout.IntField(collectTempItemSource[itemIndex].minDrop);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Maximum Drop", GUILayout.Width(96));
                collectTempItemSource[itemIndex].maxDrop = EditorGUILayout.IntField(collectTempItemSource[itemIndex].maxDrop);
                EditorGUILayout.EndHorizontal();
                collectItemSourceToggles[itemIndex] = EditorGUILayout.Toggle("Delete", collectItemSourceToggles[itemIndex]);
                itemIndex++;
            }
            if (GUILayout.Button("Delete Selected"))
            {
                for (int i = 0; i < collectItemSourceToggles.Length; i++)
                {
                    if (collectItemSourceToggles[i] == true)
                    {
                        collectDetails.itemSourceList.Remove(collectTempItemSource[i]);
                    }
                }
                BuildBools();
                GUI.changed = true;
            }
        }
        if (GUILayout.Button("Add new Quest Source") || collectFlagSource)
        {
            collectFlagSource = true;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Source NPC", GUILayout.Width(96));
            newItemSource.sourceNPCID = BuildPopupElement(masterNPCDictionary, newItemSource.sourceNPCID);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Item", GUILayout.Width(96));//need to create drop down for here based on item codes for quest items
            newItemSource.itemCode = EditorGUILayout.IntField(newItemSource.itemCode);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Minimum Drop", GUILayout.Width(96));
            newItemSource.minDrop = EditorGUILayout.IntField(newItemSource.minDrop);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Maximum Drop", GUILayout.Width(96));
            newItemSource.maxDrop = EditorGUILayout.IntField(newItemSource.maxDrop); 
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Add Source"))
            {
                collectDetails.itemSourceList.Add(newItemSource);
                GUI.changed = true;
                collectFlagSource = false;
                BuildBools();
            }
        }
        //work down from here
        GUILayout.Space(_space);
        GUILayout.Label("Quest Item Destinations");
        if (collectDetails.itemDestinationList.Count == 0)
        {
            GUILayout.Label("No Quest Item Destinations Defined");
        }
        else
        {
            int itemIndex = 0;
            foreach (QuestItems items in collectDetails.itemDestinationList)
            {
                collectTempItemDestination[itemIndex] = items;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Item", GUILayout.Width(96));//need to create drop down for here based on item codes for quest items
                collectTempItemDestination[itemIndex].questItem = EditorGUILayout.IntField(collectTempItemDestination[itemIndex].questItem);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Qunatity", GUILayout.Width(96));
                collectTempItemDestination[itemIndex].questItemQuantity = EditorGUILayout.IntField(collectTempItemDestination[itemIndex].questItemQuantity);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Target NPC", GUILayout.Width(96));
                collectTempItemDestination[itemIndex].targetNPC = BuildPopupElement(masterNPCDictionary, collectTempItemDestination[itemIndex].targetNPC);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Tracker Detail", GUILayout.Width(96));
                collectTempItemDestination[itemIndex].trackerStepDecscription = GUILayout.TextField(collectTempItemDestination[itemIndex].trackerStepDecscription,32);
                EditorGUILayout.EndHorizontal();
                collectItemDesinationToggles[itemIndex] = EditorGUILayout.Toggle("Delete", collectItemDesinationToggles[itemIndex]);
                itemIndex++;
            }
            if (GUILayout.Button("Delete Selected"))
            {
                for (int i = 0; i < collectItemDesinationToggles.Length; i++)
                {
                    if (collectItemDesinationToggles[i] == true)
                    {
                        collectDetails.itemDestinationList.Remove(collectTempItemDestination[i]);
                    }
                }
                BuildBools();
                GUI.changed = true;
            }
        }
        if (GUILayout.Button("Add new Quest Destination") || collectFlagDestination)
        {
            collectFlagDestination = true;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Item", GUILayout.Width(96));//need to create drop down for here based on item codes for quest items
            newItemDestination.questItem = EditorGUILayout.IntField(newItemDestination.questItem);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Qunatity", GUILayout.Width(96));
            newItemDestination.questItemQuantity = EditorGUILayout.IntField(newItemDestination.questItemQuantity);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Target NPC", GUILayout.Width(96));
            newItemDestination.targetNPC = BuildPopupElement(masterNPCDictionary, newItemDestination.targetNPC);
            EditorGUILayout.EndHorizontal();
            if (newItemDestination.trackerStepDecscription == null)
            {
                newItemDestination.trackerStepDecscription = "";
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Tracker Detail", GUILayout.Width(96));
            newItemDestination.trackerStepDecscription = GUILayout.TextField(newItemDestination.trackerStepDecscription, 32);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add Destination"))
            {
                collectDetails.itemDestinationList.Add(newItemDestination);
                GUI.changed = true;
                collectFlagDestination = false;
                BuildBools();
            }
        }
        GUI.changed = false;

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(collectDetails);
    }

    public void ViewEditCourierDetails()
    {
        EditorGUI.BeginChangeCheck();
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

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(courierDetails);

    }

    public void ViewEditTaskDetails()
    {
        EditorGUI.BeginChangeCheck();
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

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(taskDetails);

    }

    public void ViewEditResultsDetails()
    {
        EditorGUI.BeginChangeCheck();
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

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(resultsDetail);

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

