using UnityEngine;
using System.Collections.Generic;

public class QuestTrackerDetails
{
    public string questID;
    public string questName;
    public QuestNodeType trackedQuestType = QuestNodeType.none;
    public string questShortDesc;
    public List<QuestTaskList> taskList=new List<QuestTaskList>();
    public string questFinishDesc;
}
