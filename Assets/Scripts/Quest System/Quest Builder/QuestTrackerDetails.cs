using UnityEngine;
using System.Collections.Generic;

public class QuestTaskList
{
    public string task;
    public int collectedItems;
    public int maxItems;
}

public class QuestTrackerDetails
{
    public string questID;
    public string questName;
    public QuestType trackedQuestType = QuestType.none;
    public string questShortDesc;
    public List<QuestTaskList> taskList=new List<QuestTaskList>();
    public string questFinishDesc;
}
